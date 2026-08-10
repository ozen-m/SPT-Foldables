using System.Text;
using Foldables.Models;
using Foldables.Utils;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;

namespace Foldables;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 100000)] // Process vanilla and mod items
public class Foldables(
    FoldablesLogger L,
    FoldablesConfig config,
    FoldablesLocales locales,
    ItemHelper itemHelper,
    TemplateTable templateTable,
    LocaleTable localeTable,
    ServerLocalisationService serverLocalisationService,
    IEnumerable<IRuntimePatch> patches
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken token)
    {
        CommonExtensions.SetServerLocalisationService(serverLocalisationService);

        ProcessLocales();
        ValidateConfig();

        var items = templateTable.Items;

        var backpacksItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.BACKPACK) && GetIsFoldable(i.Id));
        AddFoldableProperties(backpacksItemTemplates, BaseClasses.BACKPACK);

        var vestsItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.VEST) && GetIsFoldable(i.Id) && !i.Properties!.Slots!.Any());
        AddFoldableProperties(vestsItemTemplates, BaseClasses.VEST);

        var headphonesItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.HEADPHONES) && GetIsFoldable(i.Id));
        AddFoldableProperties(headphonesItemTemplates, BaseClasses.HEADPHONES);

        foreach (var patch in patches)
        {
            patch.Enable();
        }

        L.Success("load-success".Localized());
        return Task.CompletedTask;
    }

    private void ProcessLocales()
    {
        if (locales.Count < 1)
        {
            L.Error("No locale files found!");
            return;
        }

        foreach (var (lang, lazyLoadLocale) in localeTable.Global)
        {
            if (locales.TryGetValue(lang, out var locale))
            {
                lazyLoadLocale.AddTransformer((localeData) =>
                {
                    foreach (var (key, value) in locale)
                    {
                        localeData.TryAdd(key, value);
                    }

                    return localeData;
                });
            }
            else
            {
                // We don't have a locale file for the current language, use english
                lazyLoadLocale.AddTransformer((localeData) =>
                {
                    foreach (var (key, value) in locales["en"])
                    {
                        localeData.TryAdd(key, value);
                    }

                    return localeData;
                });
            }
        }
    }

    private void ValidateConfig()
    {
        // Folding times
        config.MinFoldingTime = Math.Max(config.MinFoldingTime, 0d);
        config.MaxFoldingTime = Math.Max(config.MinFoldingTime, config.MaxFoldingTime);

        // Folded cell sizes
        config.BackpackFoldedCellSizes = [.. config.BackpackFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        config.VestFoldedCellSizes = [.. config.VestFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        config.HeadphonesFoldedCellSizes = [.. config.HeadphonesFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!config.BackpackFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `{nameof(FoldablesConfig.BackpackFoldedCellSizes)}`");
        }
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!config.VestFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `{nameof(FoldablesConfig.VestFoldedCellSizes)}`");
        }
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!config.HeadphonesFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `{nameof(FoldablesConfig.HeadphonesFoldedCellSizes)}`");
        }

        // Unknown/missing properties
        var sb = new StringBuilder();
        if (config.ExtensionData.Count > 0)
        {
            sb.Append("Found unknown fields under config.json:");
            foreach (var obj in config.ExtensionData)
            {
                sb.Append(' ').Append(obj.ToString());
            }
        }
        foreach (var (key, value) in config.Overrides)
        {
            if (value.ExtensionData.Count > 0)
            {
                sb.Append("; Found unknown fields under Overrides for item ").Append(key).Append(':');
                foreach (var obj in value.ExtensionData)
                {
                    sb.Append(' ').Append(obj.ToString());
                }
            }
            if (value.ItemSize?.ExtensionData.Count > 0)
            {
                sb.Append("; Found unknown fields under FoldedSize for item ").Append(key).Append(':');
                foreach (var obj in value.ItemSize.ExtensionData)
                {
                    sb.Append(' ').Append(obj.ToString());
                }
            }
            if (value is { Foldable: true, ItemSize: null, FoldingTime: null })
            {
                L.Warning("missing-override-properties".Localized(key.ToString()));
            }
        }
        if (sb.Length > 0)
        {
            L.Warning(sb.ToString());
        }
    }

    private void AddFoldableProperties(IEnumerable<TemplateItem> templates, MongoId baseClass)
    {
        TemplateItem[] itemTemplates = [.. templates];
        var (minSlotCount, maxSlotCount) = GetMinMaxSlotCount(itemTemplates.Select(i => i.Properties));

        var lessCounter = 0;
        itemTemplates
           .AsParallel()
           .ForAll(itemTemplate => ProcessTemplate(itemTemplate, baseClass, minSlotCount, maxSlotCount, ref lessCounter));

        var updatedCount = itemTemplates.Length - lessCounter;
        if (baseClass == BaseClasses.BACKPACK)
        {
            L.Info("added-backpacks".Localized(updatedCount));
        }
        else if (baseClass == BaseClasses.VEST)
        {
            L.Info("added-vests".Localized(updatedCount));
        }
        else if (baseClass == BaseClasses.HEADPHONES)
        {
            L.Info("added-headphones".Localized(updatedCount));
        }
    }

    private void ProcessTemplate(TemplateItem itemTemplate, MongoId baseClass, int minSlotCount, int maxSlotCount, ref int lessCounter)
    {
        var itemProperties = itemTemplate.Properties ?? new TemplateItemProperties();
        var slotCount = GetSlotCount(itemProperties);
        var reduceCellSize = GetReduceCellSize(itemTemplate.Id, slotCount, itemProperties, baseClass);
        if (reduceCellSize is null)
        {
            // Current size and folded size is the same, skip
            Interlocked.Increment(ref lessCounter);
            L.Debug("set-properties-skip".Localized(new { name = itemTemplate.Name, id = itemTemplate.Id }));
            return;
        }
        var foldingTime = GetFoldingTime(itemTemplate.Id, slotCount, minSlotCount, maxSlotCount, baseClass);

        itemProperties.Foldable = true;
        itemProperties.SizeReduceRight = reduceCellSize.Width;
        itemProperties.ExtensionData!["SizeReduceDown"] = reduceCellSize.Height;
        itemProperties.ExtensionData!["FoldingTime"] = foldingTime;

        L.Debug("set-properties".Localized(new
        {
            name = itemTemplate.Name,
            id = itemTemplate.Id,
            size = GetCellSize(itemProperties),
            time = foldingTime
        }));
    }

    private bool GetIsFoldable(MongoId itemId)
    {
        return !config.Overrides.TryGetValue(itemId, out var overrideProperties) || overrideProperties.Foldable;
    }

    private double GetFoldingTime(MongoId itemId, int gridCount, int minGridCount, int maxGridCount, MongoId baseClass)
    {
        if (config.Overrides.TryGetValue(itemId, out var overrideProperties) && overrideProperties.FoldingTime.HasValue)
        {
            return overrideProperties.FoldingTime.Value;
        }
        if (config.Overrides.TryGetValue(baseClass, out var baseClassOverrideProperties) && baseClassOverrideProperties.FoldingTime.HasValue)
        {
            return baseClassOverrideProperties.FoldingTime.Value;
        }

        var minFoldTime = config.MinFoldingTime;
        var maxFoldTime = config.MaxFoldingTime;
        // ReSharper disable once CompareOfFloatsByEqualityOperator, folding time is "disabled"
        if (minFoldTime == maxFoldTime)
        {
            return maxFoldTime;
        }

        var scale = (double)(gridCount - minGridCount) / (maxGridCount - minGridCount);
        return Math.Round(minFoldTime + (maxFoldTime - minFoldTime) * scale, 2);
    }

    /// <summary>
    /// Calculate the ReduceSize width and height properties for a given slotCount
    /// </summary>
    /// <param name="slotCount">Can be either the grid count or cell size of the item</param>
    /// <returns>null if slotCount is the same as the resulting folded size</returns>
    [CanBeNull]
    private ItemSize GetReduceCellSize(MongoId itemId, int slotCount, TemplateItemProperties properties, MongoId baseClass)
    {
        ItemSize foldedCellSize;
        if (config.Overrides.TryGetValue(itemId, out var overrideProperties) && overrideProperties.ItemSize != null)
        {
            foldedCellSize = overrideProperties.ItemSize;
        }
        else if (config.Overrides.TryGetValue(baseClass, out var baseClassOverride) && baseClassOverride.ItemSize != null)
        {
            foldedCellSize = baseClassOverride.ItemSize;
        }
        else
        {
            foldedCellSize = GetFoldedCellSize(slotCount, baseClass);
        }

        if (slotCount == foldedCellSize.GetArea())
        {
            return null;
        }

        if (properties.Width > properties.Height)
        {
            foldedCellSize = foldedCellSize.Swap();
        }
        return new ItemSize
        {
            Width = properties.Width!.Value - foldedCellSize.Width,
            Height = properties.Height!.Value - foldedCellSize.Height,
        };
    }

    private ItemSize GetFoldedCellSize(int gridCount, MongoId baseClass)
    {
        CellSizeRange[] foldedCellSizes;
        if (baseClass == BaseClasses.BACKPACK)
        {
            foldedCellSizes = config.BackpackFoldedCellSizes;
        }
        else if (baseClass == BaseClasses.VEST)
        {
            foldedCellSizes = config.VestFoldedCellSizes;
        }
        else if (baseClass == BaseClasses.HEADPHONES)
        {
            foldedCellSizes = config.HeadphonesFoldedCellSizes;
        }
        else
        {
            throw new ArgumentException($"Cannot get folded cell size for unrecognized base class: {baseClass}");
        }

        foreach (var cellSizeRange in foldedCellSizes)
        {
            if (gridCount <= cellSizeRange.MaxGridCount)
            {
                return cellSizeRange.CellSize;
            }
        }
        return foldedCellSizes[0].CellSize;
    }

    private static (int min, int max) GetMinMaxSlotCount(IEnumerable<TemplateItemProperties> itemsProperties)
    {
        var min = int.MaxValue;
        var max = int.MinValue;
        foreach (var itemProperties in itemsProperties)
        {
            var slotCount = GetSlotCount(itemProperties);
            if (slotCount < min)
            {
                min = slotCount;
            }
            if (slotCount > max)
            {
                max = slotCount;
            }
        }

        return (min, max);
    }

    /// <returns>The grid count of an item, or cell size if the item has no grids</returns>
    public static int GetSlotCount(TemplateItemProperties properties)
    {
        if (properties.Grids != null && properties.Grids.Any())
        {
            return properties.Grids!.Sum(g => g.Properties!.CellsH * g.Properties.CellsV).GetValueOrDefault();
        }

        return properties.Width.GetValueOrDefault() * properties.Height.GetValueOrDefault();
    }

    public static ItemSize GetCellSize(TemplateItemProperties properties)
    {
        return new ItemSize
        {
            Width = properties.Width.GetValueOrDefault() - properties.SizeReduceRight.GetValueOrDefault(),
            Height = properties.Height.GetValueOrDefault() - (int)(properties.ExtensionData!.GetValueOrDefault("SizeReduceDown") ?? 0),
        };
    }
}

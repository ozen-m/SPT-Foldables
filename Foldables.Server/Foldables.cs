using System.Reflection;
using System.Text;
using Foldables.Models;
using Foldables.Patches;
using Foldables.Utils;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;

namespace Foldables;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)] // Process vanilla and mod items
public class Foldables(
    ISptLogger<Foldables> logger,
    ModHelper modHelper,
    CustomJsonUtil customJsonUtil,
    ItemHelper itemHelper,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService
) : IOnLoad
{
    public static ModConfig ModConfig { get; private set; } = new();

    public Task OnLoad()
    {
        CommonUtils.Logger = logger;
        CommonUtils.ServerLocalisationService = serverLocalisationService;

        string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        string configPath = Path.Combine(modPath, "config");
        LoadConfig(Path.Combine(configPath, "config.json"));
        LoadLocales(Path.Combine(configPath, "locales"));
        ValidateConfig();

        Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();

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

        new GetSizePatch().Enable();

        CommonUtils.LogSuccess("load-success".Localized());
        return Task.CompletedTask;
    }

    private void LoadConfig(string configFilePath)
    {
        try
        {
            ModConfig = customJsonUtil.DeserializeFromFile<ModConfig>(configFilePath, true);
        }
        catch (Exception ex)
        {
            CommonUtils.LogError(ex.ToString());
            CommonUtils.LogError("Exception while trying to load configuration file, using default values. Misconfigured config.json?");
        }
    }

    private void LoadLocales(string localesPath)
    {
        Dictionary<string, Dictionary<string, string>> locales = [];
        if (!Directory.Exists(localesPath))
        {
            throw new FileNotFoundException($"Missing locales directory: {localesPath}");
        }
        try
        {
            string[] localeFiles = Directory.GetFiles(localesPath, "*.json");
            foreach (var localeFile in localeFiles)
            {
                string language = Path.GetFileNameWithoutExtension(localeFile);
                locales[language] = customJsonUtil.DeserializeFromFile<Dictionary<string, string>>(localeFile);
            }
        }
        catch (Exception ex)
        {
            CommonUtils.LogError(ex.ToString());
            CommonUtils.LogError("Exception while trying to load locales");
        }
        if (locales.Count < 1)
        {
            CommonUtils.LogError($"No locale files found under: {localesPath}");
            return;
        }
        
        foreach (var (lang, lazyLoadLocale) in databaseService.GetLocales().Global)
        {
            if (locales.TryGetValue(lang, out Dictionary<string, string> locale))
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

    private static void ValidateConfig()
    {
        // Folding times
        ModConfig.MinFoldingTime = Math.Max(ModConfig.MinFoldingTime, 0d);
        ModConfig.MaxFoldingTime = Math.Max(ModConfig.MinFoldingTime, ModConfig.MaxFoldingTime);

        // Folded cell sizes
        ModConfig.BackpackFoldedCellSizes = [.. ModConfig.BackpackFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        ModConfig.VestFoldedCellSizes = [.. ModConfig.VestFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        ModConfig.HeadphonesFoldedCellSizes = [.. ModConfig.HeadphonesFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!ModConfig.BackpackFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `BackpackFoldedCellSizes`");
        }
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!ModConfig.VestFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `VestFoldedCellSizes`");
        }
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!ModConfig.HeadphonesFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `HeadphonesFoldedCellSizes`");
        }

        // Unknown/missing properties
        var sb = new StringBuilder();
        if (ModConfig.ExtensionData.Count > 0)
        {
            sb.Append("Found unknown fields under config.json:");
            foreach (var obj in ModConfig.ExtensionData)
            {
                sb.Append(' ').Append(obj.ToString());
            }
        }
        foreach (var (key, value) in ModConfig.Overrides)
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
                CommonUtils.LogWarning("missing-override-properties".Localized(key.ToString()));
            }
        }
        if (sb.Length > 0)
        {
            CommonUtils.LogWarning(sb.ToString());
        }
    }

    private static void AddFoldableProperties(IEnumerable<TemplateItem> templates, MongoId baseClass)
    {
        TemplateItem[] itemTemplates = templates.ToArray();
        var (minSlotCount, maxSlotCount) = GetMinMaxSlotCount(itemTemplates.Select(i => i.Properties));

        var lessCounter = 0;
#if RELEASE
        itemTemplates
            .AsParallel()
            .ForAll(itemTemplate => ProcessTemplate(itemTemplate, baseClass, minSlotCount, maxSlotCount, ref lessCounter));
#else
        // Can't break on the parallel
        foreach (var templateItem in itemTemplates)
        {
            ProcessTemplate(templateItem, baseClass, minSlotCount, maxSlotCount, ref lessCounter);
        }
#endif
        var updatedCount = itemTemplates.Length - lessCounter;
        if (baseClass == BaseClasses.BACKPACK)
            CommonUtils.LogInfo("added-backpacks".Localized(updatedCount));
        else if (baseClass == BaseClasses.VEST)
            CommonUtils.LogInfo("added-vests".Localized(updatedCount));
        else if (baseClass == BaseClasses.HEADPHONES)
            CommonUtils.LogInfo("added-headphones".Localized(updatedCount));
    }

    private static void ProcessTemplate(TemplateItem itemTemplate, MongoId baseClass, int minSlotCount, int maxSlotCount, ref int lessCounter)
    {
        var itemProperties = itemTemplate.Properties ?? new TemplateItemProperties();
        int slotCount = GetSlotCount(itemProperties);
        ItemSize reduceCellSize = GetReduceCellSize(itemTemplate.Id, slotCount, itemProperties, baseClass);
        if (reduceCellSize is null)
        {
            // Current size and folded size is the same, skip
            Interlocked.Increment(ref lessCounter);
            CommonUtils.LogDebug("set-properties-skip".Localized(new { name = itemTemplate.Name, id = itemTemplate.Id }));
            return;
        }
        double foldingTime = GetFoldingTime(itemTemplate.Id, slotCount, minSlotCount, maxSlotCount, baseClass);

        itemProperties.Foldable = true;
        itemProperties.SizeReduceRight = reduceCellSize.Width;
        itemProperties.ExtensionData!["SizeReduceDown"] = reduceCellSize.Height;
        itemProperties.ExtensionData!["FoldingTime"] = foldingTime;

        CommonUtils.LogDebug("set-properties".Localized(new
        {
            name = itemTemplate.Name,
            id = itemTemplate.Id,
            size = GetCellSize(itemProperties),
            time = foldingTime
        }));
    }

    private static bool GetIsFoldable(MongoId itemId)
    {
        if (ModConfig.Overrides.TryGetValue(itemId, out var overrideProperties))
            return overrideProperties.Foldable;

        return true;
    }

    private static double GetFoldingTime(MongoId itemId, int gridCount, int minGridCount, int maxGridCount, MongoId baseClass)
    {
        if (ModConfig.Overrides.TryGetValue(itemId, out var overrideProperties) && overrideProperties.FoldingTime.HasValue)
            return overrideProperties.FoldingTime.Value;
        if (ModConfig.Overrides.TryGetValue(baseClass, out var baseClassOverrideProperties) && baseClassOverrideProperties.FoldingTime.HasValue)
            return baseClassOverrideProperties.FoldingTime.Value;

        double minFoldTime = ModConfig.MinFoldingTime;
        double maxFoldTime = ModConfig.MaxFoldingTime;
        // ReSharper disable once CompareOfFloatsByEqualityOperator, folding time is "disabled"
        if (minFoldTime == maxFoldTime)
            return maxFoldTime;

        double scale = (double)(gridCount - minGridCount) / (maxGridCount - minGridCount);
        return Math.Round(minFoldTime + (maxFoldTime - minFoldTime) * scale, 2);
    }

    /// <summary>
    /// Calculate the ReduceSize width and height properties for a given slotCount
    /// </summary>
    /// <param name="slotCount">Can be either the grid count or cell size of the item</param>
    /// <returns>null if slotCount is the same as the resulting folded size</returns>
    [CanBeNull]
    private static ItemSize GetReduceCellSize(MongoId itemId, int slotCount, TemplateItemProperties properties, MongoId baseClass)
    {
        ItemSize foldedCellSize;
        if (ModConfig.Overrides.TryGetValue(itemId, out var overrideProperties) && overrideProperties.ItemSize != null)
            foldedCellSize = overrideProperties.ItemSize;
        else if (ModConfig.Overrides.TryGetValue(baseClass, out var baseClassOverrideProperties) && baseClassOverrideProperties.ItemSize != null)
            foldedCellSize = baseClassOverrideProperties.ItemSize;
        else
            foldedCellSize = GetFoldedCellSize(slotCount, baseClass);

        if (slotCount == foldedCellSize.GetArea()) return null;

        if (properties.Width > properties.Height)
        {
            foldedCellSize = foldedCellSize.Swap();
        }
        return new ItemSize
        {
            Width = properties.Width!.Value - foldedCellSize.Width,
            Height = properties.Height!.Value - foldedCellSize.Height
        };
    }

    private static ItemSize GetFoldedCellSize(int gridCount, MongoId baseClass)
    {
        CellSizeRange[] foldedCellSizes;
        if (baseClass == BaseClasses.BACKPACK)
            foldedCellSizes = ModConfig.BackpackFoldedCellSizes;
        else if (baseClass == BaseClasses.VEST)
            foldedCellSizes = ModConfig.VestFoldedCellSizes;
        else if (baseClass == BaseClasses.HEADPHONES)
            foldedCellSizes = ModConfig.HeadphonesFoldedCellSizes;
        else
            throw new ArgumentException($"Cannot get folded cell size for unrecognized base class: {baseClass}");

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
            int slotCount = GetSlotCount(itemProperties);
            if (slotCount < min) min = slotCount;
            if (slotCount > max) max = slotCount;
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

    public static ItemSize GetCellSize(TemplateItemProperties properties) =>
        new()
        {
            Width = properties.Width.GetValueOrDefault() - properties.SizeReduceRight.GetValueOrDefault(),
            Height = properties.Height.GetValueOrDefault() - (int)(properties.ExtensionData!.GetValueOrDefault("SizeReduceDown") ?? 0)
        };
}

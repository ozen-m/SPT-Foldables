using Foldables.Models;
using Foldables.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Reflection;
using System.Text;

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
    public static ModConfig ModConfig { get; protected set; } = new();

    public Task OnLoad()
    {
        CommonUtils.Logger = logger;
        CommonUtils.ServerLocalisationService = serverLocalisationService;

        string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        string configPath = System.IO.Path.Combine(modPath, "config");
        LoadConfig(System.IO.Path.Combine(configPath, "config.json"));
        LoadLocales(System.IO.Path.Combine(configPath, "locales"));
        ValidateConfig();

        Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();

        var backpacksItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.BACKPACK) && GetIsFoldable(i.Id));
        AddFoldableProperties(backpacksItemTemplates, BaseClasses.BACKPACK);

        var vestsItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.VEST) && GetIsFoldable(i.Id) && !i.Properties.Slots.Any());
        AddFoldableProperties(vestsItemTemplates, BaseClasses.VEST);

        CommonUtils.LogSuccess("loaded successfully!".Localize());
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
            foreach (string localeFile in localeFiles)
            {
                string language = System.IO.Path.GetFileNameWithoutExtension(localeFile);
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
        foreach (var (lang, locale) in locales)
        {
            if (databaseService.GetLocales().Global.TryGetValue(lang, out var lazyloadedValue))
            {
                lazyloadedValue.AddTransformer((lazyloadedLocaleData) =>
                {
                    foreach (var (key, value) in locale)
                    {
                        lazyloadedLocaleData.TryAdd(key, value);
                    }
                    return lazyloadedLocaleData;
                });
            }
            else
            {
                CommonUtils.LogWarning($"Failed to add locale for language: {lang}, language not found in the SPT Database");
            }
        }
    }

    private static void ValidateConfig()
    {
        // Folding times
        ModConfig.MinFoldingTime = Math.Max(ModConfig.MinFoldingTime, 0);
        ModConfig.MaxFoldingTime = Math.Max(ModConfig.MinFoldingTime, ModConfig.MaxFoldingTime);

        // Folded cell sizes
        ModConfig.BackpackFoldedCellSizes = [.. ModConfig.BackpackFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        ModConfig.VestFoldedCellSizes = [.. ModConfig.VestFoldedCellSizes.OrderBy(s => s.MaxGridCount)];
        if (!ModConfig.BackpackFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `BackpackFoldedCellSizes`");
        }
        if (!ModConfig.VestFoldedCellSizes.Any(s => s.MaxGridCount == 0))
        {
            throw new InvalidDataException($"Default CellSize not found for `VestFoldedCellSizes`");
        }

        // Unknown/missing properties
        StringBuilder sb = new();
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
            if (value.Foldable && value.ItemSize == null && value.FoldingTime == null)
            {
                CommonUtils.LogWarning("missing-overrideproperties".Localize(key.ToString()));
            }
        }
        if (sb.Length > 0)
        {
            CommonUtils.LogWarning(sb.ToString());
        }
    }

    protected void AddFoldableProperties(IEnumerable<TemplateItem> itemTemplates, MongoId baseClass)
    {
        var (minGridCount, maxGridCount) = GetMinMaxGridCount(itemTemplates.Select(i => i.Properties));
        foreach (TemplateItem itemTemplate in itemTemplates)
        {
            TemplateItemProperties itemProperties = itemTemplate.Properties;
            int gridCount = GetGridCount(itemProperties);
            ItemSize reduceCellSize = GetReduceCellSize(itemTemplate.Id, gridCount, itemProperties, baseClass);
            double foldingTime = GetFoldingTime(itemTemplate.Id, gridCount, minGridCount, maxGridCount);

            itemProperties.Foldable = true;
            itemProperties.SizeReduceRight = reduceCellSize.Width;
            itemProperties.ExtensionData["SizeReduceDown"] = reduceCellSize.Height;
            itemProperties.ExtensionData["FoldingTime"] = foldingTime;

            CommonUtils.LogDebug("set-properties".Localize(new { name = itemTemplate.Name, id = itemTemplate.Id, size = GetCellSize(itemProperties), time = foldingTime }));
        }

        if (baseClass == BaseClasses.BACKPACK)
            CommonUtils.LogInfo("added-backpacks".Localize(itemTemplates.Count()));
        else if (baseClass == BaseClasses.VEST)
            CommonUtils.LogInfo("added-vests".Localize(itemTemplates.Count()));
    }

    protected bool GetIsFoldable(MongoId itemId)
    {
        if (ModConfig.Overrides.TryGetValue(itemId, out OverrideProperties overrideProperties))
            return overrideProperties.Foldable;

        return true;
    }

    protected double GetFoldingTime(MongoId itemId, int gridCount, int minGridCount, int maxGridCount)
    {
        if (ModConfig.Overrides.TryGetValue(itemId, out OverrideProperties overrideProperties) && overrideProperties.FoldingTime.HasValue)
            return overrideProperties.FoldingTime.Value;

        double minFoldtime = ModConfig.MinFoldingTime;
        double maxFoldTime = ModConfig.MaxFoldingTime;
        if (minFoldtime == maxFoldTime)
            return maxFoldTime;
        else
        {
            double scale = (double)(gridCount - minGridCount) / (maxGridCount - minGridCount);
            return Math.Round(minFoldtime + (maxFoldTime - minFoldtime) * scale, 2);
        }
    }

    protected ItemSize GetReduceCellSize(MongoId itemId, int gridCount, TemplateItemProperties properties, MongoId baseClass)
    {
        ItemSize foldedCellSize;
        if (ModConfig.Overrides.TryGetValue(itemId, out OverrideProperties overrideProperties) && overrideProperties.ItemSize != null)
            foldedCellSize = overrideProperties.ItemSize;
        else
            foldedCellSize = GetFoldedCellSize(gridCount, baseClass);

        if (properties.Width > properties.Height)
        {
            foldedCellSize = foldedCellSize.Swap();
        }
        return new ItemSize
        {
            Width = properties.Width.Value - foldedCellSize.Width,
            Height = properties.Height.Value - foldedCellSize.Height
        };
    }

    protected ItemSize GetFoldedCellSize(int gridCount, MongoId baseClass)
    {
        CellSizeRange[] foldedCellSizes;
        if (baseClass == BaseClasses.BACKPACK)
            foldedCellSizes = ModConfig.BackpackFoldedCellSizes;
        else if (baseClass == BaseClasses.VEST)
            foldedCellSizes = ModConfig.VestFoldedCellSizes;
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

    public static (int min, int max) GetMinMaxGridCount(IEnumerable<TemplateItemProperties> itemsProperties)
    {
        int min = int.MaxValue;
        int max = int.MinValue;

        foreach (TemplateItemProperties itemProperties in itemsProperties)
        {
            int gridCount = GetGridCount(itemProperties);
            if (gridCount < min) min = gridCount;
            if (gridCount > max) max = gridCount;
        }

        return (min, max);
    }

    public static ItemSize GetCellSize(TemplateItemProperties properties) =>
        new()
        {
            Width = properties.Width.GetValueOrDefault() - properties.SizeReduceRight.GetValueOrDefault(),
            Height = properties.Height.GetValueOrDefault() - (int)(properties.ExtensionData.GetValueOrDefault("SizeReduceDown") ?? 0)
        };

    public static int GetGridCount(TemplateItemProperties properties) =>
        properties.Grids.Sum(g => g.Properties.CellsH * g.Properties.CellsV).GetValueOrDefault();
}

using Foldables.Configuration;
using Foldables.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace Foldables;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)] // Process vanilla and mod items
public class Foldables(
    ISptLogger<Foldables> logger,
    ModHelper modHelper,
    JsonUtil jsonUtil,
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

        Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();

        var backpacksItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.BACKPACK));
        AddFoldableProperties(backpacksItemTemplates, BaseClasses.BACKPACK);

        var vestsItemTemplates = items
            .Values
            .Where(i => itemHelper.IsOfBaseclass(i.Id, BaseClasses.VEST) && !i.Properties.Slots.Any());
        AddFoldableProperties(vestsItemTemplates, BaseClasses.VEST);

        CommonUtils.LogSuccess("loaded successfully!".Localize());
        return Task.CompletedTask;
    }

    private void LoadConfig(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException(configFilePath);
        }
        try
        {
            ModConfig = jsonUtil.DeserializeFromFile<ModConfig>(configFilePath);
        }
        catch (Exception ex)
        {
            CommonUtils.LogError(ex.ToString());
            CommonUtils.LogError("Exception while trying to load configuration file, using default values. Misconfigured config.json? ");
        }
    }

    private void LoadLocales(string localesPath)
    {
        Dictionary<string, Dictionary<string, string>> locales = [];
        if (!Directory.Exists(localesPath))
        {
            throw new FileNotFoundException("Missing locales directory: " + localesPath);
        }
        try
        {
            string[] localeFiles = Directory.GetFiles(localesPath, "*.json");
            foreach (string localeFile in localeFiles)
            {
                string language = System.IO.Path.GetFileNameWithoutExtension(localeFile);
                locales[language] = jsonUtil.DeserializeFromFile<Dictionary<string, string>>(localeFile);
            }
        }
        catch (Exception ex)
        {
            CommonUtils.LogError(ex.ToString());
            CommonUtils.LogError("Exception while trying to load locales");
        }
        if (locales.Count < 1)
        {
            CommonUtils.LogWarning("No locale files found under: " + localesPath);
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
                CommonUtils.LogWarning("Failed to add locale for language: " + lang + ", language not found in the SPT Database");
            }
        }
    }

    private static void AddFoldableProperties(IEnumerable<TemplateItem> itemTemplates, MongoId baseClass)
    {
        foreach (TemplateItem itemTemplate in itemTemplates)
        {
            TemplateItemProperties itemProperties = itemTemplate.Properties;
            ItemSize reduceCellSize = GetReduceCellSize(itemTemplate.Id, itemProperties, baseClass);
            itemProperties.Foldable = true;
            itemProperties.SizeReduceRight = reduceCellSize.Width;
            itemProperties.ExtensionData["SizeReduceDown"] = reduceCellSize.Height;

            CommonUtils.LogDebug("set-size".Localize(new { name = itemTemplate.Name, id = itemTemplate.Id, size = GetCellSize(itemProperties) }));
        }

        if (baseClass == BaseClasses.BACKPACK)
            CommonUtils.LogInfo("added-backpacks".Localize(itemTemplates.Count()));
        else if (baseClass == BaseClasses.VEST)
            CommonUtils.LogInfo("added-vests".Localize(itemTemplates.Count()));
    }

    private static ItemSize GetReduceCellSize(MongoId itemId, TemplateItemProperties properties, MongoId baseClass)
    {
        int gridCount = GetGridCount(properties);

        if (ModConfig.Overrides.TryGetValue(itemId, out ItemSize foldedCellSize)) { }
        else if (baseClass == BaseClasses.BACKPACK)
            foldedCellSize = GetFoldedBackpackCellSize(gridCount);
        else if (baseClass == BaseClasses.VEST)
            foldedCellSize = GetFoldedVestCellSize(gridCount);
        else
            throw new ArgumentException("Cannot get reduced cell size for unrecognized base class");

        if (properties.Width > properties.Height)
        {
            foldedCellSize.Swap();
        }
        return new ItemSize
        {
            Width = properties.Width.Value - foldedCellSize.Width,
            Height = properties.Height.Value - foldedCellSize.Height
        };
    }

    private static int GetGridCount(TemplateItemProperties properties) =>
        properties.Grids.Sum(g => g.Properties.CellsH * g.Properties.CellsV).GetValueOrDefault();

    private static ItemSize GetFoldedBackpackCellSize(int gridCount) =>
        gridCount switch
        {
            <= 15 => new() { Width = 1, Height = 2 },
            <= 24 => new() { Width = 2, Height = 2 },
            <= 34 => new() { Width = 2, Height = 3 },
            <= 42 => new() { Width = 2, Height = 4 },
            _ => new() { Width = 3, Height = 3 }
        };

    private static ItemSize GetFoldedVestCellSize(int gridCount) =>
        gridCount switch
        {
            <= 15 => new() { Width = 1, Height = 2 },
            <= 24 => new() { Width = 1, Height = 3 },
            _ => new() { Width = 2, Height = 3 }
        };

    private static ItemSize GetCellSize(TemplateItemProperties properties) =>
        new()
        {
            Width = properties.Width.GetValueOrDefault() - properties.SizeReduceRight.GetValueOrDefault(),
            Height = properties.Height.GetValueOrDefault() - (int)(properties.ExtensionData.GetValueOrDefault("SizeReduceDown") ?? 0)
        };

}

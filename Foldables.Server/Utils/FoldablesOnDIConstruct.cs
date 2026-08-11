using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Foldables.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils.Json.Converters;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;

namespace Foldables.Utils;

public class FoldablesOnDIConstruct : IOnDIConstruct
{
    private static readonly JsonSerializerOptions _foldablesJsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NewLine = "\n",
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new StringToMongoIdConverter() },
    };

    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        var modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        var configPath = Path.Combine(modPath, "config", "config.json");
        var config = await LoadAsync<FoldablesConfig>(configPath, cancellationToken);
        serviceCollection.AddSingleton(config);

        var localesPath = Path.Combine(modPath, "config", "locales");
        var locales = await LoadLocalesAsync(localesPath, cancellationToken);
        serviceCollection.AddSingleton(locales);
    }

    private static async Task<T> LoadAsync<T>(string filePath, CancellationToken token = default) where T : new()
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        await using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return await JsonSerializer.DeserializeAsync<T>(fs, _foldablesJsonOptions, token) ?? new T();
    }

    private static async Task<FoldablesLocales> LoadLocalesAsync(string localesDirectory, CancellationToken token = default)
    {
        FoldablesLocales locales = [];

        if (!Directory.Exists(localesDirectory))
        {
            throw new DirectoryNotFoundException($"Missing locales directory: {localesDirectory}");
        }

        var localeFiles = Directory.GetFiles(localesDirectory, "*.json");
        foreach (var localeFile in localeFiles)
        {
            var language = Path.GetFileNameWithoutExtension(localeFile);
            locales[language] = await LoadAsync<Dictionary<string, string>>(localeFile, token);
        }

        return locales;
    }
}

[Injectable(InjectionType = InjectionType.Singleton)]
public class ConfigEditorProvider(FoldablesConfig config, ModHelper modHelper) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        var metadata = new ModMetadata();
        var modDir = modHelper.GetAbsolutePathToModFolder();
        yield return ConfigEditorConfigRegistration.Create(
            metadata.ModGuid,
            metadata.Name,
            config,
            Path.Combine(modDir, "config", "config.json")
        );
    }
}

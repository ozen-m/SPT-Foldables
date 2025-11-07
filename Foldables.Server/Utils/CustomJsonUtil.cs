using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace Foldables.Utils;

[Injectable(InjectionType.Singleton)]
public class CustomJsonUtil(IEnumerable<IJsonConverterRegistrator> registrators) : JsonUtil(registrators)
{
    private readonly JsonSerializerOptions _jsonSerializerOptionsNoIndentCaseInsensitive = new(JsonSerializerOptionsNoIndent!)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true
    };

    public T DeserializeFromFile<T>(string file, bool customOptions = false)
    {
        if (!customOptions)
        {
            return base.DeserializeFromFile<T>(file);
        }

        if (!File.Exists(file))
        {
            return default;
        }

        using FileStream fs = new(file, FileMode.Open, FileAccess.Read);
        return JsonSerializer.Deserialize<T>(fs, _jsonSerializerOptionsNoIndentCaseInsensitive);
    }
}

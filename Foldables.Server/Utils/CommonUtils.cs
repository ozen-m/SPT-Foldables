using Foldables.Models;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace Foldables.Utils;

public static class CommonUtils
{
    private const string _debugMessagePrefix = "[Foldables] ";

    public static ISptLogger<Foldables> Logger { get; set; }
    public static ServerLocalisationService ServerLocalisationService { get; set; }
    public static ItemHelper ItemHelper { get; set; }
    private static ModConfig ModConfig => Foldables.ModConfig;

    public static void LogSuccess(string message) => Logger?.Success(_debugMessagePrefix + message);

    public static void LogInfo(string message) => Logger?.Info(_debugMessagePrefix + message);

    public static void LogWarning(string message) => Logger?.Warning(_debugMessagePrefix + message);

    public static void LogError(string message) => Logger?.Error(_debugMessagePrefix + message);

    public static void LogDebug(string message)
    {
        if (ModConfig.DebugLogs)
        {
            Logger?.Debug(_debugMessagePrefix + message);
        }
    }

    public static ItemSize Swap(this ItemSize itemSize) => new() { Width = itemSize.Height, Height = itemSize.Width };

    public static string Localized(this string key, object args = null) => ServerLocalisationService?.GetText(key, args);

    public static string Localized<T>(this string key, T value)
        where T : IConvertible => ServerLocalisationService?.GetText(key, value);
}

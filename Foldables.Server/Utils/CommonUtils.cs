using Foldables.Configuration;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace Foldables.Utils;

public static class CommonUtils
{
    public static ISptLogger<Foldables> Logger { get; set; }
    public static ServerLocalisationService ServerLocalisationService { get; set; }
    public static ItemHelper ItemHelper { get; set; }
    private static ModConfig ModConfig => Foldables.ModConfig;

    private const string _debugMessagePrefix = "[Foldables] ";

    public static void LogSuccess(string message)
    {
        Logger?.Success(_debugMessagePrefix + message);
    }

    public static void LogInfo(string message)
    {
        Logger?.Info(_debugMessagePrefix + message);
    }

    public static void LogWarning(string message)
    {
        Logger?.Warning(_debugMessagePrefix + message);
    }

    public static void LogError(string message)
    {
        Logger?.Error(_debugMessagePrefix + message);
    }

    public static void LogDebug(string message)
    {
        if (ModConfig.DebugLogs)
        {
            Logger?.Debug(_debugMessagePrefix + message);
        }
    }

    public static void Swap(this ItemSize itemSize)
    {
        (itemSize.Width, itemSize.Height) = (itemSize.Height, itemSize.Width);
    }

    public static string Localize(this string key, object args = null)
    {
        return ServerLocalisationService.GetText(key, args);
    }

    public static string Localize<T>(this string key, T value)
        where T : IConvertible
    {
        return ServerLocalisationService.GetText(key, value);
    }
}
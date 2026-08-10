using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Services.Locales;

namespace Foldables.Utils;

public static class CommonExtensions
{
    private static ServerLocalisationService _serverLocalisationService;

    public static void SetServerLocalisationService(ServerLocalisationService service)
    {
        _serverLocalisationService = service;
    }

    public static ItemSize Swap(this ItemSize itemSize) => new() { Width = itemSize.Height, Height = itemSize.Width };
    
    public static int GetArea(this ItemSize itemSize) => itemSize.Width * itemSize.Height;

    public static string Localized(this string key, object args = null) => _serverLocalisationService?.GetText(key, args);

    public static string Localized<T>(this string key, T value)
        where T : IConvertible => _serverLocalisationService?.GetText(key, value);
}

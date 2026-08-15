using Foldables.Models;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace Foldables.Utils;

[Injectable]
public class FoldablesLogger(ISptLogger<Foldables> logger, FoldablesConfig config)
{
    private const string LogPrefix = $"[{nameof(Foldables)}] ";

    public void Success(string message) => logger.Success(LogPrefix + message);

    public void Info(string message) => logger.Info(LogPrefix + message);

    public void Warning(string message) => logger.Warning(LogPrefix + message);

    public void Error(string message) => logger.Error(LogPrefix + message);

    public void Debug(string message)
    {
        if (config.DebugLogs)
        {
            logger.Debug(LogPrefix + message);
        }
    }
}

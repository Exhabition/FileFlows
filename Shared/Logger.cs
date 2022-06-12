using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FileFlows.Shared;

using FileFlows.Plugin;

/// <summary>
/// A logger used to write log messages 
/// </summary>
public class Logger : ILogger
{
    private static Logger _Instance;
    /// <summary>
    /// Gets or sets the instance of Logger
    /// </summary>
    public static Logger Instance
    {
        get
        {
            _Instance ??= new();
            return _Instance;
        }
    }


    /// <summary>
    /// Logs a information message
    /// </summary>
    /// <param name="args">the arguments for the log message</param>
    public void ILog(params object[] args) => Log(LogType.Info, args);
    
    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="args">the arguments for the log message</param>
    public void DLog(params object[] args) => Log(LogType.Debug, args);
    
    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="args">the arguments for the log message</param>
    public void WLog(params object[] args) => Log(LogType.Warning, args);

    /// <summary>
    /// Logs a error message
    /// </summary>
    /// <param name="args">the arguments for the log message</param>
    public void ELog(params object[] args) => Log(LogType.Error, args);


    /// <summary>
    /// Gets the last number of log lines
    /// </summary>
    /// <param name="length">The maximum number of lines to grab</param>
    /// <returns>The last number of log lines</returns>
    public string GetTail(int length = 50) => "";

    private readonly List<ILogWriter> Writers = new ();

    public void RegisterWriter(ILogWriter log)
    {
        if(Writers.Contains(log) == false)
            Writers.Add(log);
    }

    /// <summary>
    /// Tries to get a specific registered log writer
    /// </summary>
    /// <typeparam name="T">the type of log writer to get</typeparam>
    /// <returns>the instance if found, otherwise false</returns>
    public bool TryGetLogger<T>(out T writer) where T : class, ILogWriter
    {
        writer = Writers.FirstOrDefault(x => x is T) as T;
        return writer != null;
    }

    private void Log(LogType type, params object[] args)
    {
        foreach(var writer in Writers)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await writer.Log(type, args);
                }
                catch (Exception)
                {
                }
            });
        }
    }
}
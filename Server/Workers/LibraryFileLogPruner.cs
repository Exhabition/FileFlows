using FileFlows.Server.Controllers;
using FileFlows.Server.Services;
using FileFlows.ServerShared.Workers;

namespace FileFlows.Server.Workers;

/// <summary>
/// Worker that will automatically delete logs for non existing library files
/// </summary>
public class LibraryFileLogPruner:Worker
{
    /// <summary>
    /// Constructor for the log pruner
    /// </summary>
    public LibraryFileLogPruner() : base(ScheduleType.Daily, 5)
    {
    }
    
    /// <summary>
    /// Executes the log pruner, Run calls this 
    /// </summary>
    protected override void Execute()
    {
        var libFiles = new LibraryFileService().GetUids().Result.Select(x => x.ToString()).ToList();
        var files = new DirectoryInfo(DirectoryHelper.LibraryFilesLoggingDirectory).GetFiles();
        foreach (var file in files)
        {
            // first check if the file is somewhat new, if it is, dont delete just yet
            if (file.LastWriteTime > DateTime.Now.AddHours(-1))
                continue;
            
            string shortName = file.Name;
            if (file.Extension?.Length > 0)
                shortName = shortName[..shortName.LastIndexOf(file.Extension, StringComparison.Ordinal)];
            
            // .html.gz, .log.gz
            if (shortName.EndsWith(".html"))
                shortName = shortName.Replace(".html", "");
            if (shortName.EndsWith(".log"))
                shortName = shortName.Replace(".log", "");
            
            bool exists = libFiles.Contains(shortName);

            if (exists)
                continue;
            try
            {
                file.Delete();
                Shared.Logger.Instance?.DLog("Deleted old unknown log file: " + file);
            }
            catch (Exception)
            {
            }
        }
    }
}
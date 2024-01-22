using System.Text.RegularExpressions;
using FileFlows.Plugin.Models;
using System.Runtime.InteropServices;
using System.Text.Json;
using FileFlows.Plugin.Helpers;
using FileFlows.Plugin.Services;

namespace FileFlows.Plugin;

public class NodeParameters
{
    /// <summary>
    /// The original filename of the file
    /// Note: This maybe a mapped filename if executed on a external processing node
    /// </summary>
    public string FileName { get; init; }
    
    /// <summary>
    /// Gets or sets the full filename as it appears in the library
    /// </summary>
    public string LibraryFileName { get; init; }

    /// <summary>
    /// Gets or sets the file relative to the library path
    /// </summary>
    public string RelativeFile { get; set; }

    /// <summary>
    /// The current working file as it is being processed in the flow, 
    /// this is what a node should save any changes too, and if the node needs 
    /// to change the file path this should be updated too
    /// </summary>
    public string WorkingFile { get; private set; }

    /// <summary>
    /// Gets the working file shortname
    /// </summary>
    public string WorkingFileName => string.IsNullOrWhiteSpace(WorkingFile) ? string.Empty : 
        IsDirectory ? new DirectoryInfo(this.WorkingFile).Name : new FileInfo(this.WorkingFile).Name;

    private long _WorkingFileSize { get; set; }
    /// <summary>
    /// Gets the last actual record file size that is greater than zero
    /// </summary>
    public long LastValidWorkingFileSize { get; private set; }

    /// <summary>
    /// Gets or sets the file size of the current working file
    /// </summary>
    public long WorkingFileSize
    {
        get => _WorkingFileSize;
        private set
        {
            if (value > 1)
                LastValidWorkingFileSize = value;
            _WorkingFileSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the logger used by the flow during execution
    /// </summary>
    public ILogger? Logger { get; set; }
    
    /// <summary>
    /// Gets or set s the script executor
    /// </summary>
    public IScriptExecutor ScriptExecutor { get; set; }

    /// <summary>
    /// Gets or sets the result of the flow
    /// </summary>
    public NodeResult Result { get; set; } = NodeResult.Success;

    /// <summary>
    /// Gets or sets the parameters used in the flow execution
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Gets or sets the variables used that are passed between executed nodes
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the function responsible for getting the actual tool path
    /// </summary>
    public Func<string, string>? GetToolPathActual { get; set; }
    
    /// <summary>
    /// Gets or sets the function responsible for getting if a plugin is available
    /// </summary>
    public Func<string, bool>? HasPluginActual { get; set; }
    
    /// <summary>
    /// Gets or sets the action that records statistics
    /// </summary>
    public Action<string, object>? StatisticRecorder { get; set; }

    /// <summary>
    /// Gets or sets the function responsible for getting plugin settings JSON configuration
    /// </summary>
    public Func<string, string> GetPluginSettingsJson { get; set; }
    
    /// <summary>
    /// Gets or sets the function responsible for mapping a path
    /// </summary>
    public Func<string, string>? PathMapper { get; set; }
    
    /// <summary>
    /// Gets or sets the function responsible for unmapping a path
    /// </summary>
    public Func<string, string>? PathUnMapper { get; set; }
    
    /// <summary>
    /// Gets or sets the function responsible for uploading a file
    /// </summary>
    public Func<string, string, (bool Success, string Error)>? UploadFile { get; set; }
    
    /// <summary>
    /// Gets or sets the function responsible for deleting a remote directory
    /// </summary>
    public Func<string, bool, string[], bool>? DeleteRemote { get; set; }

    /// <summary>
    /// Gets or sets a goto flow 
    /// </summary>
    public Action<ObjectReference> GotoFlow { get; set; }

    /// <summary>
    /// Gets or sets if a directory is being processed instead of a file
    /// </summary>
    public bool IsDirectory { get; set; }
    
    /// <summary>
    /// Gets or sets the path to the library this library file belongs to
    /// </summary>
    public string LibraryPath { get; set; }

    /// <summary>
    /// Gets or sets if this node is running inside a docker container
    /// </summary>
    public bool IsDocker { get; set; }

    /// <summary>
    /// Gets or sets if this node is running on windows
    /// </summary>
    public bool IsWindows { get; set; }

    /// <summary>
    /// Gets or sets if this node is running on linux
    /// </summary>
    public bool IsLinux { get; set; }

    /// <summary>
    /// Gets or sets if this node is running on a mac
    /// </summary>
    public bool IsMac { get; set; }

    /// <summary>
    /// Gets or sets if this node is running on a ARM base platform
    /// </summary>
    public bool IsArm { get; set; }
    
    /// <summary>
    /// Gets or sets if the file is a remote file and needs to be downloaded from the server then copied back to it
    /// </summary>
    public bool IsRemote { get; set; }

    /// <summary>
    /// Gets or sets the temporary path for this node
    /// </summary>
    public string TempPath { get; set; }

    /// <summary>
    /// Gets or sets the short temporary path name
    /// eg. Runner-42f99fc9-158e-408d-9133-de91a56a6ac8
    /// </summary>
    public string TempPathName { get; set; }

    /// <summary>
    /// Gets the temp path on the host if running on docker from the environmental variable "TempPathHost"
    /// If not running on docker just returns TempPath
    /// </summary>
    public string TempPathHost
    {
        get
        {
            var host = Environment.GetEnvironmentVariable("TempPathHost");
            if (string.IsNullOrEmpty(host))
                return TempPath;
            // need to append runner subpath
            return Path.Combine(host, "Runner-" + RunnerUid);
        }
    }

    /// <summary>
    /// Gets or sets the runners UID
    /// </summary>
    public Guid RunnerUid { get; set; }

    /// <summary>
    /// Gets or sets the action that handles updating a percentage change for a flow part
    /// </summary>
    public Action<float>? PartPercentageUpdate { get; set; }

    /// <summary>
    /// Gets or sets the process helper
    /// </summary>
    public ProcessHelper Process { get; set; }

    /// <summary>
    /// if this is af faked instance
    /// </summary>
    private bool Fake = false;
    
    /// <summary>
    /// Gets or sets the original metadata for the input file
    /// </summary>
    public Dictionary<string, object> OriginalMetadata { get; private set; }
    
    /// <summary>
    /// Gets or sets the metadata for the file
    /// </summary>
    public Dictionary<string, object> Metadata { get; private set; }
    
    /// <summary>
    /// Gets if this is a enterprise user
    /// </summary>
    public bool Enterprise { get; init; }

    /// <summary>
    /// Sets the metadata for the file
    /// </summary>
    /// <param name="metadata">the metadata for the file</param>
    public void SetMetadata(Dictionary<string, object> metadata)
    {
        if (OriginalMetadata == null)
            OriginalMetadata = metadata;
        else
            Metadata = metadata;
    }
    
    /// <summary>
    /// Gets or sets the file service to use
    /// </summary>
    public IFileService FileService { get; init; }


    /// <summary>
    /// Constructs a node parameters instance used by the flow runner
    /// </summary>
    /// <param name="filename">the filename of the original library file</param>
    /// <param name="logger">the logger used during execution</param>
    /// <param name="isDirectory">if this is executing against a directory instead of a file</param>
    /// <param name="libraryPath">the path of the library this file exists in</param>
    /// <param name="fileService">the FileService to user</param>
    public NodeParameters(string? filename, ILogger logger, bool isDirectory, string? libraryPath, IFileService fileService)
    {
        Fake = string.IsNullOrEmpty(filename);
        this.IsDirectory = isDirectory;
        this.FileName = filename;
        this.LibraryPath = libraryPath;
        this.WorkingFile = filename;
        this.FileService = fileService;
        if (Fake == false)
        {
            try
            {
                this.WorkingFileSize = IsDirectory ? GetDirectorySize(filename) : new FileInfo(filename).Length;
            }
            catch (Exception) { } // can fail in unit tests
        }
        this.RelativeFile = string.Empty;
        this.TempPath = string.Empty;
        this.Logger = logger;
        InitFile(filename);
        this.Process = new ProcessHelper(logger, this.Fake);
    }

    /// <summary>
    /// Constructs a new basic node parameters with no file 
    /// </summary>
    /// <param name="logger">the logger used during execution</param>
    public NodeParameters(ILogger logger)
    {
        this.Logger = logger;
        this.Process = new ProcessHelper(logger, false);
    }


    /// <summary>
    /// Gets the file size of a directory and all its files
    /// </summary>
    /// <param name="path">The path of the directory</param>
    /// <returns>The directories total size</returns>
    public long GetDirectorySize(string path)
    {
        if (Fake) return 100_000_000_000;
        try
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Maps a path to one that exists on the processing node
    /// Note: It is safe to map a path multiple times as this should not effect its value
    /// </summary>
    /// <param name="path">The path to map</param>
    /// <returns>The mapped path</returns>
    public string MapPath(string path)
    {
        if (Fake) return path;
        if (PathMapper == null)
            return path;
        return PathMapper(path);
    }

    /// <summary>
    /// Unmaps a path to the original FileFlows Server path
    /// Note: It is safe to unmap a path multiple times as this should not effect its value
    /// </summary>
    /// <param name="path">The path to unmap</param>
    /// <returns>The unmapped path as it appears on the server</returns>
    public string UnMapPath(string path)
    {
        if (Fake) return path;
        if (PathUnMapper == null)
            return path;
        return PathUnMapper(path);
    }
    
    /// <summary>
    /// Checks if a plugin is available
    /// </summary>
    /// <param name="name">The name of the plugin</param>
    /// <returns>true if the plugin is available</returns>
    public bool HasPlugin(string name)
    {
        if (HasPluginActual == null) return false;
        return HasPluginActual(name);
    }

    private bool initDone = false;
    
    /// <summary>
    /// Initializes a file ane updates the variables to that file information
    /// </summary>
    /// <param name="filename">the name of the file to initialize</param>
    private void InitFile(string filename)
    {
        if (Fake) return;
        try
        {
            if (IsDirectory)
            {
                var di = new DirectoryInfo(filename);
                UpdateVariables(new Dictionary<string, object>
                {
                    { "folder.Name", di.Name ?? "" },
                    { "folder.FullName", di.FullName ?? "" }
                });
                if (initDone == false)
                {
                    initDone = true;
                    var diOriginal = new DirectoryInfo(this.FileName);
                    UpdateVariables(new Dictionary<string, object>
                    {
                        { "folder.Date", diOriginal.CreationTime },
                        { "folder.Date.Year", diOriginal.CreationTime.Year },
                        { "folder.Date.Month", diOriginal.CreationTime.Month },
                        { "folder.Date.Day", diOriginal.CreationTime.Day },

                        { "folder.Orig.Name", diOriginal.Name ?? "" },
                        { "folder.Orig.FullName", diOriginal.FullName ?? "" },
                    });

                }
            }
            else
            {
                var result = FileService.FileInfo(filename);
                if (result.IsFailed)
                    return;
                var fi = result.Value;
                var ext = fi.Extension ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ext) == false && ext.StartsWith(".") == false)
                    ext = "." + ext;
                UpdateVariables(new Dictionary<string, object>
                {
                    { "ext", ext },
                    { "file.Name", fi.Name ?? "" },
                    { "file.NameNoExtension", Path.GetFileNameWithoutExtension(fi.Name ?? "") },
                    { "file.FullName", fi.FullName ?? "" },
                    { "file.Extension", fi.Extension ?? "" },
                    { "file.Size", fi.Length },

                    { "folder.Name", FileHelper.GetDirectoryName(fi.FullName) ?? "" },
                    { "folder.FullName", fi.Directory ?? "" },
                });

                if (initDone == false)
                {
                    initDone = true;
                    var fiOrigResult = FileService.FileInfo(this.FileName);
                    if (fiOrigResult.IsFailed)
                        return;
                    var fiOriginal = fiOrigResult.Value;
                    UpdateVariables(new Dictionary<string, object>
                    {
                        { "file.Create", fiOriginal.CreationTime },
                        { "file.Create.Year", fiOriginal.CreationTime.Year },
                        { "file.Create.Month", fiOriginal.CreationTime.Month },
                        { "file.Create.Day", fiOriginal.CreationTime.Day },

                        { "file.Modified", fiOriginal.LastWriteTime },
                        { "file.Modified.Year", fiOriginal.LastWriteTime.Year },
                        { "file.Modified.Month", fiOriginal.LastWriteTime.Month },
                        { "file.Modified.Day", fiOriginal.LastWriteTime.Day },

                        { "file.Orig.Extension", fiOriginal.Extension ?? string.Empty },
                        { "file.Orig.FileName", fiOriginal.Name ?? string.Empty },
                        { "file.Orig.FileNameNoExtension", Path.GetFileNameWithoutExtension(fiOriginal.Name ?? "") },
                        { "file.Orig.FullName", fiOriginal.FullName ?? string.Empty },
                        { "file.Orig.Size", fiOriginal.Length },

                        { "folder.Orig.Name", FileHelper.GetDirectoryName(fiOriginal.Directory) ?? string.Empty },
                        { "folder.Orig.FullName", fiOriginal.Directory ?? "" }
                    });

                    if (string.IsNullOrEmpty(this.LibraryPath) == false &&
                        fiOriginal.FullName.StartsWith(this.LibraryPath))
                    {
                        UpdateVariables(new Dictionary<string, object>
                        {
                            { "file.Orig.RelativeName", fiOriginal.FullName.Substring(LibraryPath.Length + 1) }
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.ELog("Failed to init file: " + ex.Message);
        }

    }

    /// <summary>
    /// Resets the working file back to the original library file
    /// </summary>
    public void ResetWorkingFile()
    {
        if (Fake) return;
        SetWorkingFile(this.FileName);
    }

    /// <summary>
    /// Tests if a file exists 
    /// </summary>
    /// <param name="filename">The filename to test</param>
    /// <returns>true if exists, otherwise false</returns>
    public bool FileExists(string filename)
    {
        try
        {
            return File.Exists(filename);
        }
        catch (Exception) { return false; } 
    }

    /// <summary>
    /// Updates the current working file and initializes it
    /// </summary>
    /// <param name="filename">The new working file</param>
    /// <param name="dontDelete">If the existing working file should not be deleted.</param>
    public void SetWorkingFile(string filename, bool dontDelete = false)
    {
        if (Fake) return;

        bool isDirectory = Directory.Exists(filename);
        Logger?.ILog("Setting working file to: " + filename);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            if (filename?.ToLower().StartsWith(TempPath.ToLower()) == true)
            {
                Logger.ILog("Changing owner on working file: " + filename);
                Helpers.FileHelper.ChangeOwner(Logger, filename, file: isDirectory == false);
            }
            else
            {
                Logger.ILog("NOT changing owner on working file: " + filename + ", temp path: " + TempPath);
            }
        }

        if (isDirectory == false)
        {
            this.WorkingFileSize = FileService.FileSize(filename).ValueOrDefault;
            Logger?.ILog("New working file size: " + this.WorkingFileSize);
        }

        if (this.WorkingFile == filename)
        {
            Logger?.ILog("Working file same as new filename: " + filename);
            return;
        }

        if (dontDelete == false)
        {
            dontDelete = this.WorkingFile.ToLowerInvariant().StartsWith(TempPath.ToLowerInvariant()) == false;
        }

        if (isDirectory == false && this.WorkingFile != this.FileName)
        {
            string fileToDelete = this.WorkingFile;
            if (dontDelete == false)
            {
                // delete the old working file
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2_000); // wait 2 seconds for the file to be released if used
                    var result = FileService.FileDelete(fileToDelete);
                    if (result.IsFailed)
                        Logger?.WLog("Failed to delete temporary file: " + result.Error);
                    else
                        Logger.ILog("Deleting old working file: " + fileToDelete);
                });
            }
        }
        this.IsDirectory = IsDirectory;
        this.WorkingFile = filename;
        InitFile(filename);
    }

    
    /// <summary>
    /// Gets a parameter by its name
    /// </summary>
    /// <param name="name">the name of the parameter</param>
    /// <typeparam name="T">The type of parameter it is</typeparam>
    /// <returns>The value if found, otherwise default(T)</returns>
    public T GetParameter<T>(string name)
    {
        if (Parameters.ContainsKey(name) == false)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)string.Empty;
            return default(T)!;
        }
        return (T)Parameters[name];
    }

    /// <summary>
    /// Sets a value in the Parameters list
    /// </summary>
    /// <param name="name">The name/key of the parameter</param>
    /// <param name="value">The value to set</param>
    public void SetParameter(string name, object value)
    {
        if (Parameters.ContainsKey(name) == false)
            Parameters[name] = value;
        else
            Parameters.Add(name, value);
    }

    
    /// <summary>
    /// Moves the working file
    /// </summary>
    /// <param name="destination">the destination to move the file</param>
    /// <returns>true if successfully moved</returns>
    public bool MoveFile(string destination)
    {
        if (Fake) return true;
        
        Logger?.ILog("MoveFile: " + WorkingFile);
        Logger?.ILog("Destination: " + destination);

        var result = FileService.FileMove(WorkingFile, destination, true);
        if (result.IsFailed)
        {
            Logger?.ELog("Failed to move file: " + result.Error);
            return false;
        }
        Logger?.ILog("File moved to: " + destination);
        this.WorkingFile = destination;
        try
        {
            // this can fail if the file is then moved really quickly by another process, radarr/sonarr etc
            Logger?.ILog("Initing new moved file");
            InitFile(destination);
        }
        catch (Exception) { }
        
        // FileInfo file = new FileInfo(destination);
        // // turning this of as per https://github.com/revenz/FileFlows/issues/79
        // // if (string.IsNullOrEmpty(file.Extension) == false)
        // // {
        // //     // just ensures extensions are lowercased
        // //     destination = new FileInfo(file.FullName.Substring(0, file.FullName.LastIndexOf(file.Extension)) + file.Extension.ToLower()).FullName;
        // // }
        //
        // Logger?.ILog("About to move file to: " + destination);
        // destination = MapPath(destination);
        // Logger?.ILog("Mapped destination path: " + destination);
        //
        // bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        // if (isWindows)
        // {
        //     if (destination.ToLower() == WorkingFile?.ToLower())
        //     {
        //         Logger?.ILog("Source and destination are the same, skipping move");
        //         return true;
        //     }
        // }
        // else
        // {
        //     // linux, is case sensitive
        //     if(destination == WorkingFile)
        //     {
        //         Logger?.ILog("Source and destination are the same, skipping move");
        //         return true;
        //     }
        // }
        //
        //
        // bool moved = false;
        // long fileSize = new FileInfo(WorkingFile).Length;
        // Task task = Task.Run(() =>
        // {
        //     try
        //     {
        //
        //         var fileInfo = new FileInfo(destination);
        //         if (fileInfo.Exists)
        //             fileInfo.Delete();
        //         else
        //             CreateDirectoryIfNotExists(fileInfo?.DirectoryName);
        //
        //         Logger?.ILog($"Moving file: \"{WorkingFile}\" to \"{destination}\"");                    
        //         File.Move(WorkingFile, destination, true);
        //         Logger?.ILog("File moved successfully");
        //
        //         Helpers.FileHelper.ChangeOwner(Logger, destination, file: true);
        //
        //         this.WorkingFile = destination;
        //         try
        //         {
        //             // this can fail if the file is then moved really quickly by another process, radarr/sonarr etc
        //             Logger?.ILog("Initing new moved file");
        //             InitFile(destination);
        //         }
        //         catch (Exception) { }
        //
        //         moved = true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Logger?.ELog("Failed to move file: " + ex.Message);
        //     }
        // });
        //
        // while (task.IsCompleted == false)
        // {
        //     long currentSize = 0;
        //     var destFileInfo = new FileInfo(destination);
        //     if (destFileInfo.Exists)
        //         currentSize = destFileInfo.Length;
        //
        //     if (PartPercentageUpdate != null && fileSize > 0)
        //         PartPercentageUpdate(currentSize / fileSize * 100);
        //     Thread.Sleep(50);
        // }
        //
        // if (moved == false)
        //     return false;
        //
        // if (PartPercentageUpdate != null)
        //     PartPercentageUpdate(100);
        return true;
    }

    /// <summary>
    /// Copies a folder to the destination
    /// Paths will automatically be mapped relative to the Node executing it
    /// </summary>
    /// <param name="source">the source file</param>
    /// <param name="destination">the destination file</param>
    /// <param name="updateWorkingFile"></param>
    /// <returns>whether or not the file was copied successfully</returns>
    public bool CopyFile(string source, string destination, bool updateWorkingFile = false)
    {
        if (Fake) return true;

        if (string.IsNullOrWhiteSpace(source))
        {
            Logger?.WLog("CopyFile.Source was not supplied");
            return false;
        }
        if (string.IsNullOrWhiteSpace(destination))
        {
            Logger?.WLog("CopyFile.Destination was not supplied");
            return false;
        }

        var result = FileService.FileCopy(source, destination, true);
        if (result.IsFailed)
        {
            Logger?.WLog("Failed to copy file: " + result.Error);
            return false;
        }
        if(updateWorkingFile)
        {
            this.WorkingFile = destination;
            try
            {
                // this can fail if the file is then moved really quickly by another process, radarr/sonarr etc
                Logger?.ILog("Initing new copied file");
                InitFile(destination);
            }
            catch (Exception) { }
            
        }

        return true;
        
        
        string originalSource = source;
        source = MapPath(source);
        if(originalSource != source)
            Logger?.ILog($"Mapped path from '{originalSource}' to '{source}'");

        string originalDestination = destination;
        destination = MapPath(destination);
        if(originalDestination != destination)
            Logger?.ILog($"Mapped path from '{originalDestination}' to '{destination}'");
        
        FileInfo file = new FileInfo(destination);
        
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            if (destination.ToLower() == source.ToLower())
            {
                Logger?.ILog("Source and destination are the same, skipping move");
                return true;
            }
        }
        else
        {
            // linux, is case sensitive
            if (destination == source)
            {
                Logger?.ILog("Source and destination are the same, skipping move");
                return true;
            }
        }


        bool copied = false;
        long fileSize = new FileInfo(source).Length;
        Task task = Task.Run(() =>
        {
            try
            {

                var fileInfo = new FileInfo(destination);
                if (fileInfo.Exists)
                    fileInfo.Delete();
                else
                    CreateDirectoryIfNotExists(fileInfo?.DirectoryName);

                bool isTempFile = source.ToLower().StartsWith(this.TempPath.ToLower()) == true;

                Logger?.ILog($"Copying file: \"{source}\" to \"{destination}\"");
                File.Copy(source, destination, true);
                Logger?.ILog("File copied successfully");

                Helpers.FileHelper.ChangeOwner(Logger, destination, file: true);

                if (updateWorkingFile == false)
                {
                    copied = true;
                    return;
                }

                this.WorkingFile = destination;
                try
                {
                    // this can fail if the file is then moved really quickly by another process, radarr/sonarr etc
                    Logger?.ILog("Initing new copied file");
                    InitFile(destination);
                }
                catch (Exception) { }

                copied = true;
            }
            catch (Exception ex)
            {
                Logger?.ELog("Failed to move file: " + ex.Message);
            }
        });

        while (task.IsCompleted == false)
        {
            long currentSize = 0;
            var destFileInfo = new FileInfo(destination);
            if (destFileInfo.Exists)
                currentSize = destFileInfo.Length;

            if (PartPercentageUpdate != null && fileSize > 0)
                PartPercentageUpdate(currentSize / fileSize * 100);
            Thread.Sleep(50);
        }

        if (copied == false)
            return false;

        if (PartPercentageUpdate != null)
            PartPercentageUpdate(100);
        return true;
    }

    /// <summary>
    /// Cancels the flow execution
    /// </summary>
    public void Cancel()
    {
        this.Process?.Cancel();
    }

    /// <summary>
    /// Updates the variables in the flow execution
    /// This is so the remaining nodes can now use these variable values.
    /// Note: if a value is null, that item will be removed from the Variable list
    /// </summary>
    /// <param name="updates">The updated values</param>
    public void UpdateVariables(Dictionary<string, object>? updates)
    {
        if (updates == null)
            return;
        foreach (var key in updates.Keys)
        {
            var value = updates[key];
            if (Variables.ContainsKey(key))
            {
                if (value == null)
                    Variables.Remove(key);
                else
                    Variables[key] = value;
            }
            else if(value != null)
                Variables.Add(key, updates[key]);
        }
    }

    /// <summary>
    /// Replaces variables in a given string
    /// </summary>
    /// <param name="input">the input string</param>
    /// <param name="stripMissing">if missing variables should be removed</param>
    /// <param name="cleanSpecialCharacters">if special characters (eg directory path separator) should be replaced</param>
    /// <returns>the string with the variables replaced</returns>
    public string ReplaceVariables(string input, bool stripMissing = false, bool cleanSpecialCharacters = false) => VariablesHelper.ReplaceVariables(input, Variables, stripMissing, cleanSpecialCharacters);


    /// <summary>
    /// Gets a safe filename with any reserved characters removed or replaced
    /// </summary>
    /// <param name="fullFileName">the full filename of the file to make safe</param>
    /// <returns>the safe filename</returns>
    public string GetSafeName(string fullFileName)
    {
        string destName = FileHelper.GetShortFileName(fullFileName);
        string destDir = FileHelper.GetDirectory(fullFileName);

        // replace these here to avoid double spaces in name
        if (Path.GetInvalidFileNameChars().Contains(':'))
        {
            destName = destName.Replace(" : ", " - ");
            destName = destName.Replace(": ", " - ");
            destDir = destDir.Replace(" : ", " - ");
            destDir = destDir.Replace(": ", " - ");
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (c == ':')
            {
                destName = destName.Replace(c.ToString(), " - ");
                if(c != Path.DirectorySeparatorChar && c != Path.PathSeparator)
                    destDir = destDir.Replace(c.ToString(), " - ");
            }
            else
            {
                destName = destName.Replace(c.ToString(), "");
                if (c != Path.DirectorySeparatorChar && c != Path.PathSeparator)
                    destDir = destDir.Replace(c.ToString(), "");
            }
        }
        // put the drive letter back if it was replaced with a ' - '
        destDir = Regex.Replace(destDir, @"^([a-z]) \- ", "$1:", RegexOptions.IgnoreCase);
        string seperator = destDir.Contains("\\") ? "\\" : "/";
        if (destDir.EndsWith(seperator))
            destDir = destDir[..^1];

        return destDir + seperator + destName;
    }

    /// <summary>
    /// Copies a file into the temporary directory if it is not already in the temporary directory
    /// </summary>
    /// <param name="filename">[Optional] the filename to copy, if not set the working file will be set</param>
    /// <returns>the new filename</returns>
    public string CopyToTemp(string filename = null)
    {
        if (Fake) return filename?.EmptyAsNull() ?? "/mnt/temp/fakefile.mkv";
            
        filename ??= WorkingFile;
        if (filename.StartsWith(TempPath))
            return filename;
        string dest = Path.Combine(TempPath, new FileInfo(filename).Name);
        File.Copy(filename, dest);
        return dest;
    }

    /// <summary>
    /// Creates a directory if it does not already exist
    /// </summary>
    /// <param name="directory">the directory path</param>
    /// <returns>true if the directory now exists</returns>
    public bool CreateDirectoryIfNotExists(string directory)
    {
        if (Fake) return true;
        return FileService.DirectoryCreate(directory).ValueOrDefault;
    }

    /// <summary>
    /// Executes a cmd and returns the result
    /// </summary>
    /// <param name="args">The execution parameters</param>
    /// <returns>The result of the command</returns>
    public ProcessResult Execute(ExecuteArgs args)
    {
        if (Fake) return new ProcessResult {  ExitCode = 0, Completed = true };
        
        var result = Process.ExecuteShellCommand(args).Result;
        return result;
    }


    /// <summary>
    /// Gets a new guid as a string
    /// </summary>
    /// <returns>a new guid as a string</returns>
    public string NewGuid() => Guid.NewGuid().ToString();


    /// <summary>
    /// Loads the plugin settings
    /// </summary>
    /// <typeparam name="T">The plugin settings to load</typeparam>
    /// <returns>The plugin settings</returns>
    public T GetPluginSettings<T>() where T : IPluginSettings
    {
        if (Fake) return default;
        var name = typeof(T).Namespace;
        if (string.IsNullOrEmpty(name))
            return default;
        name = name.Substring(name.IndexOf(".", StringComparison.Ordinal) + 1);
        string json = GetPluginSettingsJson(name);
        if (string.IsNullOrEmpty(json))
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Logger.ELog("Failed deserializing plugin settings: " + ex.Message + Environment.NewLine + json);
            throw new Exception("Failed deserializing plugin settings: " + ex.Message);
        }
    }

    /// <summary>
    /// Gets the physical path of a tool
    /// Note: this is the unmapped path and if on a remote node will have to be mapped
    /// </summary>
    /// <param name="tool">the name of the tool to get</param>
    /// <returns>the physical path of a tool</returns>
    public string GetToolPath(string tool)
    {
        if (Fake || GetToolPathActual == null) return string.Empty;
        return GetToolPathActual(tool);
    }

    /// <summary>
    /// Gets a variable from the variable list if exists
    /// </summary>
    /// <param name="name">the name of the variable</param>
    /// <returns>the value of the variable, else null if not found</returns>
    public object? GetVariable(string name)
    {
        if(this.Variables?.ContainsKey(name) == true)
            return this.Variables[name];
        return null;
    }

    /// <summary>
    /// Tests if a input string matches a variable
    /// </summary>
    /// <param name="variableName">The name of the variable</param>
    /// <param name="input">the input string</param>
    /// <returns>true if matches, otherwise false</returns>
    public bool MatchesVariable(string variableName, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.ILog("Input not set, does not match anything");
            return false;
        }

        var variable = GetToolPathActual(variableName);
        if (string.IsNullOrEmpty(variable))
        {
            Logger.WLog("Variable not found: " + variableName);
            return false;
        }

        foreach (var line in variable.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.Equals(line, input, StringComparison.InvariantCultureIgnoreCase))
                return true;
            int lastIndex = line.LastIndexOf("/", StringComparison.Ordinal);
            if (line.StartsWith("/") && lastIndex > 0)
            {
                // try a regex
                try
                {
                    string rgxCompare = line[1..lastIndex];
                    string opt = line.Substring(lastIndex + 1);
                    var options = RegexOptions.None;
                    if (opt.IndexOf("i", StringComparison.Ordinal) >= 0)
                        options |= RegexOptions.IgnoreCase;
                    var rgx = new Regex(rgxCompare, options);
                    if (rgx.IsMatch(input))
                        return true;
                }
                catch (Exception)
                {

                }
            }
        }

        return false;
    }

    /// <summary>
    /// Records a statistic with the server
    /// </summary>
    /// <param name="name">the name of the statistic</param>
    /// <param name="value">the value of the statistic</param>
    public void RecordStatistic(string name, object value) => StatisticRecorder?.Invoke(name, value);
}


/// <summary>
/// The possible results of a executed node/flow part
/// </summary>
public enum NodeResult
{
    /// <summary>
    /// The execution failed
    /// </summary>
    Failure = 0,
    /// <summary>
    /// The execution succeeded
    /// </summary>
    Success = 1,
}
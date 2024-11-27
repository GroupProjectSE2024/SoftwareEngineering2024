/******************************************************************************
 * Filename    = MainPageViewModel.FileWatcher.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Updates the UI if any file gets cloned or gets newly created
 *               in the directory which is set now.
 *****************************************************************************/
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    private readonly FileSystemWatcher _watcher = new();
    /// <summary>
    /// Watches the file present at the given path.
    /// </summary>
    /// <param name="path">Since that is the file we are viewing at UI. Path will be the root directory path</param>
    public void WatchFile(string path)
    {
        Trace.WriteLine($"Started watching at {path}");
        _watcher.Path = path;

        //The following changes are notified to the watcher
        _watcher.NotifyFilter = NotifyFilters.Attributes |
        NotifyFilters.DirectoryName |
        NotifyFilters.FileName |
        NotifyFilters.LastWrite |
        NotifyFilters.Size;

        //Watching all kinds of files. * is a wildcard which means changes in all files
        //with all extensions are watched upon.
        _watcher.Filter = "*.*";

        //Setting event handlers for the changes
        _watcher.Created += new FileSystemEventHandler(OnChanged);
        _watcher.Deleted += new FileSystemEventHandler(OnChanged);
        _watcher.Changed += new FileSystemEventHandler(OnChanged);
        _watcher.Renamed += new RenamedEventHandler(OnRenamed);

        _watcher.EnableRaisingEvents = true;
    }

    //Update the UI as and when the name of an object is changed.
    [ExcludeFromCodeCoverage]
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }

    //Update the UI as and when the a file is created or deleted.
    [ExcludeFromCodeCoverage]
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }
}

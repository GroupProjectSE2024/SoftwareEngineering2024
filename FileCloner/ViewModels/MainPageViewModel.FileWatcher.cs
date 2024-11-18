﻿/******************************************************************************
 * Filename    = MainPageViewModel.FileWatcher.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Updates the UI if any file gets cloned or gets newly created
 *               in the directory which we are referring to now.
 *****************************************************************************/

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    public void WatchFile(string path)
    {
        Trace.WriteLine($"Started watching at {path}");
        using FileSystemWatcher watcher = new();
        watcher.Path = path;
        watcher.NotifyFilter = NotifyFilters.Attributes |
        NotifyFilters.DirectoryName |
        NotifyFilters.FileName |
        NotifyFilters.LastWrite |
        NotifyFilters.Size;
        watcher.Filter = "*.*";
        watcher.Created += new FileSystemEventHandler(OnChanged);
        //watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Deleted += new FileSystemEventHandler(OnChanged);
        watcher.Renamed += new RenamedEventHandler(OnRenamed);

        watcher.EnableRaisingEvents = true;
        while (true)
        {
            ;
        }
    }

    [ExcludeFromCodeCoverage]
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }
    [ExcludeFromCodeCoverage]
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }
}

/******************************************************************************
* Filename    = TestFileChangeNotifier.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Unit Tests for FileChangeNotifier.cs
*****************************************************************************/
using ViewModel.UpdaterViewModel;

namespace TestsUpdater;

/// <summary>
/// Unit test class for FileMonitor. This class contains tests that simulate file events 
/// (creation, deletion) and verify if the FileMonitor's MessageStatus is updated correctly.
/// </summary>
[TestClass]
public class TestFileChangeNotifier
{

    private FileChangeNotifier? _fileMonitor;
    private readonly string _testFolderPath = @"C:\temp";

    /// <summary>
    /// Setup method to initialize the FileMonitor instance before each test and ensure the folder exists.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        // Create folder if it doesn't exist (to mimic production behavior)
        if (!Directory.Exists(_testFolderPath))
        {
            Directory.CreateDirectory(_testFolderPath);
        }

        // Initialize the FileMonitor before every test
        _fileMonitor = new FileChangeNotifier();
    }

    /// <summary>
    /// Cleanup method to remove any test-created files and folder after each test.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        // Remove all files in the directory
        if (Directory.Exists(_testFolderPath))
        {
            foreach (string? file in Directory.GetFiles(_testFolderPath))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Test method to simulate file creation and verify that the MessageStatus reflects the 
    /// file creation event correctly.
    /// </summary>
    [TestMethod]
    public void TestFileCreatedUpdateMessageStatus()
    {
        //Define a test file path that will simulate the file creation
        string testFilePath = @"C:\temp\createfile.dll";

        // Act: Simulate file creation event using reflection to call private method
        _fileMonitor?.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, [this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(testFilePath ?? "") ?? "", Path.GetFileName(testFilePath))]);

        // Simulate timer elapse
        Thread.Sleep(1100);

        // Check if the MessageStatus reflects the correct message
        Assert.AreEqual("Files created: createfile.dll", _fileMonitor?.MessageStatus?.TrimEnd());
    }

    /// <summary>
    /// Test method to simulate file deletion and verify that the MessageStatus reflects the 
    /// file deletion event correctly.
    /// </summary>
    [TestMethod]
    public void TestFileDeletedUpdateMessageStatus()
    {
        //Define a test file path for deletion
        string testFilePath = @"C:\temp\deletefile.dll";

        // Simulate file deletion event using reflection
        _fileMonitor?.GetType()
            .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, [this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(testFilePath ?? "") ?? "", Path.GetFileName(testFilePath))]);

        // Simulate timer elapse
        Thread.Sleep(1100);

        // Assert
        Assert.AreEqual("Files removed: deletefile.dll", _fileMonitor?.MessageStatus?.TrimEnd());
    }

    /// <summary>
    /// Test method to simulate multiple file creation and deletion events and verify that 
    /// the MessageStatus reflects all file changes correctly.
    /// </summary>
    [TestMethod]
    public void TestMultipleFilesUpdateMessageStatus()
    {
        //Define multiple file paths for testing multiple events
        string file1 = @"C:\temp\file1.dll";
        string file2 = @"C:\temp\file2.dll";
        string deletedFile = @"C:\temp\deletedfile.dll";

        // Simulate multiple file events using reflection
        _fileMonitor?.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, [this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file1 ?? "") ?? "", Path.GetFileName(file1))]);

        _fileMonitor?.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, [this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file1 ?? "") ?? "", Path.GetFileName(file2))]);

        _fileMonitor?.GetType()
            .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, [this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(file1 ?? "") ?? "", Path.GetFileName(deletedFile))]);

        // Simulate timer elapse
        Thread.Sleep(1100);

        // Ensure that the MessageStatus is correctly updated for multiple files
        Assert.AreEqual(
            "Files created: file1.dll, file2.dll\nFiles removed: deletedfile.dll".Replace("\r\n", "\n").Trim(),
            _fileMonitor?.MessageStatus?.Replace("\r\n", "\n").Trim()
        );
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileClonerTestCases;

[TestClass]
public class FileExplorerServiceProviderTests
{
    private FileExplorerServiceProvider _serviceProvider;
    private string _testRootDir;

    [TestInitialize]
    public void Setup()
    {
        _serviceProvider = new FileExplorerServiceProvider();
        _testRootDir = Path.Combine(Path.GetTempPath(), "FileExplorerServiceProviderTests");
        Directory.CreateDirectory(_testRootDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testRootDir))
        {
            Directory.Delete(_testRootDir, true);
        }
    }

    [TestMethod]
    public void CleanFolder_RemovesAllFilesInFolder()
    {
        string testDir = Path.Combine(_testRootDir, "CleanFolderTest");
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file1.txt"), "test");
        File.WriteAllText(Path.Combine(testDir, "file2.txt"), "test");

        _serviceProvider.CleanFolder(testDir);

        Assert.AreEqual(0, Directory.GetFiles(testDir).Length);
    }

    [TestMethod]
    public void CleanFolder_NoOpOnNonexistentFolder()
    {
        string nonexistentDir = Path.Combine(_testRootDir, "Nonexistent");

        _serviceProvider.CleanFolder(nonexistentDir);

        Assert.IsFalse(Directory.Exists(nonexistentDir));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GenerateInputFile_ThrowsIfSourceDirDoesNotExist()
    {
        string nonexistentDir = Path.Combine(_testRootDir, "Nonexistent");

        _serviceProvider.GenerateInputFile(nonexistentDir);
    }

    [TestMethod]
    public void GenerateInputFile_CreatesValidJsonFile()
    {
        string testDir = Path.Combine(_testRootDir, "TestDir");
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file.txt"), "content");

        _serviceProvider.GenerateInputFile(testDir);

        string inputFilePath = Constants.InputFilePath;
        Assert.IsTrue(File.Exists(inputFilePath));

        string jsonOutput = File.ReadAllText(inputFilePath);
        Dictionary<string, object>? deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonOutput);
        Assert.IsNotNull(deserialized);
    }

    [TestMethod]
    public void ParseDirectory_ReturnsValidStructureForEmptyDirectory()
    {
        Dictionary<string, object> result = InvokeParseDirectory(_testRootDir, _testRootDir);

        Assert.IsNotNull(result);
        Assert.AreEqual(_testRootDir, result["FULL_PATH"]);
        Assert.AreEqual("WHITE", result["COLOR"]);
        Assert.AreEqual(0, ((Dictionary<string, object>)result["CHILDREN"]).Count);
    }

    [TestMethod]
    public void ParseDirectory_ReturnsStructureWithFilesAndSubdirectories()
    {
        string subDir = Path.Combine(_testRootDir, "SubDir");
        string filePath = Path.Combine(_testRootDir, "file.txt");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(filePath, "content");

        Dictionary<string, object> result = InvokeParseDirectory(_testRootDir, _testRootDir);

        Assert.IsNotNull(result);
        var children = (Dictionary<string, object>)result["CHILDREN"];
        Assert.IsTrue(children.ContainsKey("SubDir"));
        Assert.IsTrue(children.ContainsKey("file.txt"));
    }

    [TestMethod]
    public void ParseDirectory_FileAttributesAreCorrect()
    {
        string filePath = Path.Combine(_testRootDir, "file.txt");
        File.WriteAllText(filePath, "content");

        Dictionary<string, object> result = InvokeParseDirectory(_testRootDir, _testRootDir);
        var children = (Dictionary<string, object>)result["CHILDREN"];
        var fileData = (Dictionary<string, object>)children["file.txt"];

        Assert.AreEqual("WHITE", fileData["COLOR"]);
        Assert.AreEqual(new FileInfo(filePath).FullName, fileData["FULL_PATH"]);
        Assert.AreEqual("file.txt", Path.GetFileName(fileData["RELATIVE_PATH"].ToString()));
    }

    private Dictionary<string, object> InvokeParseDirectory(string dirPath, string sourceDirPath)
    {
        System.Reflection.MethodInfo? method = typeof(FileExplorerServiceProvider).GetMethod("ParseDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Dictionary<string, object>)method.Invoke(_serviceProvider, new object[] { dirPath, sourceDirPath });
    }
}

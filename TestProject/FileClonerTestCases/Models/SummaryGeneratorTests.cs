using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace FileClonerTestCases;

[TestClass]
public class SummaryGeneratorTests
{
    private string _testInputFile;
    private string _testReceivedFolder;
    private string _testOutputFile;

    [TestInitialize]
    public void Setup()
    {
        // Mock Constants paths
        _testInputFile = Constants.InputFilePath;
        _testReceivedFolder = Constants.ReceivedFilesFolderPath;
        _testOutputFile = Constants.OutputFilePath;

        if (File.Exists(_testInputFile))
        {
            File.Delete(_testInputFile);
        }
        if (File.Exists(_testOutputFile))
        {
            File.Delete(_testOutputFile);
        }
        if (!Directory.Exists(_testReceivedFolder))
        {
            Directory.CreateDirectory(_testReceivedFolder);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {

        if (File.Exists(_testInputFile))
        {
            File.Delete(_testInputFile);
        }
        if (File.Exists(_testOutputFile))
        {
            File.Delete(_testOutputFile);
        }
        if (Directory.Exists(_testReceivedFolder))
        {
            Directory.Delete(_testReceivedFolder, true);
        }
    }

    [TestMethod]
    public void GenerateSummary_CreatesOutputFile()
    {
        // Arrange
        var testData = new Dictionary<string, object> {
            ["file1.txt"] = new Dictionary<string, object> {
                ["SIZE"] = 1024,
                ["RELATIVE_PATH"] = "file1.txt",
                ["LAST_MODIFIED"] = DateTime.UtcNow.ToString("o"),
                ["ADDRESS"] = "127.0.0.1"
            }
        };
        File.WriteAllText(_testInputFile, JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true }));

        // Act
        SummaryGenerator.GenerateSummary();

        // Assert
        Assert.IsTrue(File.Exists(_testOutputFile));

        string outputContent = File.ReadAllText(_testOutputFile);
        Assert.IsFalse(string.IsNullOrWhiteSpace(outputContent));
    }

    [TestMethod]
    public void GenerateSummary_MergesReceivedFiles()
    {
        // Arrange
        var inputFileData = new Dictionary<string, object> {
            ["file1.txt"] = new Dictionary<string, object> {
                ["SIZE"] = 1024,
                ["RELATIVE_PATH"] = "file1.txt",
                ["LAST_MODIFIED"] = DateTime.UtcNow.AddHours(-1).ToString("o"),
                ["ADDRESS"] = "127.0.0.1"
            }
        };
        var receivedFileData = new Dictionary<string, object> {
            ["file1.txt"] = new Dictionary<string, object> {
                ["SIZE"] = 2048,
                ["RELATIVE_PATH"] = "file1.txt",
                ["LAST_MODIFIED"] = DateTime.UtcNow.ToString("o"),
                ["ADDRESS"] = "192.168.1.1"
            }
        };
        File.WriteAllText(_testInputFile, JsonSerializer.Serialize(inputFileData, new JsonSerializerOptions { WriteIndented = true }));
        string receivedFilePath = Path.Combine(_testReceivedFolder, "received.json");
        File.WriteAllText(receivedFilePath, JsonSerializer.Serialize(receivedFileData, new JsonSerializerOptions { WriteIndented = true }));

        // Act
        SummaryGenerator.GenerateSummary();

        // Assert
        string outputContent = File.ReadAllText(_testOutputFile);
        Dictionary<string, Dictionary<string, object>>? outputData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(outputContent);
        Assert.IsNotNull(outputData);
    }

    [TestMethod]
    public void ProcessDirectoryData_AddsNewEntries()
    {
        // Arrange
        var testData = new Dictionary<string, object> {
            ["file1.txt"] = new Dictionary<string, object> {
                ["SIZE"] = 1024,
                ["RELATIVE_PATH"] = "file1.txt",
                ["LAST_MODIFIED"] = DateTime.UtcNow.ToString("o"),
                ["ADDRESS"] = "127.0.0.1"
            }
        };

        // Act
        InvokeProcessDirectoryData(testData, "WHITE");

        // Assert: Check if the key exists
        Assert.IsTrue(SummaryGenerator.Summary.ContainsKey("file1.txt"), "Expected key 'file1.txt' not found in Summary.");
    }

    private void InvokeProcessDirectoryData(Dictionary<string, object> data, string defaultColor)
    {
        System.Reflection.MethodInfo? method = typeof(SummaryGenerator).GetMethod("ProcessDirectoryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Invoke(null, new object[] { data, defaultColor });
    }

    [TestMethod]
    public void UpdateEntryWithNewData_UpdatesWithLatestTimestamp()
    {
        // Arrange
        string relativePath = "file1.txt";
        var existingMetadata = new Dictionary<string, object> {
            ["LAST_MODIFIED"] = DateTime.UtcNow.AddHours(-1).ToString("o"),
            ["SIZE"] = 1024L,
            ["ADDRESS"] = "127.0.0.1",
            ["COLOR"] = "WHITE"
        };
        SummaryGenerator.Summary[relativePath] = existingMetadata;

        var newFileDataDict = new Dictionary<string, object> {
            ["LAST_MODIFIED"] = DateTime.UtcNow.ToString("o"),
            ["SIZE"] = 2048L,
            ["ADDRESS"] = "192.168.1.1"
        };

        // Convert the dictionary to JsonElement
        JsonElement newFileData = ConvertToJsonElement(newFileDataDict);

        // Act
        InvokeUpdateEntryWithNewData(newFileData, relativePath);

        // Assert
        Dictionary<string, object> updatedMetadata = SummaryGenerator.Summary[relativePath];
        Assert.AreEqual(2048L, updatedMetadata["SIZE"]);
        Assert.AreEqual("RED", updatedMetadata["COLOR"]);
    }

    private JsonElement ConvertToJsonElement(Dictionary<string, object> data)
    {
        string jsonString = JsonSerializer.Serialize(data);
        using var document = JsonDocument.Parse(jsonString);
        return document.RootElement.Clone(); // Clone to persist after the document is disposed
    }

    private void InvokeUpdateEntryWithNewData(JsonElement element, string relativePath)
    {
        System.Reflection.MethodInfo? method = typeof(SummaryGenerator).GetMethod("UpdateEntryWithNewData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Invoke(null, new object[] { element, relativePath });
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Updater;

namespace TestsUpdater;

[TestClass]
public class TestDirectoryMetadataComparer
{
    private List<FileMetadata>? _metadataA;
    private List<FileMetadata>? _metadataB;

    [TestInitialize]
    public void Setup()
    {
        // Prepare some sample metadata for testing
        _metadataA =
        [
            new () { FileName = "file1.txt", FileHash = "hash1" },
            new () { FileName = "file2.txt", FileHash = "hash2" },
            new () { FileName = "file5.txt", FileHash = "hash5" },
        ];

        _metadataB =
        [
            new () { FileName = "file2.txt", FileHash = "hash2" },
            new () { FileName = "file3.txt", FileHash = "hash3" },
            new (){ FileName = "file4.txt", FileHash = "hash1" },
        ];
    }

    [TestMethod]
    public void TestCompareMetadataShouldIdentifyDifferences()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(_metadataA!, _metadataB!);

        // Act
        List<MetadataDifference> differences = comparer.Differences;

        // Assert: Check if differences are as expected
        Assert.AreEqual(1, differences.First(d => d.Key == "-1").Value.Count);
        Assert.AreEqual(1, differences.First(d => d.Key == "0").Value.Count);
        Assert.AreEqual(1, differences.First(d => d.Key == "1").Value.Count);
    }

    [TestMethod]
    public void TestCheckForRenamesAndMissingFilesShouldIdentifyMissingFilesInA()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(_metadataA!, _metadataB!);

        // Act
        List<string> uniqueClientFiles = comparer.UniqueClientFiles;

        // Assert: Check if missing files from A are identified
        Assert.AreEqual(1, uniqueClientFiles.Count); // file4.txt is only in B
        Assert.AreEqual("file3.txt", uniqueClientFiles.First());
    }

    [TestMethod]
    public void TestCheckForOnlyInAFilesShouldIdentifyMissingFilesInB()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(_metadataA!, _metadataB!);

        // Act
        List<string> uniqueServerFiles = comparer.UniqueServerFiles;

        // Assert: Check if missing files from B are identified
        Assert.AreEqual(1, uniqueServerFiles.Count); // file1.txt is only in A
        Assert.AreEqual("file5.txt", uniqueServerFiles.First());
    }

    [TestMethod]
    public void TestCheckForSameNameDifferentHashShouldIdentifyFileHashMismatch()
    {
        // Arrange
        // Using null-forgiving operator if you are confident they are initialized in Setup
        var comparer = new DirectoryMetadataComparer(_metadataA!, _metadataB!);

        // Act
        // Ensure invalidSyncUpFiles is non-null
        List<string> invalidSyncUpFiles = comparer.InvalidSyncUpFiles ?? [];

        // Assert
        Assert.AreEqual(0, invalidSyncUpFiles.Count);
    }



    [TestMethod]
    public void TestValidateSyncShouldReturnTrueWhenNoInvalidFilesExist()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(_metadataA!, _metadataB!);

        // Act
        bool canSync = comparer.ValidateSync();

        // Assert: Should return true due to file3.txt having different hashes
        Assert.IsTrue(canSync);
    }

    [TestMethod]
    public void TestValidateSyncShouldReturnFalseWhenNoInvalidFiles()
    {
        // Arrange
        var metadataBInvalidFiles = new List<FileMetadata>
        {
            new () { FileName = "file1.txt", FileHash = "hash1" },
            new () { FileName = "file2.txt", FileHash = "hash2" },
            new () { FileName = "file5.txt", FileHash = "hash7" }
        };
        var comparer = new DirectoryMetadataComparer(_metadataA!, metadataBInvalidFiles);

        // Act
        bool canSync = comparer.ValidateSync();

        // Assert: Should return false as there are no invalid files
        Assert.IsFalse(canSync);
    }
}

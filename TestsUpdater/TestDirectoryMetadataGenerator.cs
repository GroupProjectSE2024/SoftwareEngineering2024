using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;

namespace TestsUpdater;
[TestClass]
public class TestDirectoryMetadataGenerator
{
    private string _testDirectory = "";

    [TestInitialize]
    public void SetUp()
    {
        // Setup a temporary directory for testing
        // GetTempPath() returns system's temp directory
        // Guid.NewGuid() generates a unique identifier across system, network and databases
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void CleanUp()
    {
        // Clean up the test directory after each test
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public void TestConstructorShouldCreateDirectoryWhenDirectoryDoesNotExist()
    {
        // Arrange
        string nonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        _ = new DirectoryMetadataGenerator(nonExistentDirectory);

        // Assert
        Assert.IsTrue(Directory.Exists(nonExistentDirectory), "Directory was not created.");

        // Clean up
        if (Directory.Exists(nonExistentDirectory))
        {
            Directory.Delete(nonExistentDirectory);
        }
    }

    [TestMethod]
    public void TestConstructorShouldInitializeMetadataWhenDirectoryExists()
    {
        // Arrange
        string fileName = "testfile.txt";
        string filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, "Test file content");

        // Act
        var generator = new DirectoryMetadataGenerator(_testDirectory);
        List<FileMetadata>? metadata = generator.GetMetadata();

        // Assert
        Assert.IsNotNull(metadata, "Metadata should not be null.");
        Assert.AreEqual(1, metadata?.Count, "Metadata count should be 1.");
        Assert.AreEqual(fileName, metadata?[0].FileName, "File name in metadata is incorrect.");
    }

    [TestMethod]
    public void TestCreateFileMetadataShouldGenerateCorrectMetadataWhenFilesExist()
    {
        // Arrange
        string fileName1 = "file1.txt";
        string fileName2 = "file2.txt";
        string filePath1 = Path.Combine(_testDirectory, fileName1);
        string filePath2 = Path.Combine(_testDirectory, fileName2);
        File.WriteAllText(filePath1, "File 1 content");
        File.WriteAllText(filePath2, "File 2 content");

        // Act
        List<FileMetadata> metadata = DirectoryMetadataGenerator.CreateFileMetadata(_testDirectory);

        HashSet<string> fileNames = [];
        foreach (FileMetadata file in metadata)
        {
            if (!string.IsNullOrEmpty(file.FileName))
            {
                fileNames.Add(file.FileName);
            }
        }

        bool file1Exists = fileNames.Contains(fileName1);
        bool file2Exists = fileNames.Contains(fileName2);

        // Assert
        Assert.IsNotNull(metadata, "Metadata should not be null.");
        Assert.AreEqual(2, metadata.Count, "Metadata count is incorrect.");
        Assert.IsTrue(file1Exists, "File 1 not found in metadata.");
        Assert.IsTrue(file2Exists, "File 2 not found in metadata.");
    }

    [TestMethod]
    public void TestComputeFileHashShouldReturnCorrectHash()
    {
        // Arrange
        string fileName = "file1.txt";
        string filePath = Path.Combine(_testDirectory, fileName);
        string expectedHash = "a1ace82b7a6631e74beac3d68969d62340ad4ee55626bd2729c558d479613ded"; // Precomputed for "File 1 content"
        File.WriteAllText(filePath, "File 1 content");

        // Act
        string fileHash = DirectoryMetadataGenerator.ComputeFileHash(filePath);

        // Assert
        Assert.AreEqual(expectedHash, fileHash, $"File hash is incorrect. \n Expected: {expectedHash},\n Actual: {fileHash}");
    }

    [TestMethod]
    public void TestCreateFileMetadataShouldReturnEmptyListWhenDirectoryHasNoFiles()
    {
        // Arrange
        string emptyDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(emptyDirectory);

        // Act
        List<FileMetadata> metadata = DirectoryMetadataGenerator.CreateFileMetadata(emptyDirectory);

        // Assert
        Assert.IsNotNull(metadata, "Metadata should not be null.");
        Assert.AreEqual(0, metadata.Count, "Metadata count should be 0.");

        // Clean up
        if (Directory.Exists(emptyDirectory))
        {
            Directory.Delete(emptyDirectory);
        }
    }

    [TestMethod]
    public void TestConstructorShouldLogMessageWhenDirectoryDoesNotExist()
    {
        // Arrange
        string nonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        using var stringWriter = new System.IO.StringWriter();
        // Add a `trace` listener to capture the log message
        // Now, log message will be captured in the stringWriter, so that we can asses log messages
        Trace.Listeners.Add(new TextWriterTraceListener(stringWriter));

        // Act
        var generator = new DirectoryMetadataGenerator(nonExistentDirectory);

        // Assert
        string output = stringWriter.ToString();
        Assert.IsTrue(output.Contains("Directory does not exist"), "Log message not found.");

        // Remove the listener from `Trace.Listeners` collection to clean up after the test
        Trace.Listeners.RemoveAt(Trace.Listeners.Count - 1);
    }
}

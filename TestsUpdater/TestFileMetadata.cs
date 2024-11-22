using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;
namespace TestsUpdater;

[TestClass]
public class TestFileMetadata
{
    /// <summary>
    /// Verifies that the default constructor sets both FileName and FileHash to null.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataDefaultConstructor()
    {
        // Arrange & Act
        var fileMetadata = new FileMetadata();

        // Assert
        Assert.IsNull(fileMetadata.FileName);
        Assert.IsNull(fileMetadata.FileHash);
    }

    /// <summary>
    /// Verifies that the constructor sets the FileName and FileHash correctly when valid values are provided.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataConstructorWithValidParams()
    {
        // Arrange
        string fileName = "example.txt";
        string fileHash = "abc123";

        // Act
        var fileMetadata = new FileMetadata {
            FileName = fileName,
            FileHash = fileHash
        };

        // Assert
        Assert.AreEqual(fileName, fileMetadata.FileName);
        Assert.AreEqual(fileHash, fileMetadata.FileHash);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both properties are null.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataToStringBothPropertiesNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata();

        // Act
        string result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, FileHash: N/A", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when FileName is null and FileHash is set.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataToStringFileNameNullFileHashNotNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = null,
            FileHash = "abc123"
        };

        // Act
        string result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, FileHash: abc123", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when FileName is set and FileHash is null.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataToStringFileNameNotNullFileHashNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = "example.txt",
            FileHash = null
        };

        // Act
        string result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, FileHash: N/A", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both FileName and FileHash are set.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataToStringBothPropertiesNotNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = "example.txt",
            FileHash = "abc123"
        };

        // Act
        string result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, FileHash: abc123", result);
    }

    /// <summary>
    /// Verifies that FileName and FileHash handle empty strings correctly.
    /// </summary>
    [TestMethod]
    public void TestFileMetadataConstructorEmptyStrings()
    {
        // Arrange
        string fileName = string.Empty;
        string fileHash = string.Empty;

        // Act
        var fileMetadata = new FileMetadata {
            FileName = fileName,
            FileHash = fileHash
        };

        // Assert
        Assert.AreEqual(fileName, fileMetadata.FileName);
        Assert.AreEqual(fileHash, fileMetadata.FileHash);
    }
}

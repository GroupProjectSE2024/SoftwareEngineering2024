using Updater;

namespace TestsUpdater;

[TestClass]
public class TestFileContent
{
    /// <summary>
    /// Verifies that the default constructor sets both FileName and SerializedContent to null.
    /// </summary>
    [TestMethod]
    public void TestFileContentDefaultConstructor()
    {
        // Arrange & Act
        var fileContent = new FileContent();

        // Assert
        Assert.IsNull(fileContent.FileName);
        Assert.IsNull(fileContent.SerializedContent);
    }

    /// <summary>
    /// Verifies that the constructor sets the FileName and SerializedContent correctly when valid values are provided.
    /// </summary>
    [TestMethod]
    public void TestFileContentConstructorWithValidParams()
    {
        // Arrange
        string fileName = "example.txt";
        string serializedContent = "Some content";

        // Act
        var fileContent = new FileContent(fileName, serializedContent);

        // Assert
        Assert.AreEqual(fileName, fileContent.FileName);
        Assert.AreEqual(serializedContent, fileContent.SerializedContent);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both properties are null.
    /// </summary>
    [TestMethod]
    public void TestFileContentToStringForDefaultConstructor()
    {
        // Arrange
        var fileContent = new FileContent();

        // Act
        string result = fileContent.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, Content Length: 0", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both FileName and SerializedContent are not null.
    /// </summary>
    [TestMethod]
    public void TestFileContentToStringBothPropertiesNotNull()
    {
        // Arrange
        var fileContent = new FileContent("example.txt", "Some content");

        // Act
        string result = fileContent.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, Content Length: 12", result);
    }
}

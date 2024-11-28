using System.Net;
using FileCloner.Models;

namespace FileClonerTestCases;

[TestClass]
public class ConstantsTests
{
    [TestMethod]
    public void Verify_BasePath()
    {
        // Act
        string expectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FileCloner");

        // Assert
        Assert.AreEqual(expectedPath, Constants.BasePath);
    }

    [TestMethod]
    public void Verify_IconPaths()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "FileCloner", "Assets", "Images"));

        // Act & Assert
        Assert.AreEqual(Path.Combine(basePath, "loading.png"), Constants.LoadingIconPath);
        Assert.AreEqual(Path.Combine(basePath, "file.png"), Constants.FileIconPath);
        Assert.AreEqual(Path.Combine(basePath, "folder.png"), Constants.FolderIconPath);
    }

    [TestMethod]
    public void Verify_FileAndFolderPaths()
    {
        // Arrange
        string expectedDefaultFolderPath = Path.Combine(Constants.BasePath, "Temp");
        string expectedInputFilePath = Path.Combine(Constants.BasePath, "input.json");
        string expectedOutputFilePath = Path.Combine(Constants.BasePath, "output.json");
        string expectedReceivedFilesFolderPath = Path.Combine(Constants.BasePath, "ReceivedFiles");
        string expectedSenderFilesFolderPath = Path.Combine(Constants.BasePath, "SenderFiles");

        // Assert
        Assert.AreEqual(expectedDefaultFolderPath, Constants.DefaultFolderPath);
        Assert.AreEqual(expectedInputFilePath, Constants.InputFilePath);
        Assert.AreEqual(expectedOutputFilePath, Constants.OutputFilePath);
        Assert.AreEqual(expectedReceivedFilesFolderPath, Constants.ReceivedFilesFolderPath);
        Assert.AreEqual(expectedSenderFilesFolderPath, Constants.SenderFilesFolderPath);
    }

    [TestMethod]
    public void Verify_NetworkServiceConstants()
    {
        // Assert
        Assert.AreEqual("success", Constants.Success);
        Assert.AreEqual("FileCloner", Constants.ModuleName);
        Assert.AreEqual("request", Constants.Request);
        Assert.AreEqual("response", Constants.Response);
        Assert.AreEqual("summary", Constants.Summary);
        Assert.AreEqual("cloning", Constants.Cloning);
        Assert.AreEqual("BroadCast", Constants.Broadcast);
    }

    [TestMethod]
    public void Verify_FileChunkConstants()
    {
        // Assert
        Assert.AreEqual(13 * 1024 * 1024, Constants.FileChunkSize);
        Assert.AreEqual(1, Constants.ChunkStartIndex);
    }

    [TestMethod]
    public void Verify_IPAddress()
    {
        // Act
        string ipAddress = Constants.IPAddress;

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(ipAddress), "IP Address should not be empty.");
        Assert.IsTrue(IPAddress.TryParse(ipAddress, out _), "IP Address should be a valid IP address.");
    }

    [TestMethod]
    public void GetIP_ReturnsNonLoopbackIPv4Address()
    {
        // Arrange
        string ipAddress = Constants.IPAddress;

        // Act & Assert
        Assert.IsFalse(ipAddress.EndsWith(".1"), "IP address should not end with .1");
        Assert.IsTrue(IPAddress.TryParse(ipAddress, out _), "The returned IP address should be valid.");
    }
}

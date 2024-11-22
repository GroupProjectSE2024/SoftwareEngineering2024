using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;

namespace TestsUpdater;

/// <summary>
/// Tests for the DataPacket class
/// </summary>
[TestClass]
public class TestDataPacket
{

    /// <summary>
    /// Shitty test because of parameterless constructor
    /// Verifies that the DataPacket type defaults to SyncUp if no type is explicitly set.
    /// </summary>
    [TestMethod]
    public void TestDataPacketConstructorWithoutType()
    {
        // Arrange & Act
        var dataPacket = new DataPacket();

        // Assert
        Assert.AreEqual(DataPacket.PacketType.SyncUp, dataPacket.DataPacketType); // Defaults to SyncUp
    }

    /// <summary>
    /// Test default constructor
    /// </summary>
    [TestMethod]
    public void TestDataPacketDefaultConstructor()
    {
        // Arrange & Act
        var dataPacket = new DataPacket();

        // Assert
        // Parameterless constructor should default to SyncUp
        // Parameterless constructor is only for xml serialization
        Assert.AreEqual(DataPacket.PacketType.SyncUp, dataPacket.DataPacketType);
        Assert.AreEqual(0, dataPacket.FileContentList.Count); // Default value for FileContentList
    }

    /// <summary>
    /// Tests datapacket constructor with multiple files
    /// </summary>
    [TestMethod]
    public void TestDataPacketConstructorWithMultipleFiles()
    {
        // Arrange
        var fileContents = new List<FileContent>
            {
                new("file1.txt", "Content1"),
                new("file2.txt", "Content2")
            };
        DataPacket.PacketType packetType = DataPacket.PacketType.Differences;

        // Act
        var dataPacket = new DataPacket(packetType, fileContents);

        // Assert
        Assert.AreEqual(packetType, dataPacket.DataPacketType); // Ensures the packet type is set correctly
        Assert.AreEqual(2, dataPacket.FileContentList.Count); // Ensures two file contents are added

        Assert.AreEqual("file1.txt", dataPacket.FileContentList[0].FileName);
        Assert.AreEqual("Content1", dataPacket.FileContentList[0].SerializedContent);

        Assert.AreEqual("file2.txt", dataPacket.FileContentList[1].FileName);
        Assert.AreEqual("Content2", dataPacket.FileContentList[1].SerializedContent);
    }
}

using FileCloner.Models.NetworkService;

namespace FileClonerTestCases;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var message = new Message();

        // Assert
        Assert.AreEqual("", message.Subject, "Default Subject should be an empty string.");
        Assert.AreEqual(-1, message.RequestID, "Default RequestID should be -1.");
        Assert.AreEqual("", message.From, "Default From should be an empty string.");
        Assert.AreEqual("", message.To, "Default To should be an empty string.");
        Assert.AreEqual("", message.MetaData, "Default MetaData should be an empty string.");
        Assert.AreEqual("", message.Body, "Default Body should be an empty string.");
    }

    [TestMethod]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var message = new Message {
            Subject = "Request",
            RequestID = 42,
            From = "192.168.1.1",
            To = "192.168.1.2",
            MetaData = "FilePath=/docs/example.txt",
            Body = "This is the file content"
        };

        // Assert
        Assert.AreEqual("Request", message.Subject, "Subject was not set correctly.");
        Assert.AreEqual(42, message.RequestID, "RequestID was not set correctly.");
        Assert.AreEqual("192.168.1.1", message.From, "From was not set correctly.");
        Assert.AreEqual("192.168.1.2", message.To, "To was not set correctly.");
        Assert.AreEqual("FilePath=/docs/example.txt", message.MetaData, "MetaData was not set correctly.");
        Assert.AreEqual("This is the file content", message.Body, "Body was not set correctly.");
    }

    [TestMethod]
    public void CanHandleEmptyAndNullValues()
    {
        // Arrange
        var message = new Message {
            Subject = null,
            From = null,
            To = null,
            MetaData = null,
            Body = null
        };

        // Assert
        Assert.IsNull(message.Subject, "Subject should accept null.");
        Assert.IsNull(message.From, "From should accept null.");
        Assert.IsNull(message.To, "To should accept null.");
        Assert.IsNull(message.MetaData, "MetaData should accept null.");
        Assert.IsNull(message.Body, "Body should accept null.");
    }

    [TestMethod]
    public void Equality_CheckByReference()
    {
        // Arrange
        var message1 = new Message { RequestID = 1, Subject = "Test" };
        var message2 = new Message { RequestID = 1, Subject = "Test" };
        Message message3 = message1;

        // Assert
        Assert.AreNotEqual(message1, message2, "Different objects with the same data should not be equal by reference.");
        Assert.AreEqual(message1, message3, "Objects with the same reference should be equal.");
    }

    [TestMethod]
    public void SerializeAndDeserialize_Message()
    {
        // Arrange
        var originalMessage = new Message {
            Subject = "Response",
            RequestID = 101,
            From = "Server",
            To = "Client",
            MetaData = "MetadataExample",
            Body = "Serialized content"
        };

        // Act
        string serialized = System.Text.Json.JsonSerializer.Serialize(originalMessage);
        Message? deserialized = System.Text.Json.JsonSerializer.Deserialize<Message>(serialized);

        // Assert
        Assert.AreEqual(originalMessage.Subject, deserialized.Subject, "Subject was not serialized/deserialized correctly.");
        Assert.AreEqual(originalMessage.RequestID, deserialized.RequestID, "RequestID was not serialized/deserialized correctly.");
        Assert.AreEqual(originalMessage.From, deserialized.From, "From was not serialized/deserialized correctly.");
        Assert.AreEqual(originalMessage.To, deserialized.To, "To was not serialized/deserialized correctly.");
        Assert.AreEqual(originalMessage.MetaData, deserialized.MetaData, "MetaData was not serialized/deserialized correctly.");
        Assert.AreEqual(originalMessage.Body, deserialized.Body, "Body was not serialized/deserialized correctly.");
    }
}

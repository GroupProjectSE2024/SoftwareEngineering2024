using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Networking.Communication;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Content;

namespace Content.Tests;

[TestClass]
public class ChatServerTests
{
    private Mock<ICommunicator> _mockCommunicator;
    private ChatServer _chatServer;

    [TestInitialize]
    public void Setup()
    {
        // Mock the communicator and initialize ChatServer
        _mockCommunicator = new Mock<ICommunicator>();
        CommunicationFactory.SetCommunicatorMock(_mockCommunicator.Object);

        _chatServer = new ChatServer {
            ClientId = "1" // Default ClientId for tests
        };
    }

    [TestMethod]
    public void OnDataReceived_IgnoreEmptyStringMessage()
    {
        // Tests that an empty string message results in no action taken
        string serializedData = "";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnDataReceived_IgnoreMessageWithInvalidCharacters()
    {
        // Tests that a message with invalid characters or encoding is ignored
        string serializedData = "invalid Æ data";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void GetServerSingleton_ShouldReturnSameInstance()
    {
        // Tests that the singleton instance of ChatServer is consistent across calls
        ChatServer instance1 = ChatServer.GetServerInstance;
        ChatServer instance2 = ChatServer.GetServerInstance;

        Assert.AreSame(instance1, instance2);
    }

    [TestMethod]
    public void OnDataReceived_IgnoreInvalidJsonFormat()
    {
        // Tests that invalid JSON format is ignored and no communication is sent
        string serializedData = "Invalid JSON";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnDataReceived_SkipsEmptyMessageContent()
    {
        // Tests that an empty content message does not trigger communication
        var messageWithEmptyContent = new {
            Type = "Message",
            User = "TestUser",
            Content = "",
            Timestamp = "2024-11-28T12:00:00"
        };
        string serializedData = JsonSerializer.Serialize(messageWithEmptyContent);

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(
            comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    [TestMethod]
    public void OnMessageReceived_EmptyData_ShouldNotSend()
    {
        // Tests that no action is taken when an empty message is received
        string serializedData = "";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    [TestMethod]
    public void GetServerInstance_ValidatesSingleton()
    {
        // Tests that the singleton server instance is always the same
        ChatServer instance1 = ChatServer.GetServerInstance;
        ChatServer instance2 = ChatServer.GetServerInstance;

        Assert.AreSame(instance1, instance2);
    }

    [TestMethod]
    public void OnMessageReceived_InvalidJsonMessage_ShouldBeIgnored()
    {
        // Tests that an invalid JSON message does not trigger communication
        string serializedData = "{ invalid json }";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnMessageReceived_EmptyContent_ShouldNotSendMessage()
    {
        // Tests that messages with empty content are ignored
        var messageWithEmptyContent = new {
            Type = "Message",
            User = "TestUser",
            Content = "",
            Timestamp = "2024-11-28T12:00:00"
        };
        string serializedData = JsonSerializer.Serialize(messageWithEmptyContent);

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(
            comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void DataReceived_ShouldNotSendForEmptyMessage()
    {
        // Tests that no communication occurs for empty message data
        string serializedData = "";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void DataReceived_ShouldNotSendForInvalidMessageFormat()
    {
        // Tests that an invalid message format results in no action
        string serializedData = "invalid format message";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void SingletonCheck_ShouldReturnSameInstance()
    {
        // Ensures that multiple calls to GetServerInstance return the same instance
        ChatServer instance1 = ChatServer.GetServerInstance;
        ChatServer instance2 = ChatServer.GetServerInstance;

        Assert.AreSame(instance1, instance2);
    }

    [TestMethod]
    public void OnInvalidDataReceived_ShouldNotSendMessage()
    {
        // Tests that invalid data such as invalid JSON is ignored and does not trigger communication
        string serializedData = "{ invalid: true }";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnDataReceived_EmptyContentMessages_ShouldNotSend()
    {
        // Tests that empty content messages do not result in a send action
        var messageWithEmptyContent = new {
            Type = "Message",
            User = "TestUser",
            Content = "",
            Timestamp = "2024-11-28T12:00:00"
        };
        string serializedData = JsonSerializer.Serialize(messageWithEmptyContent);

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(
            comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnMessageReceived_InvalidData_ShouldNotSend()
    {
        // Tests that invalid data formats are ignored
        string serializedData = "Some invalid message data";

        _chatServer.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    // Helper class for mocking CommunicationFactory
    public static class CommunicationFactory
    {
        private static ICommunicator? s_mockCommunicator;

        public static ICommunicator GetCommunicator(bool isMocked)
        {
            return s_mockCommunicator;
        }

        public static void SetCommunicatorMock(ICommunicator mockCommunicator)
        {
            s_mockCommunicator = mockCommunicator;
        }
    }

}

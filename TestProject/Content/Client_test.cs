//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Networking.Communication;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Text.Json;

//namespace Content.Tests;

//[TestClass]
//public class ChatClientTests
//{
//    private Mock<ICommunicator> _mockCommunicator;
//    private ChatClient _chatClient;

//    [TestInitialize]
//    public void Setup()
//    {
//        // Mock the communicator
//        _mockCommunicator = new Mock<ICommunicator>();

//        // Initialize ChatClient
//        _chatClient = new ChatClient {
//            Username = "TestUser",
//            ClientId = "1",
//            UserProfileUrl = "http://example.com/profile.jpg"
//        };
//    }

//    /// <summary>
//    /// Verifies that the OnDataReceived method triggers the MessageReceived event for a public message.
//    /// </summary>
//    [TestMethod]
//    public void OnDataReceived_ShouldInvokeMessageReceivedEvent()
//    {
//        string serializedData = "publicÆHello WorldÆUser1Æ123";
//        string receivedMessage = null;

//        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
//        _chatClient.OnDataReceived(serializedData);
//        Assert.AreEqual(serializedData, receivedMessage);
//    }

//    /// <summary>
//    /// Verifies that the OnDataReceived method ignores invalid message formats without errors.
//    /// </summary>
//    [TestMethod]
//    public void OnDataReceived_ShouldIgnoreInvalidMessageFormat()
//    {
//        string serializedData = "invalidÆmessage";
//        _chatClient.OnDataReceived(serializedData);
//        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//    }

//    /// <summary>
//    /// Ensures that the ChatClient constructor initializes the ObservableCollection for client list.
//    /// </summary>
//    [TestMethod]
//    public void Constructor_ShouldInitializeClientListObs()
//    {
//        Assert.IsNotNull(_chatClient._clientListobs);
//        Assert.IsInstanceOfType(_chatClient._clientListobs, typeof(ObservableCollection<string>));
//    }


//    /// <summary>
//    /// Verifies that LatestAction is not triggered when OnDataReceived receives non-client list data.
//    /// </summary>
//    [TestMethod]
//    public void OnDataReceived_ShouldNotTriggerLatestActionForNonClientListData()
//    {
//        string serializedData = "publicÆTest MessageÆUser1Æ123";

//        bool latestActionTriggered = false;
//        _chatClient.LatestAction += (dict) => latestActionTriggered = true;

//        _chatClient.OnDataReceived(serializedData);

//        Assert.IsFalse(latestActionTriggered);
//    }


//}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Networking.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Content.Tests;

[TestClass]
public class ChatClientTests
{
    private Mock<ICommunicator> _mockCommunicator;
    private ChatClient _chatClient;

    [TestInitialize]
    public void Setup()
    {
        // Mock the communicator
        _mockCommunicator = new Mock<ICommunicator>();

        // Initialize ChatClient
        _chatClient = new ChatClient {
            Username = "TestUser",
            ClientId = "1",
            UserProfileUrl = "http://example.com/profile.jpg"
        };
    }

    /// <summary>
    /// Verifies that the OnDataReceived method triggers the MessageReceived event for a public message.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldInvokeMessageReceivedEvent()
    {
        string serializedData = "publicÆHello WorldÆUser1Æ123";
        string receivedMessage = null;

        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
        _chatClient.OnDataReceived(serializedData);
        Assert.AreEqual(serializedData, receivedMessage);
    }

    /// <summary>
    /// Verifies that the OnDataReceived method ignores invalid message formats without errors.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldIgnoreInvalidMessageFormat()
    {
        string serializedData = "invalidÆmessage";
        _chatClient.OnDataReceived(serializedData);
        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Ensures that the ChatClient constructor initializes the ObservableCollection for client list.
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldInitializeClientListObs()
    {
        Assert.IsNotNull(_chatClient._clientListobs);
        Assert.IsInstanceOfType(_chatClient._clientListobs, typeof(ObservableCollection<string>));
    }

    /// <summary>
    /// Verifies that LatestAction is not triggered when OnDataReceived receives non-client list data.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldNotTriggerLatestActionForNonClientListData()
    {
        string serializedData = "publicÆTest MessageÆUser1Æ123";

        bool latestActionTriggered = false;
        _chatClient.LatestAction += (dict) => latestActionTriggered = true;

        _chatClient.OnDataReceived(serializedData);

        Assert.IsFalse(latestActionTriggered);
    }


    /// <summary>
    /// Verifies that OnDataReceived correctly handles a private message.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldInvokeMessageReceivedForPrivateMessage()
    {
        string serializedData = "privateÆHello User2ÆUser1Æ123";
        string receivedMessage = null;

        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
        _chatClient.OnDataReceived(serializedData);

        Assert.AreEqual(serializedData, receivedMessage);
    }


    /// <summary>
    /// Verifies that OnDataReceived ignores invalid public messages.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldIgnoreInvalidPublicMessage()
    {
        string serializedData = "publicÆInvalid Public MessageÆUser1";

        _chatClient.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    /// <summary>
    /// Verifies that invalid private messages are ignored.
    /// </summary>
    [TestMethod]
    public void OnDataReceived_ShouldIgnoreInvalidPrivateMessages()
    {
        string serializedData = "privateÆInvalid Private Message";

        _chatClient.OnDataReceived(serializedData);

        _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void OnDataReceived_ShouldInvokeMessageReceivedEventForBroadcastMessage()
    {
        // Verifies that the OnDataReceived method triggers the MessageReceived event for a broadcast message.
        string serializedData = "broadcastÆTest broadcast messageÆUser1Æ123";
        string receivedMessage = null;

        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
        _chatClient.OnDataReceived(serializedData);

        Assert.AreEqual(serializedData, receivedMessage);
    }

    [TestMethod]
    public void OnDataReceived_ShouldNotTriggerClientListUpdateForInvalidClientListMessage()
    {
        // Verifies that an invalid client list message does not trigger the ClientListUpdated event.
        string serializedData = "clientListÆUser1ÆInvalidMessage";

        List<string> updatedClientList = null;
        _chatClient.ClientListUpdated += (sender, list) => updatedClientList = list;

        _chatClient.OnDataReceived(serializedData);

        Assert.IsNull(updatedClientList);
    }


    [TestMethod]
    public void OnDataReceived_ShouldTriggerMessageReceivedForValidMessageWithMultipleWords()
    {
        // Verifies that a valid public message with multiple words is correctly received.
        string serializedData = "publicÆThis is a valid message with multiple wordsÆUser1Æ123";
        string receivedMessage = null;

        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
        _chatClient.OnDataReceived(serializedData);

        Assert.AreEqual(serializedData, receivedMessage);
    }

    [TestMethod]
    public void OnDataReceived_ShouldInvokeMessageReceivedEventForSystemMessage()
    {
        // Verifies that a system message triggers the MessageReceived event.
        string serializedData = "systemÆServer maintenance at 12:00 AMÆSystemÆ000";
        string receivedMessage = null;

        _chatClient.MessageReceived += (sender, message) => receivedMessage = message;
        _chatClient.OnDataReceived(serializedData);

        Assert.AreEqual(serializedData, receivedMessage);
    }

}

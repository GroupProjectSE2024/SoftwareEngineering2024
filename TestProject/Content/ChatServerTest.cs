//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Networking.Communication;
//using System;
//using System.Collections.Generic;
//using System.Text.Json;
//using Content;

//namespace Content.Tests
//{
//    [TestClass]
//    public class ChatServerTests
//    {
//        private Mock<ICommunicator> _mockCommunicator;
//        private ChatServer _chatServer;

//        [TestInitialize]
//        public void Setup()
//        {
//            // Mock the communicator
//            _mockCommunicator = new Mock<ICommunicator>();
//            CommunicationFactory.SetCommunicatorMock(_mockCommunicator.Object);

//            // Initialize ChatServer
//            _chatServer = new ChatServer();
//            _chatServer.ClientId = "1"; // Default ClientId for tests
//        }







//        [TestMethod]
//        public void OnDataReceived_ShouldIgnoreEmptyMessage()
//        {
//            // Arrange
//            string serializedData = "";

//            // Act
//            _chatServer.OnDataReceived(serializedData);

//            // Assert
//            _mockCommunicator.Verify(comm => comm.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//        }

//        [TestMethod]
//        public void OnDataReceived_ShouldNotUpdateClientUsernames_ForInvalidMessage()
//        {
//            // Arrange
//            string serializedData = "invalid|data";

//            // Act
//            _chatServer.OnDataReceived(serializedData);

//            // Assert
//            Assert.AreEqual(0, _chatServer.ClientUsernames.Count);
//        }



//        [TestMethod]
//        public void OnDataReceived_ShouldHandleDuplicateClientId()
//        {
//            // Arrange
//            string serializedData1 = "connect|connect|User1|1";
//            string serializedData2 = "connect|connect|User2|1";

//            // Act
//            _chatServer.OnDataReceived(serializedData1);
//            _chatServer.OnDataReceived(serializedData2);

//            // Assert
//            Assert.AreEqual(1, _chatServer.ClientUsernames.Count);
//            Assert.AreEqual("User2", _chatServer.ClientUsernames[1]); // Latest username should overwrite
//        }




//        // Helper class to mock CommunicationFactory
//        public static class CommunicationFactory
//        {
//            private static ICommunicator _mockCommunicator;

//            public static ICommunicator GetCommunicator(bool isMocked)
//            {
//                return _mockCommunicator;
//            }

//            public static void SetCommunicatorMock(ICommunicator mockCommunicator)
//            {
//                _mockCommunicator = mockCommunicator;
//            }
//        }
//    }
//}

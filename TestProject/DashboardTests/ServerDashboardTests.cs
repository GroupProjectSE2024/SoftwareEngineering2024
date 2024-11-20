using Dashboard;
using Xunit;
using Networking;
using System.Collections.ObjectModel;
using Assert = Xunit.Assert;
using Networking.Communication;

namespace TestProject.DashboardTests { 
    public class ServerDashboardTests
    {
        private readonly ICommunicator _mockCommunicator = CommunicationFactory.GetCommunicator(false);
        [Fact]
        public void Initialize_ShouldReturnExpectedString()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator,"USERNAME","USEREMAIL","PROFILEURL");
            string result = serverDashboard.Initialize();
            Assert.NotNull(result);
        }

        [Fact]
        public void BroadcastMessage_ShouldNotThrowException()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator, "USERNAME", "USEREMAIL", "PROFILEURL");
            serverDashboard.BroadcastMessage("Test Message");
        }

        [Fact]
        public void SendMessage_ShouldNotThrowException()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator, "USERNAME", "USEREMAIL", "PROFILEURL");
            serverDashboard.SendMessage("127.0.0.1", "Test Message");
        }

        [Fact]
        public void ServerStop_ShouldReturnTrue()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator, "USERNAME", "USEREMAIL", "PROFILEURL");
            bool result = serverDashboard.ServerStop();
            Assert.True(result);
        }

        [Fact]
        public void OnClientJoined_ShouldNotThrowException()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator, "USERNAME", "USEREMAIL", "PROFILEURL");
            serverDashboard.OnClientJoined(null, "127.0.0.1", "8080");
        }

        [Fact]
        public void OnClientLeft_ShouldNotThrowException()
        {
            var serverDashboard = new ServerDashboard(_mockCommunicator, "USERNAME", "USEREMAIL", "PROFILEURL");
            serverDashboard.OnClientLeft("TestClientId");
        }
    }
}

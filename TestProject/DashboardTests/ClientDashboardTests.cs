using Dashboard;
using Xunit;
using System.Collections.ObjectModel;
using Networking;
using Assert = Xunit.Assert;
using Networking.Communication;

namespace TestProject.DashboardTests
{
    public class ClientDashboardTests
    {
        private readonly ICommunicator _mockCommunicator = CommunicationFactory.GetCommunicator();


        [Fact]
        public void Initialize_ShouldReturnExpectedString()
        {
            var clientDashboard = new ClientDashboard(_mockCommunicator, "clientname", "clientEmail", "clientID");
            string result = clientDashboard.Initialize("127.0.0.1", "8080");
            Assert.NotNull(result);
        }

        [Fact]
        public void SendMessage_ShouldNotThrowException()
        {
            var clientDashboard = new ClientDashboard(_mockCommunicator, "clientname", "clientEmail", "clientID");
            clientDashboard.SendMessage("127.0.0.1", "Test Message");
        }

        [Fact]
        public void SendInfo_ShouldNotThrowException()
        {
            var clientDashboard = new ClientDashboard(_mockCommunicator, "clientname", "clientEmail", "clientID");
            clientDashboard.SendInfo("Testclient", "test@example.com");
        }

        [Fact]
        public void ClientLeft_ShouldReturnTrue()
        {
            var clientDashboard = new ClientDashboard(_mockCommunicator, "clientname", "clientEmail", "clientID");
            bool result = clientDashboard.ClientLeft();
            Assert.True(result);
        }

        [Fact]
        public void OnDataReceived_ShouldNotThrowException()
        {
            var clientDashboard = new ClientDashboard(_mockCommunicator, "clientname", "clientEmail", "clientID");
            clientDashboard.OnDataReceived("Test Data");
        }
    }
}


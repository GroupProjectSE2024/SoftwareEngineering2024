using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using ViewModel.DashboardViewModel;
using Moq;
using Networking.Communication;
using Dashboard;
using ViewModel.DashboardViewModel;

namespace TestProject.DashboardTests
{
    [TestClass]
    public class MainPageViewModelTests
    {
        private MainPageViewModel _viewModel;
        private Mock<ICommunicator> _mockCommunicator;
        private Mock<ServerDashboard> _mockServerDashboard;
        private Mock<ClientDashboard> _mockClientDashboard;

        [TestInitialize]
        public void Setup()
        {
            _mockCommunicator = new Mock<ICommunicator>();
            _mockServerDashboard = new Mock<ServerDashboard>(_mockCommunicator.Object, "TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");
            _mockClientDashboard = new Mock<ClientDashboard>(_mockCommunicator.Object, "TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            _viewModel = new MainPageViewModel();
        }

        [TestMethod]
        public void CreateSession_Success()
        {
            // Arrange
            _mockServerDashboard.Setup(m => m.Initialize()).Returns("127.0.0.1:8080");
            _viewModel.CreateSession("TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            // Act
            var result = _viewModel.CreateSession("TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            // Assert
            Assert.AreEqual("success", result);
            Assert.IsTrue(_viewModel.IsHost);
            Assert.AreEqual("TestUser", _viewModel.UserName);
            Assert.AreEqual("https://developers.google.com/identity/images/g-logo.png", _viewModel.ProfilePicUrl);
            Assert.AreEqual("127.0.0.1", _viewModel.ServerIP);
            Assert.AreEqual("8080", _viewModel.ServerPort);
        }

        [TestMethod]
        public void CreateSession_Failure()
        {
            // Arrange
            _mockServerDashboard.Setup(m => m.Initialize()).Returns("failure");

            // Act
            var result = _viewModel.CreateSession("TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            // Assert
            Assert.AreEqual("failure", result);
        }

        [TestMethod]
        public void JoinSession_Success()
        {
            // Arrange
            _mockClientDashboard.Setup(m => m.Initialize("127.0.0.1", "8080")).Returns("success");

            // Act
            var result = _viewModel.JoinSession("TestUser", "test@example.com", "127.0.0.1", "8080", "https://developers.google.com/identity/images/g-logo.png");

            // Assert
            Assert.AreEqual("success", result);
            Assert.IsFalse(_viewModel.IsHost);
            Assert.AreEqual("TestUser", _viewModel.UserName);
            Assert.AreEqual("https://developers.google.com/identity/images/g-logo.png", _viewModel.ProfilePicUrl);
        }

        [TestMethod]
        public void JoinSession_Failure()
        {
            // Arrange
            _mockClientDashboard.Setup(m => m.Initialize("127.0.0.1", "8080")).Returns("failure");

            // Act
            var result = _viewModel.JoinSession("TestUser", "test@example.com", "127.0.0.1", "8080", "https://developers.google.com/identity/images/g-logo.png");

            // Assert
            Assert.AreEqual("failure", result);
        }

        [TestMethod]
        public void ServerStopSession_Success()
        {
            // Arrange
            _mockServerDashboard.Setup(m => m.ServerStop()).Returns(true);
            _viewModel.CreateSession("TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            // Act
            var result = _viewModel.ServerStopSession();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ClientLeaveSession_Success()
        {
            // Arrange
            _mockClientDashboard.Setup(m => m.ClientLeft()).Returns(true);
            _viewModel.JoinSession("TestUser", "test@example.com", "127.0.0.1", "8080", "https://developers.google.com/identity/images/g-logo.png");

            // Act
            var result = _viewModel.ClientLeaveSession();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PropertyChanged_EventIsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainPageViewModel.UserName))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            _viewModel.UserName = "NewUserName";

            // Assert
            Assert.IsTrue(propertyChangedRaised);
        }

        [TestMethod]
        public void UpdateUserListOnPropertyChanged_UpdatesUserDetailsList()
        {
            // Arrange
            var userDetails = new ObservableCollection<UserDetails>
            {
                new UserDetails { UserName = "User1" },
                new UserDetails { UserName = "User2" }
            };
            _mockServerDashboard.SetupGet(m => m.ServerUserList).Returns(userDetails);
            _viewModel.CreateSession("TestUser", "test@example.com", "https://developers.google.com/identity/images/g-logo.png");

            // Act
            _viewModel.UpdateUserListOnPropertyChanged(_mockServerDashboard.Object, new PropertyChangedEventArgs(nameof(ServerDashboard.ServerUserList)));

            // Assert
            Assert.AreEqual(2, _viewModel.UserDetailsList.Count);
            Assert.AreEqual("User1", _viewModel.UserDetailsList[0].UserName);
            Assert.AreEqual("User2", _viewModel.UserDetailsList[1].UserName);
        }

        [TestMethod]
        public void TimerOnTick_UpdatesUserCountGraph()
        {
            // Arrange
            _viewModel.UserDetailsList.Add(new UserDetails { UserName = "User1" });

            // Act
            _viewModel.TimerOnTick(null, null);

            // Assert
            Assert.AreEqual(1, _viewModel.CurrentUserCount);
            Assert.AreEqual(1, _viewModel.SeriesCollection[0].Values.Count);
        }

    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Globalization;
using System.Windows;
using UXModule;
using UXModule.Views;
using ViewModel.DashboardViewModel;
using Dashboard;

namespace TestProject.DashboardTests
{
    [TestClass]
    public class ApplicationPageValueConverterTests
    {
        private ApplicationPageValueConverter _converter;
        private Mock<WindowViewModel> _windowViewModelMock;
        private Mock<MainPageViewModel> _mainPageViewModelMock;

        [TestInitialize]
        public void Setup()
        {
            _converter = new ApplicationPageValueConverter();
            _windowViewModelMock = new Mock<WindowViewModel>();
            _mainPageViewModelMock = new Mock<MainPageViewModel>();

            // Mock the Application.Current.MainWindow.DataContext to return the WindowViewModel
            var mainWindowMock = new Mock<Window>();
            mainWindowMock.SetupGet(m => m.DataContext).Returns(_windowViewModelMock.Object);
            Application.Current.MainWindow = mainWindowMock.Object;

            _windowViewModelMock.SetupGet(vm => vm.MainPageViewModel).Returns(_mainPageViewModelMock.Object);
        }

        [TestMethod]
        public void Convert_Homepage_ReturnsHomePage()
        {
            // Arrange
            var value = ApplicationPage.Homepage;

            // Act
            var result = _converter.Convert(value, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(HomePage));
        }

        [TestMethod]
        public void Convert_Login_ReturnsLoginPage()
        {
            // Arrange
            var value = ApplicationPage.Login;

            // Act
            var result = _converter.Convert(value, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(LoginPage));
        }

        [TestMethod]
        public void Convert_ServerHomePage_ReturnsServerHomePage()
        {
            // Arrange
            var value = ApplicationPage.ServerHomePage;

            // Act
            var result = _converter.Convert(value, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ServerHomePage));
        }

        [TestMethod]
        public void Convert_ClientHomePage_ReturnsClientHomePage()
        {
            // Arrange
            var value = ApplicationPage.ClientHomePage;

            // Act
            var result = _converter.Convert(value, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ClientHomePage));
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act
            _converter.ConvertBack(null, null, null, CultureInfo.InvariantCulture);
        }
    }
}
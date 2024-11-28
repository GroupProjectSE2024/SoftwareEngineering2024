using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Input;
using WhiteboardGUI.ViewModel;

namespace Whiteboard;

[TestClass]
public class RelayCommandTests
{
    [TestMethod]
    public void RelayCommand_ShouldThrowArgumentNullException_WhenExecuteIsNull()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            var command = new RelayCommand(null);
        });
    }

    [TestMethod]
    public void RelayCommand_ShouldCreateInstance_WhenValidParametersArePassed()
    {
        // Arrange
        Action execute = () => { };

        // Act
        var command = new RelayCommand(execute);

        // Assert
        Assert.IsNotNull(command);
    }

    [TestMethod]
    public void RelayCommand_CanExecute_ShouldReturnTrue_WhenCanExecuteIsNull()
    {
        // Arrange
        Action execute = () => { };
        var command = new RelayCommand(execute);

        // Act
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    [TestMethod]
    public void RelayCommand_CanExecute_ShouldReturnTrue_WhenCanExecuteReturnsTrue()
    {
        // Arrange
        Action execute = () => { };
        Func<bool> canExecuteFunc = () => true;
        var command = new RelayCommand(execute, canExecuteFunc);

        // Act
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    [TestMethod]
    public void RelayCommand_CanExecute_ShouldReturnFalse_WhenCanExecuteReturnsFalse()
    {
        // Arrange
        Action execute = () => { };
        Func<bool> canExecuteFunc = () => false;
        var command = new RelayCommand(execute, canExecuteFunc);

        // Act
        bool canExecute = command.CanExecute(null);

        // Assert
        Assert.IsFalse(canExecute);
    }

    [TestMethod]
    public void RelayCommand_Execute_ShouldInvokeExecuteAction()
    {
        // Arrange
        bool isExecuted = false;
        Action execute = () => { isExecuted = true; };
        var command = new RelayCommand(execute);

        // Act
        command.Execute(null);

        // Assert
        Assert.IsTrue(isExecuted);
    }

    [TestMethod]
    public void RelayCommand_ShouldAddAndRemoveCanExecuteChangedEventHandlers()
    {
        // Arrange
        Action execute = () => { };
        var command = new RelayCommand(execute);
        EventHandler handler = (s, e) => { };

        // Act
        command.CanExecuteChanged += handler;
        command.CanExecuteChanged -= handler;

        // Assert
        // No exception should be thrown
    }
}

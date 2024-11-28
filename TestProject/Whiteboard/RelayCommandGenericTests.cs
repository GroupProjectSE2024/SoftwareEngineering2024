using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Input;
using WhiteboardGUI.ViewModel;

namespace Whiteboard;

[TestClass]
public class RelayCommandGenericTests
{
    // Existing tests...

    [TestMethod]
    public void RelayCommandT_Execute_ShouldHandleNullParameter_WhenTIsReferenceType()
    {
        // Arrange
        string receivedParam = "initial";
        Action<string> execute = (param) => { receivedParam = param; };
        var command = new RelayCommand<string>(execute);

        // Act
        command.Execute(null);

        // Assert
        Assert.IsNull(receivedParam);
    }

    [TestMethod]
    public void RelayCommandT_CanExecute_ShouldHandleNullParameter_WhenTIsReferenceType()
    {
        // Arrange
        bool canExecuteCalled = false;
        Predicate<string> canExecute = (param) =>
        {
            canExecuteCalled = true;
            return param == null;
        };
        var command = new RelayCommand<string>((s) => { }, canExecute);

        // Act
        bool result = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecuteCalled);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RelayCommandT_Execute_ShouldThrowNullReferenceException_WhenParameterIsNullAndTIsValueType()
    {
        // Arrange
        Action<int> execute = (param) => { };
        var command = new RelayCommand<int>(execute);

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => command.Execute(null));
    }

    [TestMethod]
    public void RelayCommandT_CanExecute_ShouldThrowNullReferenceException_WhenParameterIsNullAndTIsValueType()
    {
        // Arrange
        Predicate<int> canExecute = (param) => true;
        var command = new RelayCommand<int>((i) => { }, canExecute);

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => command.CanExecute(null));
    }


    [TestMethod]
    public void RelayCommandT_Execute_ShouldHandleNullParameter_WhenTIsNullableValueType()
    {
        // Arrange
        int? receivedParam = 42;
        Action<int?> execute = (param) => { receivedParam = param; };
        var command = new RelayCommand<int?>(execute);

        // Act
        command.Execute(null);

        // Assert
        Assert.IsNull(receivedParam);
    }

    [TestMethod]
    public void RelayCommandT_CanExecute_ShouldHandleNullParameter_WhenTIsNullableValueType()
    {
        // Arrange
        bool canExecuteCalled = false;
        Predicate<int?> canExecute = (param) =>
        {
            canExecuteCalled = true;
            return !param.HasValue;
        };
        var command = new RelayCommand<int?>((i) => { }, canExecute);

        // Act
        bool result = command.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecuteCalled);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RelayCommandT_ShouldRaiseCanExecuteChanged_WhenCommandManagerRequerySuggestedIsRaised()
    {
        // Arrange
        Action<object> execute = (param) => { };
        var command = new RelayCommand<object>(execute);

        bool eventRaised = false;
        command.CanExecuteChanged += (s, e) => eventRaised = true;

        // Act
        CommandManager.InvalidateRequerySuggested();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => { }));

        // Assert
        Assert.IsTrue(eventRaised);
    }
}

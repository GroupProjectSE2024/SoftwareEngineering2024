/******************************************************************************
 * Filename    = RelayCommand.cs
 *
 * Author      = Yash Mittal | Vishnu Nair
 *
 * Project     = WhiteBoard
 *
 * Description = Implements ICommand to handle actions and their execution logic in MVVM.
 *****************************************************************************/

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WhiteboardGUI.ViewModel;

/// <summary>
/// A command that relays its functionality by invoking delegates.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class with the execute action.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    public RelayCommand(Action execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class with execute and canExecute delegates.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The function that determines whether the command can execute.</param>
    /// <exception cref="ArgumentNullException">Thrown if the execute action is null.</exception>
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. Ignored in this implementation.</param>
    /// <returns>true if the command can execute; otherwise, false.</returns>
    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute();
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Executes the command's action.
    /// </summary>
    /// <param name="parameter">Data used by the command. Ignored in this implementation.</param>
    public void Execute(object parameter)
    {
        _execute();
    }
}

/// <summary>
/// A generic command that relays its functionality by invoking delegates with a parameter.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class with the execute action.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    public RelayCommand(Action<T> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class with execute and canExecute delegates.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <exception cref="ArgumentNullException">Thrown if the execute action is null.</exception>
    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>true if the command can execute; otherwise, false.</returns>
    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute((T)parameter);
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Executes the command's action with the provided parameter.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}

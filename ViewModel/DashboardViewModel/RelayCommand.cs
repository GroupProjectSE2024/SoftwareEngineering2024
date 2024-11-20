using System;
using System.Windows.Input;

namespace ViewModel.DashboardViewModel
{
    /// <summary>
    /// A command that relays its execution to a specified action
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// The action to be executed
        /// </summary>
        private readonly Action<object> _action;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="action">The action to be executed by the command</param>
        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        /// <summary>
        /// Event that is raised when the execution status changes
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">The parameter to be passed to the command</param>
        /// <returns>True if the command can execute, otherwise false</returns>
        public bool CanExecute(object parameter) => true;

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">The parameter to be passed to the action</param>
        public void Execute(object parameter) => _action(parameter);
    }
}
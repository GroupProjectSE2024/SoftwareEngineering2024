using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dashboard;

namespace ViewModel.DashboardViewModel
{
    /// <summary>
    /// ViewModel for custom window
    /// </summary>
    public class WindowViewModel : INotifyPropertyChanged
    {
        #region Private Members

        /// <summary>
        /// The window instance
        /// </summary>
        private Window _window;

        /// <summary>
        /// Margin around the window that allows to Drop Shadow
        /// </summary>
        private int _outerMarginSize = 10;

        /// <summary>
        /// The size of the window corner
        /// </summary>
        private int _windowCornerSize = 10;

        #endregion

        #region Public Members

        /// <summary>
        /// ViewModel for the main page
        /// </summary>
        public MainPageViewModel MainPageViewModel { get; private set; }

        /// <summary>
        /// Minimum width of the window
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 400;

        /// <summary>
        /// Maximum height of the window
        /// </summary>
        public double WindowMaximumHeight { get; set; } = 400;

        /// <summary>
        /// True if the window should be borderless because it is docked or maximized
        /// </summary>
        public bool Borderless => _window.WindowState == WindowState.Maximized;

        /// <summary>
        /// The size of the resize border around the window
        /// </summary>
        public int ResizeBorderSize => Borderless ? 0 : 6;

        /// <summary>
        /// The thickness of the resize border
        /// </summary>
        public Thickness ResizeBorderThickness => new Thickness(ResizeBorderSize + OuterMarginSize);

        /// <summary>
        /// The inner content padding
        /// </summary>
        public Thickness InnerContentPadding => new Thickness(ResizeBorderSize);

        /// <summary>
        /// Margin size around the window
        /// </summary>
        public int OuterMarginSize
        {
            get => _window.WindowState == WindowState.Maximized ? 0 : _outerMarginSize;
            set => _outerMarginSize = value;
        }

        /// <summary>
        /// The thickness of the outer margin
        /// </summary>
        public Thickness OuterMarginThickness => new Thickness(OuterMarginSize);

        /// <summary>
        /// The radius of the window corners
        /// </summary>
        public int CornerRadius
        {
            get => _window.WindowState == WindowState.Maximized ? 0 : _windowCornerSize;
            set => _windowCornerSize = value;
        }

        /// <summary>
        /// The corner radius of the window
        /// </summary>
        public CornerRadius WindowCornerRadius => new CornerRadius(CornerRadius);

        /// <summary>
        /// The height of the window caption
        /// </summary>
        public int CaptionHeightSize { get; set; } = 42;

        /// <summary>
        /// The grid length of the window caption height
        /// </summary>
        public GridLength CaptionHeightGridLength => new GridLength(CaptionHeightSize);

        /// <summary>
        /// The current page of the application
        /// </summary>
        public ApplicationPage CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// The command to minimize the window
        /// </summary>
        public ICommand MinimizeCommand { get; set; }

        /// <summary>
        /// The command to maximize the window
        /// </summary>
        public ICommand MaximizeCommand { get; set; }

        /// <summary>
        /// The command to close the window
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// The command to show the menu of the window
        /// </summary>
        public ICommand MenuCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The current page in the application
        /// </summary>
        private ApplicationPage _currentPage = ApplicationPage.Login;

        /// <summary>
        /// Constructor for WindowViewModel
        /// </summary>
        /// <param name="window">The window instance</param>
        /// <param name="viewModel">The main page ViewModel</param>
        public WindowViewModel(Window window, MainPageViewModel viewModel)
        {
            _window = window;
            MainPageViewModel = viewModel;

            _window.StateChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(CaptionHeightGridLength));
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(CornerRadius));
                OnPropertyChanged(nameof(WindowCornerRadius));
            };

            MinimizeCommand = new RelayCommand(_ => _window.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(_ => _window.WindowState ^= WindowState.Maximized);
            CloseCommand = new RelayCommand(_ => _window.Close());
            MenuCommand = new RelayCommand(_ => SystemCommands.ShowSystemMenu(_window, GetMousePosition()));
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Gets the current mouse position on the screen
        /// </summary>
        /// <returns>The mouse position as a Point</returns>
        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(_window);

            // Add the window position so it's a "ToScreen"
            return new Point(position.X + _window.Left, position.Y + _window.Top);
        }

        /// <summary>
        /// Event handler for property change notifications
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
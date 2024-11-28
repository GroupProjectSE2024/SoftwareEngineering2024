/******************************************************************************
 * Filename    = MainPage.xaml.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = Code-behind for MainPage handling user interactions and shape manipulations
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WhiteboardGUI.ViewModel;
using WhiteboardGUI.Models;


namespace UXModule.Views;

/// <summary>
/// Interaction logic for MainPage.xaml
/// </summary>
public partial class WhiteboardPage : Page
{
    private MainPageViewModel? ViewModel => DataContext as MainPageViewModel;
    private IShape? _resizingShape;
    private string? _currentHandle;
    private Point _startPoint;
    private Rect _initialBounds;
    private List<Point>? _initialPoints;

    public WhiteboardPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the left mouse button down event on the canvas.
    /// Initiates dragging operation.
    /// </summary>
    private void Canvas_LeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel?.CanvasLeftMouseDownCommand.Execute(e);
        ViewModel.IsDragging = true;
    }

    /// <summary>
    /// Handles the mouse move event on the canvas.
    /// Executes the corresponding command in the ViewModel.
    /// </summary>
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        ViewModel?.CanvasMouseMoveCommand.Execute(e);
    }

    /// <summary>
    /// Handles the left mouse button up event on the canvas.
    /// Ends dragging operation.
    /// </summary>
    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ViewModel?.CanvasMouseUpCommand.Execute(e);
        ViewModel.IsDragging = false;
    }

    /// <summary>
    /// Opens the color palette popup when the toggle button is checked.
    /// </summary>
    private void PaletteToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        ColorPopup.IsOpen = true;
    }

    /// <summary>
    /// Closes the color palette popup when the toggle button is unchecked.
    /// </summary>
    private void PaletteToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        ColorPopup.IsOpen = false;
    }

    /// <summary>
    /// Opens the upload popup when the upload button is clicked.
    /// </summary>
    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        UploadPopup.IsOpen = true;
    }

    /// <summary>
    /// Handles the submission of the filename in the upload popup.
    /// Closes the popup and notifies the user.
    /// </summary>
    private void SubmitFileName_Click(object sender, RoutedEventArgs e)
    {
        UploadPopup.IsOpen = false;
        MessageBox.Show($"Filename '{ViewModel.SnapShotFileName}' has been set.", "Filename Set", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Initiates the resizing of a shape when a resize handle is pressed.
    /// Captures the mouse and stores initial state for resizing.
    /// </summary>
    private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement rect)
        {
            _currentHandle = rect.Tag as string;
            _resizingShape = rect.DataContext as IShape;
            if (_resizingShape != null)
            {
                _startPoint = e.GetPosition(this);
                Mouse.Capture(rect);

                // Store initial bounds and points for scaling
                _initialBounds = _resizingShape.GetBounds();
                if (_resizingShape is ScribbleShape scribble)
                {
                    _initialPoints = new List<Point>(scribble.Points);
                }

                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Handles the mouse move event during a resize operation.
    /// Updates the shape's size based on mouse movement.
    /// </summary>
    private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
    {
        if (_resizingShape != null && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPoint = e.GetPosition(this);
            Vector totalDelta = currentPoint - _startPoint;
            ResizeShape(_resizingShape, _currentHandle, currentPoint);

            // Conditionally update _startPoint only for CircleShape and LineShape
            if (!(_resizingShape is ScribbleShape))
            {
                _startPoint = currentPoint;
            }

            e.Handled = true;
        }
    }

    /// <summary>
    /// Finalizes the resize operation when the left mouse button is released.
    /// Releases the mouse capture and updates the rendering service.
    /// </summary>
    private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_resizingShape != null)
        {
            Mouse.Capture(null);
            if (this.DataContext is MainPageViewModel viewModel)
            {
                viewModel.RenderingService.RenderShape(_resizingShape, "MODIFY");
            }
            _resizingShape = null;
            _currentHandle = null;
            _initialBounds = Rect.Empty;
            _initialPoints = null;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Resizes the specified shape based on the handle being dragged and the current mouse position.
    /// Delegates to specific resize methods based on shape type.
    /// </summary>
    private void ResizeShape(IShape shape, string handle, Point currentPoint)
    {
        if (shape is LineShape line)
        {
            ResizeLineShape(line, handle, currentPoint);
        }
        else if (shape is CircleShape circle)
        {
            ResizeCircleShape(circle, handle, currentPoint - _startPoint);
            _startPoint = currentPoint; // Update for CircleShape
        }
        else if (shape is ScribbleShape scribble)
        {
            ResizeScribbleShape(scribble, handle, currentPoint - _startPoint);
            // Do not update _startPoint for ScribbleShape
        }
        // Add more shapes if needed
    }

    /// <summary>
    /// Resizes a line shape by updating its start or end coordinates.
    /// </summary>
    private void ResizeLineShape(LineShape line, string handle, Point currentPoint)
    {
        if (handle == "Start")
        {
            line.StartX = currentPoint.X;
            line.StartY = currentPoint.Y;
        }
        else if (handle == "End")
        {
            line.EndX = currentPoint.X;
            line.EndY = currentPoint.Y;
        }
    }

    // Existing methods for other shapes remain unchanged

    /// <summary>
    /// Resizes a scribble shape by scaling its points based on the handle and mouse movement.
    /// Ensures the shape does not collapse below minimum size.
    /// </summary>
    private void ResizeScribbleShape(ScribbleShape scribble, string handle, Vector totalDelta)
    {
        if (_initialBounds == Rect.Empty || _initialPoints == null)
        {
            return;
        }

        double minWidth = 8;
        double minHeight = 8;

        double newLeft = _initialBounds.Left;
        double newTop = _initialBounds.Top;
        double newWidth = _initialBounds.Width;
        double newHeight = _initialBounds.Height;

        // Adjust new bounds based on handle and totalDelta
        switch (handle)
        {
            case "TopLeft":
                newLeft += totalDelta.X;
                newTop += totalDelta.Y;
                newWidth -= totalDelta.X;
                newHeight -= totalDelta.Y;
                break;
            case "TopRight":
                newTop += totalDelta.Y;
                newWidth += totalDelta.X;
                newHeight -= totalDelta.Y;
                break;
            case "BottomLeft":
                newLeft += totalDelta.X;
                newWidth -= totalDelta.X;
                newHeight += totalDelta.Y;
                break;
            case "BottomRight":
                newWidth += totalDelta.X;
                newHeight += totalDelta.Y;
                break;
            default:
                break;
        }

        // Enforce minimum size
        if (newWidth < minWidth)
        {
            if (handle == "TopLeft" || handle == "BottomLeft")
            {
                newLeft = _initialBounds.Right - minWidth;
            }
            newWidth = minWidth;
        }

        if (newHeight < minHeight)
        {
            if (handle == "TopLeft" || handle == "TopRight")
            {
                newTop = _initialBounds.Bottom - minHeight;
            }
            newHeight = minHeight;
        }

        // Determine the anchor point based on handle
        Point anchor;
        switch (handle)
        {
            case "TopLeft":
                anchor = new Point(_initialBounds.Right, _initialBounds.Bottom);
                break;
            case "TopRight":
                anchor = new Point(_initialBounds.Left, _initialBounds.Bottom);
                break;
            case "BottomLeft":
                anchor = new Point(_initialBounds.Right, _initialBounds.Top);
                break;
            case "BottomRight":
                anchor = new Point(_initialBounds.Left, _initialBounds.Top);
                break;
            default:
                anchor = new Point(_initialBounds.Left, _initialBounds.Top);
                break;
        }

        // Calculate scaling factors
        double scaleX = _initialBounds.Width != 0 ? newWidth / _initialBounds.Width : 1;
        double scaleY = _initialBounds.Height != 0 ? newHeight / _initialBounds.Height : 1;

        // Apply scaling to each point relative to the anchor
        List<Point> newPoints = new List<Point>();
        foreach (Point point in _initialPoints)
        {
            double newX = anchor.X + (point.X - anchor.X) * scaleX;
            double newY = anchor.Y + (point.Y - anchor.Y) * scaleY;
            newPoints.Add(new Point(newX, newY));
        }

        // Update the ScribbleShape's points
        scribble.Points = newPoints;
    }

    /// <summary>
    /// Resizes a circle shape by updating its center and radii based on the handle and mouse movement.
    /// Ensures the circle does not collapse below minimum size.
    /// </summary>
    private void ResizeCircleShape(CircleShape circle, string handle, Vector delta)
    {
        double minSize = 8; // Minimum size to prevent collapsing

        switch (handle)
        {
            case "TopLeft":
                {
                    double newLeft = circle.Left + delta.X;
                    double newTop = circle.Top + delta.Y;
                    double newWidth = circle.Width - delta.X;
                    double newHeight = circle.Height - delta.Y;

                    if (newWidth >= minSize && newHeight >= minSize)
                    {
                        circle.CenterX = newLeft + newWidth / 2;
                        circle.CenterY = newTop + newHeight / 2;
                        circle.RadiusX = newWidth / 2;
                        circle.RadiusY = newHeight / 2;
                    }
                    break;
                }
            case "TopRight":
                {
                    double newTop = circle.Top + delta.Y;
                    double newWidth = circle.Width + delta.X;
                    double newHeight = circle.Height - delta.Y;

                    if (newWidth >= minSize && newHeight >= minSize)
                    {
                        circle.CenterX = circle.Left + newWidth / 2;
                        circle.CenterY = newTop + newHeight / 2;
                        circle.RadiusX = newWidth / 2;
                        circle.RadiusY = newHeight / 2;
                    }
                    break;
                }
            case "BottomLeft":
                {
                    double newLeft = circle.Left + delta.X;
                    double newWidth = circle.Width - delta.X;
                    double newHeight = circle.Height + delta.Y;

                    if (newWidth >= minSize && newHeight >= minSize)
                    {
                        circle.CenterX = newLeft + newWidth / 2;
                        circle.CenterY = circle.Top + newHeight / 2;
                        circle.RadiusX = newWidth / 2;
                        circle.RadiusY = newHeight / 2;
                    }
                    break;
                }
            case "BottomRight":
                {
                    double newWidth = circle.Width + delta.X;
                    double newHeight = circle.Height + delta.Y;

                    if (newWidth >= minSize && newHeight >= minSize)
                    {
                        circle.CenterX = circle.Left + newWidth / 2;
                        circle.CenterY = circle.Top + newHeight / 2;
                        circle.RadiusX = newWidth / 2;
                        circle.RadiusY = newHeight / 2;
                    }
                    break;
                }
            default:
                break;
        }
    }

    /// <summary>
    /// Handles the right mouse button down event on text shapes.
    /// Generates a context menu with text-specific options.
    /// </summary>
    private void Shape_MouseRightButtonDownText(object sender, MouseButtonEventArgs e)
    {
        GenerateContextMenuForRightClick(sender, e, true);
    }

    /// <summary>
    /// Handles the right mouse button down event on non-text shapes.
    /// Generates a context menu with general shape options.
    /// </summary>
    private void Shape_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        GenerateContextMenuForRightClick(sender, e, false);
    }

    /// <summary>
    /// Generates and displays a context menu for the clicked shape.
    /// Adds additional options if the shape is a text shape.
    /// </summary>
    private void GenerateContextMenuForRightClick(object sender, MouseButtonEventArgs e, bool isText)
    {
        // Get the ViewModel
        var vm = this.DataContext as MainPageViewModel;

        // Check if the current tool is Select
        if (vm.CurrentTool != ShapeType.Select)
        {
            return;
        }

        // Get the shape from the sender's DataContext
        var shapeElement = sender as FrameworkElement;
        var shape = shapeElement.DataContext as IShape;

        // Check if the shape is the selected shape
        if (vm.SelectedShape != shape)
        {
            return;
        }

        // Create the ContextMenu
        var contextMenu = new ContextMenu();

        var sendBackwardMenuItem = new MenuItem {
            Header = "Send Backward",
            Command = vm.SendBackwardCommand,
            CommandParameter = shape
        };

        var sendToBackMenuItem = new MenuItem {
            Header = "Send to Back",
            Command = vm.SendToBackCommand,
            CommandParameter = shape
        };

        if (isText)
        {
            var editMenuItem = new MenuItem {
                Header = "Edit Text",
                Command = vm.EditTextCommand,
                CommandParameter = shape
            };
            contextMenu.Items.Add(editMenuItem);
        }

        contextMenu.Items.Add(sendBackwardMenuItem);
        contextMenu.Items.Add(sendToBackMenuItem);

        // Assign the ContextMenu to the shape element
        shapeElement.ContextMenu = contextMenu;

        // Open the ContextMenu
        contextMenu.IsOpen = true;

        // Mark the event as handled to prevent further processing
        e.Handled = true;
    }

    /// <summary>
    /// Handles the value changed event of the slider.
    /// Currently not implemented.
    /// </summary>
    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

    }
}


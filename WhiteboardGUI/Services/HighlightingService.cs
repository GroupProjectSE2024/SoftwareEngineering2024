/**************************************************************************************************
 * Filename    : HighlightingService.cs
 *
 * Author      : Rachit Jain
 *
 * Product     : WhiteBoard
 * 
 * Project     : Tooltip Feature for Canvas Shapes
 *
 * Description : Implements tooltip-like functionality that displays information about shapes
 *               and their creators when the user hovers over them on the canvas.
 *************************************************************************************************/


using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WhiteboardGUI.Adorners;
using WhiteboardGUI.Models;
using WhiteboardGUI.ViewModel;

namespace WhiteboardGUI.Services;

/// <summary>
/// Provides services for highlighting shapes on mouse hover by adding hover adorners.
/// </summary>
public static class HighlightingService
{
    /// <summary>
    /// Attached property to enable or disable highlighting for a UI element.
    /// </summary>
    public static readonly DependencyProperty EnableHighlightingProperty =
        DependencyProperty.RegisterAttached(
            "EnableHighlighting",
            typeof(bool),
            typeof(HighlightingService),
            new PropertyMetadata(false, OnEnableHighlightingChanged));

    /// <summary>
    /// Gets the value of the EnableHighlighting attached property.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <returns>A boolean value indicating whether highlighting is enabled.</returns>
    public static bool GetEnableHighlighting(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableHighlightingProperty);
    }

    /// <summary>
    /// Sets the value of the EnableHighlighting attached property.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">True to enable highlighting, false to disable.</param>
    public static void SetEnableHighlighting(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableHighlightingProperty, value);
    }

    /// <summary>
    /// Attached property to store a hover timer for each element.
    /// </summary>
    private static readonly DependencyProperty s_hoverTimerProperty =
        DependencyProperty.RegisterAttached(
            "HoverTimer",
            typeof(DispatcherTimer),
            typeof(HighlightingService),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets the hover timer associated with a dependency object.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <returns>The hover timer associated with the object.</returns>
    private static DispatcherTimer GetHoverTimer(DependencyObject obj)
    {
        return (DispatcherTimer)obj.GetValue(s_hoverTimerProperty);
    }

    /// <summary>
    /// Sets the hover timer for a dependency object.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">The hover timer to associate with the object.</param>
    private static void SetHoverTimer(DependencyObject obj, DispatcherTimer value)
    {
        obj.SetValue(s_hoverTimerProperty, value);
    }

    /// <summary>
    /// Called when the EnableHighlighting attached property changes.
    /// Adds or removes event handlers for mouse enter and leave events.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The event arguments containing old and new values.</param>
    private static void OnEnableHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseEnter += Element_MouseEnter;
                element.MouseLeave += Element_MouseLeave;
            }
            else
            {
                element.MouseEnter -= Element_MouseEnter;
                element.MouseLeave -= Element_MouseLeave;
            }
        }
    }

    /// <summary>
    /// Handles the mouse enter event to start hover detection and potentially show a hover adorner.
    /// </summary>
    private static void Element_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            DispatcherTimer hoverTimer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(0.4)
            };
            hoverTimer.Tick += (s, args) =>
            {
                hoverTimer.Stop();
                SetHoverTimer(element, null);

                MainPageViewModel? viewModel = FindParentViewModel(element);
                if (viewModel is MainPageViewModel vm && element.DataContext is IShape shape)
                {
                    Point elementPosition = element.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);

                    if (adornerLayer != null)
                    {
                        RemoveHoverAdorner(adornerLayer, vm);
                        vm.HoveredShape = shape;
                        vm.IsShapeHovered = true;
                        Color shapeColor = (Color)ColorConverter.ConvertFromString(shape.Color);
                        ImageSource imageSource = GetImageSourceForShape(shape);

                        HoverAdorner hoverAdorner = new HoverAdorner(element, vm.HoveredShapeDetails, elementPosition, imageSource, shapeColor);
                        adornerLayer.Add(hoverAdorner);

                        vm.CurrentHoverAdorner = hoverAdorner;
                    }
                }
            };

            SetHoverTimer(element, hoverTimer);
            hoverTimer.Start();
        }
    }

    /// <summary>
    /// Retrieves the image source for a shape.
    /// </summary>
    /// <param name="shape">The shape whose image source is needed.</param>
    /// <returns>The image source of the shape, or null if unavailable.</returns>
    private static ImageSource GetImageSourceForShape(IShape shape)
    {
        //string imagePath = "../Views/Assets/sirphoto.png";
        try
        {
            return new BitmapImage(new Uri(shape.ProfilePictureURL));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Handles the mouse leave event to stop hover detection and remove the hover adorner.
    /// </summary>
    private static void Element_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            DispatcherTimer hoverTimer = GetHoverTimer(element);
            hoverTimer?.Stop();
            SetHoverTimer(element, null);

            MainPageViewModel? viewModel = FindParentViewModel(element);
            if (viewModel is MainPageViewModel vm && element.DataContext is IShape shape)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);
                if (adornerLayer != null && vm.CurrentHoverAdorner != null)
                {
                    adornerLayer.Remove(vm.CurrentHoverAdorner);
                    vm.CurrentHoverAdorner = null;
                }

                vm.HoveredShape = null;
                vm.IsShapeHovered = false;
            }
        }
    }

    /// <summary>
    /// Finds the parent ViewModel from the visual tree.
    /// </summary>
    /// <param name="element">The child element whose ViewModel is to be found.</param>
    /// <returns>The <see cref="MainPageViewModel"/> associated with the element.</returns>
    private static MainPageViewModel? FindParentViewModel(FrameworkElement element)
    {
        DependencyObject parent = element;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is FrameworkElement fe && fe.DataContext is MainPageViewModel vm)
            {
                return vm;
            }
        }
        return null;
    }

    /// <summary>
    /// Removes the existing hover adorner from the adorner layer.
    /// </summary>
    /// <param name="adornerLayer">The adorner layer to modify.</param>
    /// <param name="vm">The ViewModel containing the current hover adorner.</param>
    private static void RemoveHoverAdorner(AdornerLayer adornerLayer, MainPageViewModel vm)
    {
        if (vm.CurrentHoverAdorner != null)
        {
            adornerLayer.Remove(vm.CurrentHoverAdorner);
            vm.CurrentHoverAdorner = null;
        }
    }
}

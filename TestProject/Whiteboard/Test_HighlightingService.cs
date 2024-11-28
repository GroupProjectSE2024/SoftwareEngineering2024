// File: UnitTests/Test_HighlightingService.cs

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WhiteboardGUI.Services;
using WhiteboardGUI.ViewModel;
using WhiteboardGUI.Models;
using WhiteboardGUI.Adorners;

namespace Whiteboard;

[TestClass]
public class HighlightingServiceTests
{
    /// <summary>
    /// Helper method to execute test actions on the STA thread.
    /// WPF requires UI components to be accessed on an STA thread.
    /// </summary>
    /// <param name="action">The test action to execute.</param>
    private void RunOnUIThread(Action action)
    {
        Exception capturedException = null;
        var done = new ManualResetEvent(false);
        var thread = new Thread(() =>
        {
            try
            {
                // Ensure an Application instance exists
                if (Application.Current == null)
                {
                    new Application();
                    
                }

                // Execute the test action
                action();
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
            finally
            {
                done.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // Wait for the action to complete or timeout after 5 seconds
        if (!done.WaitOne(TimeSpan.FromSeconds(5)))
        {
            Assert.Fail("Test execution timed out.");
        }

        if (capturedException != null)
        {
            throw new Exception("Exception in UI thread.", capturedException);
        }
    }

    /// <summary>
    /// Tests that multiple hover events correctly remove the previous HoverAdorner and add a new one.
    /// </summary>
    [TestMethod]
    public void MultipleHovers_RemoveAndAddHoverAdorner()
    {
        RunOnUIThread(() =>
        {
            // Arrange
            var window = new Window();
            var adornerDecorator = new AdornerDecorator();
            window.Content = adornerDecorator;

            var grid = new Grid();
            adornerDecorator.Child = grid;

            var element = new Button();
            grid.Children.Add(element);

            var viewModel = new MainPageViewModel();
            window.DataContext = viewModel;

            // Set Application.Current.MainWindow explicitly
            Application.Current.MainWindow = window;

            window.Show();

            HighlightingService.SetEnableHighlighting(element, true);

            // Mock shape 1 with a valid color
            var mockShape1 = new Mock<IShape>();
            mockShape1.Setup(s => s.ShapeId).Returns(Guid.NewGuid());
            mockShape1.Setup(s => s.ShapeType).Returns("Circle");
            mockShape1.Setup(s => s.Color).Returns("#FF0000"); // Valid color
            mockShape1.Setup(s => s.ProfilePictureURL).Returns("vishnu");

            // Mock shape 2 with a valid color
            var mockShape2 = new Mock<IShape>();
            mockShape2.Setup(s => s.ShapeId).Returns(Guid.NewGuid());
            mockShape2.Setup(s => s.ShapeType).Returns("Rectangle");
            mockShape2.Setup(s => s.Color).Returns("#00FF00"); // Valid color
            mockShape2.Setup(s => s.ProfilePictureURL).Returns("vishnu");

            // First hover
            element.DataContext = mockShape1.Object;
            var mouseEnterEvent1 = new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = UIElement.MouseEnterEvent
            };
            element.RaiseEvent(mouseEnterEvent1);

            // Wait for the DispatcherTimer to tick
            WaitForDispatcherTimer();

            // Assert first hover
            Assert.IsTrue(viewModel.IsShapeHovered, "IsShapeHovered should be true after first hover.");

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            Assert.IsNotNull(adornerLayer, "AdornerLayer should not be null.");

            // Second hover with a different shape
            element.DataContext = mockShape2.Object;
            var mouseEnterEvent2 = new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = UIElement.MouseEnterEvent
            };
            element.RaiseEvent(mouseEnterEvent2);

            // Wait for the DispatcherTimer to tick
            WaitForDispatcherTimer();

            // Assert second hover
            Assert.IsTrue(viewModel.IsShapeHovered, "IsShapeHovered should be true after second hover.");
            Assert.AreEqual(mockShape2.Object, viewModel.HoveredShape, "HoveredShape should be updated to the second shape.");

            Adorner[] adorners = adornerLayer.GetAdorners(element);
            Assert.IsNotNull(adorners, "Adorners should not be null after second hover.");
            Assert.IsTrue(adorners.Any(a => a is HoverAdorner), "HoverAdorner should be added after second hover.");

            // Clean up
            window.Close();
        });
    }

    /// <summary>
    /// Helper method to wait for the DispatcherTimer.
    /// </summary>
    private void WaitForDispatcherTimer()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}

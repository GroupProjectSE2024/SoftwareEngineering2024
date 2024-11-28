using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardGUI.Adorners;

namespace Whiteboard;

[TestClass]
public class Test_HoverAdorner
{
    private static Thread s_uiThread;
    private static AutoResetEvent s_uiThreadReady = new AutoResetEvent(false);
    private static Dispatcher s_uiDispatcher;
    private static Exception s_uiException;

    private HoverAdorner _hoverAdorner;
    private FrameworkElement _adornedElement;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        s_uiThread = new Thread(() =>
        {
            s_uiDispatcher = Dispatcher.CurrentDispatcher;
            s_uiThreadReady.Set();
            try
            {
                Dispatcher.Run(); // Start the Dispatcher loop
            }
            catch (Exception ex)
            {
                s_uiException = ex;
            }
        });

        s_uiThread.SetApartmentState(ApartmentState.STA);
        s_uiThread.Start();
        s_uiThreadReady.WaitOne(); // Wait until the Dispatcher is ready
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        s_uiDispatcher?.InvokeShutdown(); // Shut down the Dispatcher

        s_uiThread?.Join(); // Ensure the thread has exited
    }

    [TestInitialize]
    public void Setup()
    {
        RunOnUIThread(() =>
        {
            // Create a simple adorned element
            _adornedElement = new Canvas
            {
                Width = 500,
                Height = 500
            };

            // Create a HoverAdorner instance
            _hoverAdorner = new HoverAdorner(
                _adornedElement,
                text: "Sample Text",
                mousePosition: new Point(50, 50),
                imageSource: null,
                shapeColor: Colors.Blue);
        });
    }

    [TestMethod]
    public void HoverAdorner_VisualChildrenCount_ReturnsExpectedValue()
    {
        RunOnUIThread(() =>
        {
            PropertyInfo visualChildrenCountProperty = typeof(HoverAdorner).GetProperty("VisualChildrenCount", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(visualChildrenCountProperty);

            int visualChildrenCount = (int)visualChildrenCountProperty.GetValue(_hoverAdorner);

            Assert.AreEqual(1, visualChildrenCount);
        });
    }

    [TestMethod]
    public void HoverAdorner_GetVisualChild_ReturnsCorrectVisual()
    {
        RunOnUIThread(() =>
        {
            MethodInfo getVisualChildMethod = typeof(HoverAdorner).GetMethod("GetVisualChild", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(getVisualChildMethod);

            object? visualChild = getVisualChildMethod.Invoke(_hoverAdorner, new object[] { 0 });

            Assert.IsInstanceOfType(visualChild, typeof(Border));
        });
    }

    [TestMethod]
    public void HoverAdorner_ArrangeOverride_AdjustsWithinBounds()
    {
        RunOnUIThread(() =>
        {
            MethodInfo arrangeOverrideMethod = typeof(HoverAdorner).GetMethod("ArrangeOverride", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(arrangeOverrideMethod);

            var arrangeBounds = new Size(300, 300);

            var finalSize = (Size)arrangeOverrideMethod.Invoke(_hoverAdorner, new object[] { arrangeBounds });

            Assert.AreEqual(arrangeBounds, finalSize);
        });
    }

    [TestMethod]
    public void HoverAdorner_MeasureOverride_ReturnsDesiredSize()
    {
        RunOnUIThread(() =>
        {
            MethodInfo measureOverrideMethod = typeof(HoverAdorner).GetMethod("MeasureOverride", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(measureOverrideMethod);

            var availableSize = new Size(300, 300);

            var desiredSize = (Size)measureOverrideMethod.Invoke(_hoverAdorner, new object[] { availableSize });

            Assert.IsTrue(desiredSize.Width > 0);
            Assert.IsTrue(desiredSize.Height > 0);
        });
    }

    private void RunOnUIThread(Action action)
    {
        if (s_uiDispatcher == null)
        {
            throw new InvalidOperationException("Dispatcher is not initialized.");
        }

        s_uiDispatcher.Invoke(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                s_uiException = ex;
            }
        });

        if (s_uiException != null)
        {
            Exception exception = s_uiException;
            s_uiException = null;
            throw exception;
        }
    }
}

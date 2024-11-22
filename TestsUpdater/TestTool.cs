/******************************************************************************
* Filename    = TestTool.cs
*
* Author      = Garima Ranjan 
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Unit Tests for Tool.cs
*****************************************************************************/
using ViewModel.UpdaterViewModel;

namespace TestsUpdater;
/// <summary>
/// Unit tests for the Tool class to ensure proper functionality and 
/// that PropertyChanged events are raised correctly when properties change.
/// </summary>

[TestClass]
public class TestTool
{
    private Tool? _tool;
    /// <summary>
    /// Setup method that runs before each test to initialize the Tool instance.
    /// </summary>

    [TestInitialize]
    public void Setup()
    {
        _tool = new Tool();
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the ID property changes.
    /// </summary>

    [TestMethod]
    public void TestIdShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        // Null-check for _tool to resolve CS8602 warning
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.ID))
                {
                    eventRaised = true;
                }
            };

            _tool.ID = "123";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for ID.");
            Assert.AreEqual("123", _tool.ID, "ID was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }

    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the Version property changes.
    /// </summary>

    [TestMethod]
    public void TestVersionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.Version))
                {
                    eventRaised = true;
                }
            };

            _tool.Version = "1.0.0";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Version.");
            Assert.AreEqual("1.0.0", _tool.Version, "Version was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the Description property changes.
    /// </summary>

    [TestMethod]
    public void TestDescriptionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.Description))
                {
                    eventRaised = true;
                }
            };

            _tool.Description = "Sample tool description.";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Description.");
            Assert.AreEqual("Sample tool description.", _tool.Description, "Description was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the Deprecated property changes.
    /// </summary>

    [TestMethod]
    public void TestDeprecatedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.Deprecated))
                {
                    eventRaised = true;
                }
            };

            _tool.Deprecated = "Yes";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Deprecated.");
            Assert.AreEqual("Yes", _tool.Deprecated, "Deprecated was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the CreatedBy property changes.
    /// </summary>

    [TestMethod]
    public void TestCreatedByShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.CreatedBy))
                {
                    eventRaised = true;
                }
            };

            _tool.CreatedBy = "Jane Doe";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for CreatedBy.");
            Assert.AreEqual("Jane Doe", _tool.CreatedBy, "CreatedBy was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the CreatorEmail property changes.
    /// </summary>

    [TestMethod]
    public void TestCreatorEmailShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.CreatorEmail))
                {
                    eventRaised = true;
                }
            };

            _tool.CreatorEmail = "janedoe@example.com";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for CreatorEmail.");
            Assert.AreEqual("janedoe@example.com", _tool.CreatorEmail, "CreatorEmail was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }

    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the LastUpdated property changes.
    /// </summary>

    [TestMethod]
    public void TestLastUpdatedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.LastUpdated))
                {
                    eventRaised = true;
                }
            };

            _tool.LastUpdated = "2024-11-17";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for LastUpdated.");
            Assert.AreEqual("2024-11-17", _tool.LastUpdated, "LastUpdated was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
    /// <summary>
    /// Test to ensure that the PropertyChanged event is raised when the LastModified property changes.
    /// </summary>

    [TestMethod]
    public void TestLastModifiedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.LastModified))
                {
                    eventRaised = true;
                }
            };

            _tool.LastModified = "2024-11-16";

            Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for LastModified.");
            Assert.AreEqual("2024-11-16", _tool.LastModified, "LastModified was not set correctly.");
        }
        else
        {
            Assert.Fail("Tool object is null.");

        }
    }
    /// <summary>
    /// Test to ensure that PropertyChanged event is not raised when setting the same value for a property.
    /// </summary>

    [TestMethod]
    public void TestPropertyChangedEventIsNotRaisedWhenSettingSameValue()
    {
        bool eventRaised = false;
        if (_tool != null)
        {
            _tool.ID = "123"; // Setting initial value
            _tool.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Tool.ID))
                {
                    eventRaised = true;
                }
            };
            _tool.ID = "123"; // Setting the same value again

            Assert.IsFalse(eventRaised, "PropertyChanged event should not be raised when setting the same value.");
        }
        else
        {
            Assert.Fail("Tool object is null.");
        }
    }
}

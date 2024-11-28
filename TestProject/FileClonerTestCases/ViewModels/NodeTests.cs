using FileCloner.ViewModels;
using FileCloner.Models;

namespace FileClonerTestCases;

[TestClass]
public class NodeTests
{
    [TestMethod]
    public void CheckBoxClick_DeselectUpdatesCountersCorrectly()
    {
        // Arrange
        Node folder = new Node { Name = "Folder", IsFile = false, RelativePath = "Root/Folder" };
        Node file = new Node { Name = "File", IsFile = true, Size = 50, RelativePath = "Root/Folder/File" };

        folder.Children.Add(file);
        file.Parent = folder;

        // Reset static counters
        Node.SelectedFilesCount = 0;
        Node.SelectedFolderCount = 0;
        Node.SumOfSelectedFilesSizeInBytes = 0;

        // Act: Select the file, then deselect it
        file.IsChecked = true;
        file.CheckBoxClick();

        file.IsChecked = false;
        file.CheckBoxClick();

        // Assert
        Assert.AreEqual(0, Node.SelectedFilesCount, "Selected files count should be 0 after deselection.");
        Assert.AreEqual(0, Node.SumOfSelectedFilesSizeInBytes, "Sum of selected files' sizes should be 0 after deselection.");
        Assert.AreEqual(0, Node.SelectedFolderCount, "Selected folders count should be 0 after deselection.");
    }

    [TestMethod]
    public void CheckBoxClick_MultipleFilesSelectionUpdatesCounters()
    {
        // Arrange
        Node file1 = new Node { Name = "File1", IsFile = true, Size = 100, RelativePath = "Root/File1" };
        Node file2 = new Node { Name = "File2", IsFile = true, Size = 200, RelativePath = "Root/File2" };

        // Reset static counters
        Node.SelectedFilesCount = 0;
        Node.SumOfSelectedFilesSizeInBytes = 0;

        // Act
        file1.IsChecked = true;
        file1.CheckBoxClick();

        file2.IsChecked = true;
        file2.CheckBoxClick();

        // Assert
        Assert.AreEqual(2, Node.SelectedFilesCount, "Selected files count should be 2.");
        Assert.AreEqual(300, Node.SumOfSelectedFilesSizeInBytes, "Total size of selected files should be 300.");
    }

    [TestMethod]
    public void CheckBoxClick_ParentStateUpdatesCorrectlyWhenAllChildrenSelected()
    {
        // Arrange
        Node parent = new Node { Name = "Parent", IsFile = false, RelativePath = "Root/Parent" };
        Node child1 = new Node { Name = "Child1", IsFile = true, Size = 100, RelativePath = "Root/Parent/Child1" };
        Node child2 = new Node { Name = "Child2", IsFile = true, Size = 200, RelativePath = "Root/Parent/Child2" };

        parent.Children.Add(child1);
        parent.Children.Add(child2);
        child1.Parent = parent;
        child2.Parent = parent;

        // Act
        child1.IsChecked = true;
        child1.CheckBoxClick();

        child2.IsChecked = true;
        child2.CheckBoxClick();

        // Assert
        Assert.IsTrue(parent.IsChecked, "Parent should be marked as checked when all children are selected.");
    }
}

/******************************************************************************
 * Filename    = Node.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Represents a node in a file structure with checkbox functionality,
 *               managing selection states and updating counters for selected files
 *               and folders.
 *****************************************************************************/
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using FileCloner.Models;

namespace FileCloner.ViewModels;

/// <summary>
/// Represents a file or folder node in the file tree, supporting selection tracking
/// and hierarchical state updates for its parent and child nodes.
/// </summary>
public class Node : ViewModelBase
{
    // UI Bindings
    public ObservableCollection<Node> Children { get; }

    //If it is set to true we will update it with a check in the UI
    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set {
            _isChecked = value;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    //The following properties store the metadata of a node (which can be a file or a folder).
    private string _fullFilePath;

    public string FullFilePath
    {
        get => _fullFilePath;
        set => _fullFilePath = value;
    }

    private string _ipAddress;
    public string IpAddress
    {
        get => _ipAddress;
        set => _ipAddress = value;
    }

    private string _userName;
    public string UserName
    {
        get => _userName;
        set => _userName = value;
    }

    private string _lastModified;
    public string LastModified
    {
        get => _lastModified;
        set => _lastModified = value;
    }

    private string _color;

    public string Color
    {
        get => _color;
        set => _color = value;
    }

    private Uri _iconPath;
    public Uri IconPath
    {
        get => _iconPath;
        set {
            _iconPath = value;
            OnPropertyChanged(nameof(IconPath));
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    // Class members
    public Node? Parent { get; set; }
    public ICommand CheckBoxCommand { get; }
    public static event Action CheckBoxClickEvent;
    public bool IsFile { get; set; }
    public int Size { get; set; }
    public string RelativePath { get; set; }

    // Static fields to track selected files, folders, and total file size
    public static int SelectedFolderCount = 0;
    public static int SelectedFilesCount = 0;
    public static int SumOfSelectedFilesSizeInBytes = 0;

    /// <summary>
    /// Initializes a new instance of the Node class with default properties.
    /// </summary>
    public Node()
    {
        _isChecked = false;
        _ipAddress = "localhost";
        _userName = "localhost";
        _color = "";
        _fullFilePath = "";
        Children = [];
        IconPath = new Uri(Constants.LoadingIconPath, UriKind.Absolute);
        CheckBoxCommand = new RelayCommand(CheckBoxClick);
        LastModified = "";
        Size = 0;
    }

    // Handle checkbox click logic
    public void CheckBoxClick()
    {
        IsChecked = _isChecked;

        if (IsFile)
        {
            MainPageViewModel.UpdateSelectedFiles(IpAddress, FullFilePath, RelativePath, IsChecked);
        }

        // Update static counters based on node's type and checked state
        if (IsChecked)
        {
            if (IsFile)
            {
                SelectedFilesCount++;
                SumOfSelectedFilesSizeInBytes += Size;
            }
            else
            {
                SelectedFolderCount++;
            }
        }
        else
        {
            if (IsFile)
            {
                SelectedFilesCount--;
                SumOfSelectedFilesSizeInBytes -= Size;
            }
            else
            {
                SelectedFolderCount--;
            }
        }

        UpdateParentCheckState();
        UpdateChildrenCheckState(IsChecked);

        // Trigger the static event to notify MainPageViewModel
        CheckBoxClickEvent?.Invoke();
    }

    /// <summary>
    /// Updates the IsChecked state of all child nodes recursively and adjusts selection counters.
    /// </summary>
    /// <param name="isChecked">Boolean value representing the desired checked state.</param>
    [ExcludeFromCodeCoverage]
    private void UpdateChildrenCheckState(bool isChecked)
    {
        foreach (Node child in Children)
        {
            child.IsChecked = isChecked;


            if (child.IsFile)
            {
                //MainPageViewModel.UpdateSelectedFiles(Address, FullPath, _isChecked);
                MainPageViewModel.UpdateSelectedFiles(child.IpAddress, child.FullFilePath, child.RelativePath, _isChecked);
            }

            // Update selection counters based on child's state and type
            if (isChecked)
            {
                if (child.IsFile)
                {
                    SelectedFilesCount++;
                    SumOfSelectedFilesSizeInBytes += child.Size;
                }
                else
                {
                    SelectedFolderCount++;
                }
            }
            else
            {
                if (child.IsFile)
                {
                    SelectedFilesCount--;
                    SumOfSelectedFilesSizeInBytes -= child.Size;
                }
                else
                {
                    SelectedFolderCount--;
                }
            }

            // Recursive call for nested children
            child.UpdateChildrenCheckState(isChecked);
        }
    }

    /// <summary>
    /// Updates the IsChecked and IsPartiallyChecked states of parent nodes based on sibling states.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void UpdateParentCheckState()
    {
        if (Parent == null)
        {
            return;
        }

        bool allChecked = true;

        // Check all siblings' IsChecked state to determine parent's state
        foreach (Node sibling in Parent.Children)
        {
            if (!sibling.IsChecked)
            {
                allChecked = false;
                break;
            }
        }

        // Update selection counters if parent's checked state changes
        if (!Parent.IsChecked && allChecked)
        {
            SelectedFolderCount++;
        }
        else if (Parent.IsChecked && !allChecked)
        {
            SelectedFolderCount--;
        }

        Parent.IsChecked = allChecked;
        Parent.UpdateParentCheckState(); // Recursive call for ancestors
    }
}

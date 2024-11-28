using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content;


public class ChatMessage : INotifyPropertyChanged
{
    private string _content;
    private bool _isDeleted;

    public string User { get; set; }
    public string Time { get; set; }
    public bool IsSentByUser { get; set; }
    public string? HighlightedText { get; set; }
    public string? HighlightedAfterText { get; set; }
    public string? UserProfileUrl {  get; set; }

    /// <summary>
    /// Gets or sets the content of the message. 
    /// Raises the PropertyChanged event if the content changes.
    /// </summary>

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the message is deleted. 
    /// Raises the PropertyChanged event if the deleted state changes.
    /// </summary>

    public bool IsDeleted
    {
        get => _isDeleted;
        set
        {
            if (_isDeleted != value)
            {
                _isDeleted = value;
                OnPropertyChanged(nameof(IsDeleted));
            }
        }
    }

    public string Text { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Notifies listeners about property value changes, enabling data binding updates.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Initializes a new instance of the ChatMessage class with the specified user, content, timestamp, and sender status.
    /// Sets the default value for IsDeleted to false.
    /// </summary>
    /// <param name="user">The username of the message sender.</param>
    /// <param name="content">The content of the message.</param>
    /// <param name="time">The timestamp of when the message was sent.</param>
    /// <param name="isSentByUser">Indicates whether the message was sent by the current user.</param>

    public ChatMessage(string user, string content, string time, bool isSentByUser, string? userProfileUrl)
    {
        User = user;
        Content = content;
        Time = time;
        IsSentByUser = isSentByUser;
        Text = content;
        IsDeleted = false;
        UserProfileUrl = userProfileUrl;
    }
}
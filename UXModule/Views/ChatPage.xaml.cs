using Content;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Content.ChatViewModel;

namespace UXModule.Views;
/// <summary>
/// Interaction logic for ChatPage.xaml
/// </summary>
public partial class ChatPage : Page
{
    private ChatClient _client;
    private MainViewModel _viewModel;

    /// <summary>
    /// Initializes the ContentPage, setting up the ViewModel, binding data to the UI, 
    /// and subscribing to ViewModel events.
    /// </summary>

    public ChatPage()
    {
        InitializeComponent();

        // Initialize ViewModel
        _viewModel = MainViewModel.GetInstance;
        DataContext = _viewModel;

        // Bind Messages to ListView
        MessagesListView.ItemsSource = _viewModel.Messages;

        // Subscribe to ViewModel events
        _viewModel.MessageAdded += OnMessageAdded;
    }

    /// <summary>
    /// Updates the client list in the dropdown menu with the latest values from the ViewModel.
    /// Ensures no default selection is made.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="clientList">The updated list of clients.</param>

    private void UpdateClientList(object sender, List<string> clientList)
    {
        Dispatcher.Invoke(() => {
            ClientDropdown.ItemsSource = clientList;
            ClientDropdown.SelectedIndex = -1; // Ensure no default selection
        });
    }

    /// <summary>
    /// Scrolls the MessagesListView to the most recently added message.
    /// </summary>
    /// <param name="message">The newly added chat message.</param>

    private void OnMessageAdded(ChatMessage message)
    {
        Dispatcher.Invoke(() => {
            if (_viewModel.Messages.Any())
            {
                MessagesListView.ScrollIntoView(_viewModel.Messages.Last());
            }
        });
    }

    /// <summary>
    /// Handles focus for the MessageTextBox by clearing placeholder text and resetting font style.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>


    private void MessageTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (MessageTextBox.Text == "  Type something...")
        {
            MessageTextBox.Text = string.Empty;
            MessageTextBox.FontStyle = FontStyles.Normal;
        }
    }

    /// <summary>
    /// Handles focus for the SearchTextBox by clearing placeholder text and updating its appearance.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>

    private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (SearchTextBox.Text == " ")
        {
            SearchPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#190019"));
            SearchTextBox.Text = string.Empty;
            SearchTextBox.FontStyle = FontStyles.Normal;
        }
    }

    /// <summary>
    /// Handles the ESC key in the SearchTextBox to clear search results and reset the view.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data containing the pressed key.</param>

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClearSearch();
        }
    }

    /// <summary>
    /// Handles the search button click to initiate a search panel animation and transition.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        PerformSearchTransition();
    }

    /// <summary>
    /// Reverses the search panel animation, returning to the main view.
    /// </summary>

    private void ReverseSearchTransition()
    {
        var fadeOutStoryboard = new Storyboard();
        var fadeOutAnimation = new DoubleAnimation {
            To = 0,
            Duration = TimeSpan.FromSeconds(0.5)
        };

        fadeOutAnimation.Completed += (s, args) => {
            SearchPanel.Visibility = Visibility.Collapsed;
            TopDockPanel.Visibility = Visibility.Visible;
            TopDockPanel.Opacity = 0;
        };

        Storyboard.SetTarget(fadeOutAnimation, SearchPanel);
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
        fadeOutStoryboard.Children.Add(fadeOutAnimation);

        var fadeInAnimation = new DoubleAnimation {
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5)
        };

        Storyboard.SetTarget(fadeInAnimation, TopDockPanel);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
        fadeOutStoryboard.Children.Add(fadeInAnimation);

        fadeOutStoryboard.Begin();
    }

    /// <summary>
    /// Clears the search input and results, restoring the original message list.
    /// Scrolls to the last message in the list.
    /// </summary>

    private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSearch();
    }

    // Clear search logic
    private void ClearSearch()
    {
        SearchTextBox.Clear();
        _viewModel.SearchResults?.Clear();
        _viewModel.BackToOriginalMessages();
        MessagesListView.ItemsSource = _viewModel.Messages;
        if (_viewModel.Messages != null && _viewModel.Messages.Any())
        {
            MessagesListView.ScrollIntoView(_viewModel.Messages.Last());
        }
    }

    /// <summary>
    /// Handles the back button click during a search, clearing results and reversing the search panel transition.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>

    private void BackFromSearchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.Messages != null)
        {
            ClearSearch();
        }
        ReverseSearchTransition();
    }

    // Handle dynamic search
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PerformDynamicSearch();
    }

    /// <summary>
    /// Dynamically searches messages as the user types in the SearchTextBox.
    /// Updates the MessagesListView with matching results or the original message list.
    /// </summary>
    /// 
    private void PerformDynamicSearch()
    {
        string query = SearchTextBox.Text;

        if (!string.IsNullOrWhiteSpace(query))
        {
            _viewModel.SearchMessages(query);
            MessagesListView.ItemsSource = _viewModel.SearchResults;
        }
        else
        {
            _viewModel.BackToOriginalMessages();
            MessagesListView.ItemsSource = _viewModel.Messages;
        }
    }

    /// <summary>
    /// Toggles the visibility of the emoji popup for selecting emojis.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>

    private void EmojiPopupButton_Click(object sender, RoutedEventArgs e)
    {
        EmojiPopup.IsOpen = !EmojiPopup.IsOpen;
    }

    /// <summary>
    /// Adds a selected emoji to the message input box and maintains the caret position.
    /// </summary>
    /// <param name="sender">The event source (the selected emoji button).</param>
    /// <param name="e">Event data.</param>

    private void Emoji_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button emojiButton)
        {
            string emoji = emojiButton.Content.ToString();

            if (MessageTextBox.Text == "  Type something...")
            {
                MessageTextBox.Text = string.Empty;
                MessageTextBox.FontStyle = FontStyles.Normal;
            }

            MessageTextBox.Text += emoji;
            MessageTextBox.Focus();
            MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
        }
    }

    /// <summary>
    /// Handles the options button click to display a context menu for actions like deleting messages.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>

    private void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// Deletes a message by invoking the ViewModel's delete logic.
    /// </summary>
    /// <param name="sender">The event source (the delete menu item).</param>
    /// <param name="e">Event data.</param>

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
        {
            var placementTarget = contextMenu.PlacementTarget as FrameworkElement;
            if (placementTarget?.DataContext is ChatMessage message)
            {
                _viewModel.DeleteMessage(message);
            }
        }
    }

    /// <summary>
    /// Performs the search animation, transitioning from the main view to the search panel.
    /// </summary>

    private void PerformSearchTransition()
    {
        SearchPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0A392"));
        var fadeOutStoryboard = new Storyboard();
        var fadeOutAnimation = new DoubleAnimation {
            To = 0,
            Duration = TimeSpan.FromSeconds(0.5)
        };

        fadeOutAnimation.Completed += (s, args) => {
            TopDockPanel.Visibility = Visibility.Collapsed;
            SearchPanel.Visibility = Visibility.Visible;
            SearchPanel.Opacity = 0;
            SearchTextBox.Focus();
        };

        Storyboard.SetTarget(fadeOutAnimation, TopDockPanel);
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
        fadeOutStoryboard.Children.Add(fadeOutAnimation);

        var fadeInAnimation = new DoubleAnimation {
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5)
        };

        Storyboard.SetTarget(fadeInAnimation, SearchPanel);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
        fadeOutStoryboard.Children.Add(fadeInAnimation);

        fadeOutStoryboard.Begin();
    }


}

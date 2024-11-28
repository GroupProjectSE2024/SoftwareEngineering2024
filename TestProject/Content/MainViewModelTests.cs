using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;
using Content.ChatViewModel;
using Content;

namespace Content.Tests;

[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public void ChatHistory_PropertyChangedEventTriggered()
    {
        // Test to ensure that when the ChatHistory property is updated, the PropertyChanged event is triggered.
        var viewModel = new MainViewModel();
        bool eventTriggered = false;
        viewModel.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(viewModel.ChatHistory))
            {
                eventTriggered = true;
            }
        };

        viewModel.ChatHistory = "New chat history";

        Assert.IsTrue(eventTriggered);
        Assert.AreEqual("New chat history", viewModel.ChatHistory);
    }

    [TestMethod]
    public void Message_PropertyChangedEventTriggered()
    {
        // Test to verify that updating the Message property triggers the PropertyChanged event.
        var viewModel = new MainViewModel();
        bool eventTriggered = false;
        viewModel.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(viewModel.Message))
            {
                eventTriggered = true;
            }
        };

        viewModel.Message = "New message";

        Assert.IsTrue(eventTriggered);
        Assert.AreEqual("New message", viewModel.Message);
    }

    [TestMethod]
    public void IsNotFoundPopupOpen_PropertyChangedEventTriggered()
    {
        // Test to check if updating the IsNotFoundPopupOpen property triggers the PropertyChanged event.
        var viewModel = new MainViewModel();
        bool eventTriggered = false;
        viewModel.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(viewModel.IsNotFoundPopupOpen))
            {
                eventTriggered = true;
            }
        };

        viewModel.IsNotFoundPopupOpen = true;

        Assert.IsTrue(eventTriggered);
        Assert.IsTrue(viewModel.IsNotFoundPopupOpen);
    }

    [TestMethod]
    public void DeleteMessage_MarksMessageAsDeleted()
    {
        // Test to verify that calling DeleteMessage marks the message as deleted and updates its content.
        MainViewModel viewModel = new MainViewModel();
        ChatMessage message = new ChatMessage("User1", "Original message", "10:00 AM", true, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");

        viewModel.DeleteMessage(message);

        Assert.IsTrue(message.IsDeleted);
        Assert.AreEqual("[Message deleted]", message.Content);
        Assert.AreEqual("[Message deleted]", message.Text);
    }

    [TestMethod]
    public void SearchMessages_FindsMatchingMessages()
    {
        // Test to ensure that SearchMessages correctly finds messages that match the search term.
        var viewModel = new MainViewModel();
        viewModel.Messages.Add(new ChatMessage("User1", "Hello World", "10:00 AM", true, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180"));
        viewModel.Messages.Add(new ChatMessage("User2", "Hello Universe", "10:01 AM", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180"));

        viewModel.SearchMessages("Hello");

        Assert.AreEqual(2, viewModel.SearchResults.Count);
    }

    [TestMethod]
    public void SearchMessages_HighlightsMatchingText()
    {
        // Test to verify that SearchMessages correctly highlights the matching text within the message.
        var viewModel = new MainViewModel();
        var message = new ChatMessage("User1", "Hello World", "10:00 AM", true, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        viewModel.Messages.Add(message);

        viewModel.SearchMessages("World");

        ChatMessage result = viewModel.SearchResults.First();
        Assert.AreEqual("Hello ", result.Content);
        Assert.AreEqual("World", result.HighlightedText);
        Assert.AreEqual("", result.HighlightedAfterText);
    }

    [TestMethod]
    public void BackToOriginalMessages_RestoresOriginalMessages()
    {
        // Test to ensure that BackToOriginalMessages restores the original content and clears highlighted text.
        var viewModel = new MainViewModel();
        var message = new ChatMessage("User1", "Hello World", "10:00 AM", true, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        viewModel.Messages.Add(message);
        viewModel.SearchMessages("World");

        viewModel.BackToOriginalMessages();

        Assert.AreEqual("Hello World", message.Content);
        Assert.AreEqual("", message.HighlightedText);
        Assert.AreEqual("", message.HighlightedAfterText);
    }

    [TestMethod]
    public void AddMessage_MessageIsAddedToCollection()
    {
        // Arrange
        var viewModel = new MainViewModel();
        var message = new ChatMessage("User1", "New message", "10:00 AM", true, "image-url");

        // Act
        viewModel.Messages.Add(message);

        // Assert
        Assert.AreEqual(1, viewModel.Messages.Count);
        Assert.AreEqual(message, viewModel.Messages[0]);
    }

    [TestMethod]
    public void ChatHistory_PropertyUpdate_UpdatesCorrectly()
    {
        // Arrange
        var viewModel = new MainViewModel {
            ChatHistory = "Initial chat history"
        };

        // Act
        viewModel.ChatHistory = "Updated chat history";

        // Assert
        Assert.AreEqual("Updated chat history", viewModel.ChatHistory);
    }

    [TestMethod]
    public void AddMessage_WithDuplicateContent_DuplicateHandledCorrectly()
    {
        // Arrange
        var viewModel = new MainViewModel();
        var message1 = new ChatMessage("User1", "Hello", "10:00 AM", true, "image-url");
        var message2 = new ChatMessage("User2", "Hello", "10:01 AM", false, "image-url");

        viewModel.Messages.Add(message1);

        // Act
        viewModel.Messages.Add(message2); ;  // Adding duplicate message

        // Assert
        Assert.AreEqual(2, viewModel.Messages.Count); // Check that both messages are added (if duplicates are allowed)
    }

    [TestMethod]
    public void SearchMessages_CaseInsensitiveSearch_FindsMatches()
    {
        // Arrange
        var viewModel = new MainViewModel();
        viewModel.Messages.Add(new ChatMessage("User1", "Hello World", "10:00 AM", true, "image-url"));
        viewModel.Messages.Add(new ChatMessage("User2", "HELLO universe", "10:01 AM", false, "image-url"));

        // Act
        viewModel.SearchMessages("hello");

        // Assert
        Assert.AreEqual(2, viewModel.SearchResults.Count); // Both messages should be found (case insensitive)
    }

    [TestMethod]
    public void SearchMessages_FindsMessages_DifferentCases()
    {
        var viewModel = new MainViewModel();
        ChatMessage message1 = new ChatMessage("User1", "Hello how are you", "10:00 ", true, "https://example.com/avatar1.png");
        viewModel.Messages.Add(message1);
        viewModel.Messages.Add(new ChatMessage("User2", "Fine how about you", "10:01 ", false, "https://example.com/avatar2.png"));
        string query = "fine";


        viewModel.SearchMessages(query);

        // Assert
        Assert.AreEqual(1, viewModel.SearchResults.Count);
        Assert.AreEqual("", viewModel.SearchResults[0].Content);
        Assert.AreEqual("Fine", viewModel.SearchResults[0].HighlightedText);
        Assert.AreEqual(" how about you", viewModel.SearchResults[0].HighlightedAfterText);

        query = "Bye";
        viewModel.SearchMessages(query);

        // Assert
        Assert.AreEqual(0, viewModel.SearchResults.Count);

        query = string.Empty;
        viewModel.SearchMessages(query);

        //Assert
        Assert.AreEqual(0, viewModel.SearchResults.Count);

        viewModel.DeleteMessage(message1);
        query = "Hello";
        viewModel.SearchMessages(query);

        //Assert
        Assert.AreEqual(0, viewModel.SearchResults.Count);

    }
}

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Content;

namespace Content.Tests;

[TestClass]
public class ChatMessageTests
{
    [TestMethod]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Tests if the constructor initializes the properties correctly
        string user = "JohnDoe";
        string content = "Hello, world!";
        string time = "2024-11-20T10:00:00Z";
        bool isSentByUser = true;
        string userProfileUrl = "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180";

        var message = new ChatMessage(user, content, time, isSentByUser, userProfileUrl);

        Assert.AreEqual(user, message.User);
        Assert.AreEqual(content, message.Content);
        Assert.AreEqual(time, message.Time);
        Assert.AreEqual(isSentByUser, message.IsSentByUser);
        Assert.AreEqual(content, message.Text);
        Assert.IsFalse(message.IsDeleted);
    }

    [TestMethod]
    public void Content_Setter_RaisesPropertyChangedEvent()
    {
        // Tests if setting the Content property raises the PropertyChanged event
        var message = new ChatMessage("User", "Old Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        bool eventRaised = false;
        message.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(ChatMessage.Content))
            {
                eventRaised = true;
            }
        };

        message.Content = "New Content";

        Assert.IsTrue(eventRaised);
        Assert.AreEqual("New Content", message.Content);
    }

    [TestMethod]
    public void Content_Setter_DoesNotRaiseEvent_IfValueUnchanged()
    {
        // Tests if setting the Content property to the same value does not raise the PropertyChanged event
        var message = new ChatMessage("User", "Same Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        bool eventRaised = false;
        message.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(ChatMessage.Content))
            {
                eventRaised = true;
            }
        };

        message.Content = "Same Content";

        Assert.IsFalse(eventRaised);
    }

    [TestMethod]
    public void IsDeleted_Setter_RaisesPropertyChangedEvent()
    {
        // Tests if setting the IsDeleted property raises the PropertyChanged event
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        bool eventRaised = false;
        message.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(ChatMessage.IsDeleted))
            {
                eventRaised = true;
            }
        };

        message.IsDeleted = true;

        Assert.IsTrue(eventRaised);
        Assert.IsTrue(message.IsDeleted);
    }

    [TestMethod]
    public void IsDeleted_Setter_DoesNotRaiseEvent_IfValueUnchanged()
    {
        // Tests if setting the IsDeleted property to the same value does not raise the PropertyChanged event
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        bool eventRaised = false;
        message.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(ChatMessage.IsDeleted))
            {
                eventRaised = true;
            }
        };

        message.IsDeleted = false;

        Assert.IsFalse(eventRaised);
    }

    [TestMethod]
    public void PropertyChangedEvent_IsRaisedCorrectlyForMultipleProperties()
    {
        // Tests if the PropertyChanged event is raised correctly for multiple properties
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        var changedProperties = new List<string>();
        message.PropertyChanged += (sender, e) => {
            changedProperties.Add(e.PropertyName);
        };

        message.Content = "Updated Content";
        message.IsDeleted = true;

        Assert.AreEqual(2, changedProperties.Count);
        CollectionAssert.Contains(changedProperties, nameof(ChatMessage.Content));
        CollectionAssert.Contains(changedProperties, nameof(ChatMessage.IsDeleted));
    }

    [TestMethod]
    public void HighlightedText_DefaultIsNull()
    {
        // Tests if the default value of HighlightedText is null
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");

        Assert.IsNull(message.HighlightedText);
    }

    [TestMethod]
    public void HighlightedAfterText_DefaultIsNull()
    {
        // Tests if the default value of HighlightedAfterText is null
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");

        Assert.IsNull(message.HighlightedAfterText);
    }

    [TestMethod]
    public void Text_PropertyReflectsContent()
    {
        // Tests if the Text property reflects the Content property correctly
        var message = new ChatMessage("User", "Initial Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180") {
            Content = "Updated Content"
        };
        Assert.AreEqual("Updated Content", message.Content);
    }

    [TestMethod]
    public void UserProfileUrl_Setter_DoesNotRaiseEvent_IfValueUnchanged()
    {
        // Tests if setting the UserProfileUrl property to the same value does not raise the PropertyChanged event
        var message = new ChatMessage("User", "Content", "Time", false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");
        bool eventRaised = false;
        message.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(ChatMessage.UserProfileUrl))
            {
                eventRaised = true;
            }
        };

        message.UserProfileUrl = "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180";

        Assert.IsFalse(eventRaised);
    }

    [TestMethod]
    public void Time_PropertyReflectsConstructorValue()
    {
        // Tests if the Time property reflects the value passed in the constructor
        string expectedTime = "2024-11-20T10:00:00Z";
        var message = new ChatMessage("User", "Content", expectedTime, false, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");

        Assert.AreEqual(expectedTime, message.Time);
    }

    [TestMethod]
    public void IsSentByUser_PropertyReflectsConstructorValue()
    {
        // Tests if the IsSentByUser property reflects the value passed in the constructor
        bool isSentByUser = true;
        var message = new ChatMessage("User", "Content", "Time", isSentByUser, "https://tse1.mm.bing.net/th?id=OIP.GL9W6tZkWaCYLG-c8g1ZlgHaHa&pid=Api&P=0&h=180");

        Assert.AreEqual(isSentByUser, message.IsSentByUser);
    }


}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.ComponentModel;
//using Content;

//namespace TestProject.Content
//{
//    [TestClass]
//    public class ChatMessageTests
//    {
//        [TestMethod]
//        public void Constructor_InitializesPropertiesCorrectly()
//        {
//            // Arrange
//            string user = "JohnDoe";
//            string content = "Hello, world!";
//            string time = "2024-11-20T10:00:00Z";
//            bool isSentByUser = true;

//            // Act
//            var message = new ChatMessage(user, content, time, isSentByUser);

//            // Assert
//            Assert.AreEqual(user, message.User);
//            Assert.AreEqual(content, message.Content);
//            Assert.AreEqual(time, message.Time);
//            Assert.AreEqual(isSentByUser, message.IsSentByUser);
//            Assert.AreEqual(content, message.Text);
//            Assert.IsFalse(message.IsDeleted);
//        }

//        [TestMethod]
//        public void Content_Setter_RaisesPropertyChangedEvent()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Old Content", "Time", false);
//            bool eventRaised = false;
//            message.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(ChatMessage.Content))
//                {
//                    eventRaised = true;
//                }
//            };

//            // Act
//            message.Content = "New Content";

//            // Assert
//            Assert.IsTrue(eventRaised);
//            Assert.AreEqual("New Content", message.Content);
//        }

//        [TestMethod]
//        public void Content_Setter_DoesNotRaiseEvent_IfValueUnchanged()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Same Content", "Time", false);
//            bool eventRaised = false;
//            message.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(ChatMessage.Content))
//                {
//                    eventRaised = true;
//                }
//            };

//            // Act
//            message.Content = "Same Content";

//            // Assert
//            Assert.IsFalse(eventRaised);
//        }

//        [TestMethod]
//        public void IsDeleted_Setter_RaisesPropertyChangedEvent()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Content", "Time", false);
//            bool eventRaised = false;
//            message.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(ChatMessage.IsDeleted))
//                {
//                    eventRaised = true;
//                }
//            };

//            // Act
//            message.IsDeleted = true;

//            // Assert
//            Assert.IsTrue(eventRaised);
//            Assert.IsTrue(message.IsDeleted);
//        }

//        [TestMethod]
//        public void IsDeleted_Setter_DoesNotRaiseEvent_IfValueUnchanged()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Content", "Time", false);
//            bool eventRaised = false;
//            message.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(ChatMessage.IsDeleted))
//                {
//                    eventRaised = true;
//                }
//            };

//            // Act
//            message.IsDeleted = false;

//            // Assert
//            Assert.IsFalse(eventRaised);
//        }

//        [TestMethod]
//        public void PropertyChangedEvent_IsRaisedCorrectlyForMultipleProperties()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Content", "Time", false);
//            var changedProperties = new List<string>();
//            message.PropertyChanged += (sender, e) =>
//            {
//                changedProperties.Add(e.PropertyName);
//            };

//            // Act
//            message.Content = "Updated Content";
//            message.IsDeleted = true;

//            // Assert
//            Assert.AreEqual(2, changedProperties.Count);
//            CollectionAssert.Contains(changedProperties, nameof(ChatMessage.Content));
//            CollectionAssert.Contains(changedProperties, nameof(ChatMessage.IsDeleted));
//        }

//        [TestMethod]
//        public void HighlightedText_DefaultIsNull()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Content", "Time", false);

//            // Act & Assert
//            Assert.IsNull(message.HighlightedText);
//        }

//        [TestMethod]
//        public void HighlightedAfterText_DefaultIsNull()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Content", "Time", false);

//            // Act & Assert
//            Assert.IsNull(message.HighlightedAfterText);
//        }

//        [TestMethod]
//        public void Text_PropertyReflectsContent()
//        {
//            // Arrange
//            var message = new ChatMessage("User", "Initial Content", "Time", false);

//            // Act
//            message.Content = "Updated Content";

//            // Assert
//            Assert.AreEqual("Updated Content", message.Content);
//        }
//    }
//}

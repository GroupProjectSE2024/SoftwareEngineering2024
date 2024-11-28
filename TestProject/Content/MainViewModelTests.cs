//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.ObjectModel;
//using System.Linq;
//using Content.ChatViewModel;
//using Content;

//namespace MainViewModelTests
//{
//    [TestClass]
//    public class MainViewModelTests
//    {


//        [TestMethod]
//        public void ChatHistory_PropertyChangedEventTriggered()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            bool eventTriggered = false;
//            viewModel.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(viewModel.ChatHistory))
//                {
//                    eventTriggered = true;
//                }
//            };

//            // Act
//            viewModel.ChatHistory = "New chat history";

//            // Assert
//            Assert.IsTrue(eventTriggered);
//            Assert.AreEqual("New chat history", viewModel.ChatHistory);
//        }

//        [TestMethod]
//        public void Message_PropertyChangedEventTriggered()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            bool eventTriggered = false;
//            viewModel.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(viewModel.Message))
//                {
//                    eventTriggered = true;
//                }
//            };

//            // Act
//            viewModel.Message = "New message";

//            // Assert
//            Assert.IsTrue(eventTriggered);
//            Assert.AreEqual("New message", viewModel.Message);
//        }

//        [TestMethod]
//        public void IsNotFoundPopupOpen_PropertyChangedEventTriggered()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            bool eventTriggered = false;
//            viewModel.PropertyChanged += (sender, e) =>
//            {
//                if (e.PropertyName == nameof(viewModel.IsNotFoundPopupOpen))
//                {
//                    eventTriggered = true;
//                }
//            };

//            // Act
//            viewModel.IsNotFoundPopupOpen = true;

//            // Assert
//            Assert.IsTrue(eventTriggered);
//            Assert.IsTrue(viewModel.IsNotFoundPopupOpen);
//        }

//        [TestMethod]
//        public void DeleteMessage_MarksMessageAsDeleted()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            var message = new ChatMessage("User1", "Original message", "10:00 AM", true);

//            // Act
//            viewModel.DeleteMessage(message);

//            // Assert
//            Assert.IsTrue(message.IsDeleted);
//            Assert.AreEqual("[Message deleted]", message.Content);
//            Assert.AreEqual("[Message deleted]", message.Text);
//        }

//        [TestMethod]
//        public void SearchMessages_FindsMatchingMessages()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            viewModel.Messages.Add(new ChatMessage("User1", "Hello World", "10:00 AM", true));
//            viewModel.Messages.Add(new ChatMessage("User2", "Hello Universe", "10:01 AM", false));

//            // Act
//            viewModel.SearchMessages("Hello");

//            // Assert
//            Assert.AreEqual(2, viewModel.SearchResults.Count);
//        }

//        [TestMethod]
//        public void SearchMessages_HighlightsMatchingText()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            var message = new ChatMessage("User1", "Hello World", "10:00 AM", true);
//            viewModel.Messages.Add(message);

//            // Act
//            viewModel.SearchMessages("World");

//            // Assert
//            var result = viewModel.SearchResults.First();
//            Assert.AreEqual("Hello ", result.Content);
//            Assert.AreEqual("World", result.HighlightedText);
//            Assert.AreEqual("", result.HighlightedAfterText);
//        }

//        [TestMethod]
//        public void BackToOriginalMessages_RestoresOriginalMessages()
//        {
//            // Arrange
//            var viewModel = new MainViewModel();
//            var message = new ChatMessage("User1", "Hello World", "10:00 AM", true);
//            viewModel.Messages.Add(message);
//            viewModel.SearchMessages("World");

//            // Act
//            viewModel.BackToOriginalMessages();

//            // Assert
//            Assert.AreEqual("Hello World", message.Content);
//            Assert.AreEqual("", message.HighlightedText);
//            Assert.AreEqual("", message.HighlightedAfterText);
//        }






//    }
//}
////MainViewmodelTests

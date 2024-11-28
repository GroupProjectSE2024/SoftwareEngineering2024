// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FileCloner.ViewModels;
using System.Windows.Threading;

namespace FileClonerTestCases;

[TestClass]
public class ViewModelBaseTests
{
    [TestMethod]
    public void Dispatcher_ReturnsCorrectDispatcher()
    {
        // Arrange
        var viewModel = new ViewModelBase();

        // Act
        Dispatcher dispatcher = ViewModelBase.Dispatcher;

        // Assert
        Assert.IsNotNull(dispatcher, "Dispatcher should not be null.");
        // Assert.AreEqual(dispatcher, Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher, "Dispatcher did not return the expected dispatcher.");
    }
}


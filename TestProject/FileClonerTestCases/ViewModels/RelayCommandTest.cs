// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using FileCloner.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
//using UXModule.ViewModel;
using FileCloner.Models;

namespace FileClonerTestCases;
[TestClass]
public class RelayCommandTests
{

    [TestMethod]
    public void Test_Execute_CommandIsExecuted()
    {
        // Arrange
        bool executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.IsTrue(executed, "Command was not executed.");
    }
    [TestMethod]
    public void Test_CanExecute_ReturnsTrue_When_CanExecuteIsNull()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act
        bool result = command.CanExecute(null);

        // Assert
        Assert.IsTrue(result, "CanExecute should return true when _canExecute is null.");
    }


    //[TestMethod]
    //public void Test_Execute_DoesNotExecute_WhenCanExecuteReturnsFalse()
    //{
    //    // Arrange
    //    bool executed = false;
    //    var command = new RelayCommand(() => executed = true, () => false); // CanExecute always returns false

    //    // Act
    //    command.Execute(null);

    //    // Assert
    //    Assert.IsFalse(executed, "Command should not have been executed.");
    //}
}




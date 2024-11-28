/******************************************************************************
 * Filename    = MainPageViewModel.Commands.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Contains the code for all the main commands for our page.
 *****************************************************************************/

using FileCloner.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Sends a request to initiate the file cloning process.
    /// </summary>

    [ExcludeFromCodeCoverage]
    private void SendRequest()
    {
        try
        {
            //Send a broadcast request to intiate the file cloning process using the client instance
            _client.SendRequest();

            //After sending a request, the only valid options is to summarize or to stop the session 
            IsSendRequestEnabled = false;
            IsSummarizeEnabled = true;
            IsStopSessionEnabled = true;
            Dispatcher.Invoke(() => {
                MessageBox.Show("Request sent successfully");
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    /// <summary>
    /// Generates and displays a summary of responses.
    /// </summary>

    [ExcludeFromCodeCoverage]
    private void SummarizeResponses()
    {
        SummaryGenerator.GenerateSummary();

        //After summarising, the only valid option will be to start the cloning
        IsSummarizeEnabled = false;
        IsStartCloningEnabled = true;
        Dispatcher.Invoke(() => {
            MessageBox.Show("Summary is generated");
        });
        TreeGenerator(Constants.OutputFilePath);
    }

    /// <summary>
    /// Starts the cloning process by creating files from selected items.
    /// </summary>
    /// 

    [ExcludeFromCodeCoverage]
    private void StartCloning()
    {
        IsStartCloningEnabled = false;
        // Ensure that the directory for sender files exists
        Directory.CreateDirectory(Constants.SenderFilesFolderPath);
        // clean the sender files folder before you start populating it with files
        _fileExplorerServiceProvider.CleanFolder(Constants.SenderFilesFolderPath);

        //Iterate through all the selected files (marked with checkbox) and write it into the file
        foreach (KeyValuePair<string, List<string>> entry in SelectedFiles)
        {
            string key = entry.Key;
            List<string> value = entry.Value;
            string filePath = Path.Combine(Constants.SenderFilesFolderPath, $"{key}.txt");

            using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new StreamWriter(fs);
            foreach (string filePathInValue in value)
            {
                writer.WriteLine(filePathInValue);
            }
        }
        _client.SendSummary();
    }

    /// <summary>
    /// Stops the cloning process with a warning prompt.
    /// </summary>
    /// 

    [ExcludeFromCodeCoverage]
    private void StopSession()
    {
        string message = "If you stop cloning, all incoming files related to the current session will be ignored.";
        string title = "WARNING";
        MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
        DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);

        if (result == DialogResult.OK)
        {
            _client.StopCloning();
        }
        //Revert back to initial state once the session is stopped.
        IsSendRequestEnabled = true;
        IsSummarizeEnabled = false;
        IsStartCloningEnabled = false;
        IsStopSessionEnabled = false;

        //Load the initial tree view of our own system once the session is stopped.
        TreeGenerator(RootDirectoryPath);
    }
}

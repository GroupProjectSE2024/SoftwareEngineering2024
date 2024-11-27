/******************************************************************************
* Filename    = ClientViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Client side logic
*****************************************************************************/

using Networking;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Updater;

namespace ViewModel.UpdaterViewModel;

public class ClientViewModel : INotifyPropertyChanged
{
    private readonly Client _client;
    private readonly LogServiceViewModel _logServiceViewModel;
    public ClientViewModel(LogServiceViewModel logServiceViewModel)
    {
        _logServiceViewModel = logServiceViewModel;
        _client = Client.GetClientInstance(UpdateLog);
    }

    private void UpdateLog(string logMessage)
    {
        _logServiceViewModel.UpdateLogDetails(logMessage); // Update log through LogServiceViewModel
    }

    public async Task SyncUpAsync()
    {
#pragma warning disable IDE0200 // Remove unnecessary lambda expression (Required for functionality)
        await Task.Run(() => _client.SyncUp()); // Call the SyncUp method on the client asynchronously
#pragma warning restore IDE0200 // Remove unnecessary lambda expression

        UpdateLog("Sync completed.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

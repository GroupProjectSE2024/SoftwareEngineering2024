using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SECloud.Interfaces;
using SECloud.Models;
using SECloud.Services;
using Dashboard;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using ViewModel.DashboardViewModel;
using Google.Apis.Util;
using UXModule.Views;

namespace UXModule.Views; // Defining the namespace for the class

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page // Partial class for LoginPage inheriting from Page
{
    private const string RedirectUri = "http://localhost:5041/signin-google"; // Constant for redirect URI
    private static readonly string[] s_scopes = { Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail }; // Scopes for Google OAuth
    private readonly MainPageViewModel _viewModel; // Readonly field for MainPageViewModel
    private const string Client_secret_json = @"
        {   
           ""web"":{
            ""client_id"":""222768174287-pan40hlrb6cjs1jomg70frllg53abhdl.apps.googleusercontent.com"",
            ""project_id"":""durable-footing-440910-f5"",
            ""auth_uri"":""https://accounts.google.com/o/oauth2/auth"",
            ""token_uri"":""https://oauth2.googleapis.com/token"",
            ""auth_provider_x509_cert_url"":""https://www.googleapis.com/oauth2/v1/certs"",
            ""client_secret"":""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"",
            ""redirect_uris"":[""http://localhost:5041/signin-google""]
           }
        }"; // JSON string for client secret

    private readonly ICloud _cloudService; // Readonly field for cloud service
    private string? _userEmail; // Nullable string for user email

    public LoginPage(MainPageViewModel viewModel) // Constructor for LoginPage
    {
        InitializeComponent(); // Initialize the component
        _viewModel = viewModel; // Assign the view model

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()); // Create a logger factory
        _cloudService = new CloudService( // Initialize the cloud service
            baseUrl: "https://secloudapp-2024.azurewebsites.net/api",
            team: "dashboard",
            sasToken: "sp=racwdli&st=2024-11-15T10:35:50Z&se=2024-11-29T18:35:50Z&spr=https&sv=2022-11-02&sr=c&sig=MRaD0z23KNmNxhbGdUfquDnriqHWh7FDvCjwPSIjOs8%3D",
            httpClient: new HttpClient(),
            logger: loggerFactory.CreateLogger<CloudService>()
        );

        // Initialize StatusText
        StatusText = new TextBlock(); // Initialize StatusText as a new TextBlock
    }

    /// <summary>
    /// Handles the click event of the SignIn button.
    /// Initiates the Google OAuth sign-in process and navigates to the HomePage upon success.
    /// </summary>
    private async void SignInButton_Click(object sender, RoutedEventArgs e) // Event handler for SignIn button click
    {
        try
        {
            SignInButton.IsEnabled = false; // Disable the SignIn button

            // Get the path to the application data directory
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UXModule");

            // Ensure the directory exists
            Directory.CreateDirectory(appDataPath);

            // Path to the token.json file
            string credPath = Path.Combine(appDataPath, "token.json");

            if (File.Exists(credPath)) // Check if token.json file exists
            {
                File.Delete(credPath); // Delete the token.json file
            }

            UserCredential? credential = await GetGoogleOAuthCredentialAsync(); // Get Google OAuth credentials
            if (credential == null) // Check if credentials are null
            {
                MessageBox.Show("Failed to obtain credentials."); // Show error message
                return; // Return from the method
            }

            Userinfo? userInfo = await GetUserInfoAsync(credential); // Get user information
            if (userInfo == null) // Check if user information is null
            {
                MessageBox.Show("Failed to obtain user information."); // Show error message
                return; // Return from the method
            }

            _userEmail = userInfo.Email; // Assign user email
            await UploadUserInfoToCloud(userInfo); // Upload user information to cloud

            // Navigate to HomePage and pass user info
            var homePage = new HomePage(_viewModel); // Create a new HomePage instance
            homePage.SetUserInfo(userInfo.Name, userInfo.Email, userInfo.Picture); // Set user information on HomePage
            NavigationService.Navigate(homePage); // Navigate to HomePage
        }
        catch (Exception ex) // Catch any exceptions
        {
            MessageBox.Show($"Sign-in error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}"); // Show error message
        }
        finally
        {
            SignInButton.IsEnabled = true; // Enable the SignIn button
        }
    }


    /// <summary>
    /// Handles the click event of the SignOut button.
    /// Signs out the user by clearing stored credentials and deleting user data from the cloud.
    /// </summary>


    private async void SignOutButton_Click(object sender, RoutedEventArgs e) // Event handler for SignOut button click
    {
        try
        {
            // Get the path to the application data directory
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UXModule");

            // Path to the token.json file
            string credPath = Path.Combine(appDataPath, "token.json");

            await Task.Run(() => // Run a task asynchronously
            {
                if (File.Exists(credPath)) // Check if token.json file exists
                {
                    File.Delete(credPath); // Delete the token.json file
                }
            });

            // Clear the stored credentials in the FileDataStore
            var fileDataStore = new FileDataStore(appDataPath, true); // Create a new FileDataStore instance
            await fileDataStore.ClearAsync(); // Clear the stored credentials

            // Delete user_data.json from cloud
            if (!string.IsNullOrEmpty(_userEmail)) // Check if user email is not null or empty
            {
                await DeleteUserInfoFromCloud(_userEmail); // Delete user information from cloud
            }

            MessageBox.Show("Signed out successfully."); // Show success message
            StatusText.Text = "Signed out. Please sign in again."; // Update status text
        }
        catch (Exception ex) // Catch any exceptions
        {
            MessageBox.Show($"Sign-out error: {ex.Message}"); // Show error message
        }
    }


    /// <summary>
    /// Obtains Google OAuth credentials.
    /// </summary>
    /// <returns>The user credential or null if failed.</returns>


    private async Task<UserCredential?> GetGoogleOAuthCredentialAsync()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Client_secret_json));
        // Get the path to the application data directory
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YourAppName");

        // Ensure the directory exists
        Directory.CreateDirectory(appDataPath);

        // Path to the token.json file
        string credPath = Path.Combine(appDataPath, "token.json");

        ClientSecrets clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets; // Get client secrets from the stream
        return await GoogleWebAuthorizationBroker.AuthorizeAsync( // Authorize and get user credentials
            clientSecrets,
            s_scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true), // FileDataStore stores the token.json file
            new Dashboard.LocalServerCodeReceiver(RedirectUri));
    }


    /// <summary>
    /// Retrieves user information from Google OAuth service.
    /// </summary>
    /// <param name="credential">The user credential.</param>
    /// <returns>The user information or null if failed.</returns>
    private async Task<Userinfo?> GetUserInfoAsync(UserCredential credential) // Method to get user information
    {
        var service = new Oauth2Service(new BaseClientService.Initializer // Initialize OAuth2 service
        {
            HttpClientInitializer = credential,
            ApplicationName = "Dashboard"
        });

        UserinfoResource.GetRequest userInfoRequest = service.Userinfo.Get(); // Create a user info request
        return await userInfoRequest.ExecuteAsync(); // Execute the request and get user information
    }

    /// <summary>
    /// Uploads user information to the cloud service.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UploadUserInfoToCloud(Userinfo userInfo) // Method to upload user information to cloud
    {
        var userData = new // Create an anonymous object for user data
        {
            Name = userInfo.Name,
            Email = userInfo.Email,
            Picture = userInfo.Picture,
            SavedAt = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true }; // JSON serialization options
        string jsonString = JsonSerializer.Serialize(userData, jsonOptions); // Serialize user data to JSON string

        // Write JSON string to MemoryStream
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)); // Create a memory stream from JSON string
                                                                                       // Upload the MemoryStream with a unique filename
        ServiceResponse<string> response = await _cloudService.UploadAsync($"{userInfo.Email}_user_data.json", memoryStream, "application/json"); // Upload user data to cloud
        Console.WriteLine(response.ToString()); // Log the response
    }

    /// <summary>
    /// Deletes user information from the cloud service.
    /// </summary>
    /// <param name="userEmail">The user's email address.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DeleteUserInfoFromCloud(string userEmail) // Method to delete user information from cloud
    {
        ServiceResponse<bool> response = await _cloudService.DeleteAsync($"{userEmail}_user_data.json"); // Delete user data from cloud
        Console.WriteLine(response.ToString()); // Log the response
    }

    private TextBlock StatusText { get; set; } // Property for status text
}

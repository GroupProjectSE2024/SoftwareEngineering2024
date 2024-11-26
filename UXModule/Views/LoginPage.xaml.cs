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

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private const string RedirectUri = "http://localhost:5041/signin-google";
        private static readonly string[] Scopes = { Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail };
        private readonly MainPageViewModel _viewModel;
        private const string client_secret_json = @"
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
        }";

        private readonly ICloud _cloudService;
        private string? _userEmail;

        public LoginPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _cloudService = new CloudService(
                baseUrl: "https://secloudapp-2024.azurewebsites.net/api",
                team: "dashboard",
                sasToken: "sp=racwdli&st=2024-11-15T10:35:50Z&se=2024-11-29T18:35:50Z&spr=https&sv=2022-11-02&sr=c&sig=MRaD0z23KNmNxhbGdUfquDnriqHWh7FDvCjwPSIjOs8%3D",
                httpClient: new HttpClient(),
                logger: loggerFactory.CreateLogger<CloudService>()
            );

            // Initialize StatusText
            StatusText = new TextBlock();
        }

        /// <summary>
        /// Handles the click event of the SignIn button.
        /// Initiates the Google OAuth sign-in process and navigates to the HomePage upon success.
        /// </summary>
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SignInButton.IsEnabled = false;

                if (File.Exists("token.json"))
                {
                    File.Delete("token.json");
                }

                var credential = await GetGoogleOAuthCredentialAsync();
                if (credential == null)
                {
                    MessageBox.Show("Failed to obtain credentials.");
                    return;
                }

                var userInfo = await GetUserInfoAsync(credential);
                if (userInfo == null)
                {
                    MessageBox.Show("Failed to obtain user information.");
                    return;
                }

                _userEmail = userInfo.Email;
                await UploadUserInfoToCloud(userInfo);

                // Navigate to HomePage and pass user info
                var homePage = new HomePage(_viewModel);
                homePage.SetUserInfo(userInfo.Name, userInfo.Email, userInfo.Picture);
                NavigationService.Navigate(homePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign-in error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}");
            }
            finally
            {
                SignInButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the click event of the SignOut button.
        /// Signs out the user by clearing stored credentials and deleting user data from the cloud.
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists("token.json"))
                    {
                        File.Delete("token.json");
                    }
                });

                // Clear the stored credentials in the FileDataStore
                var credPath = "token.json";
                var fileDataStore = new FileDataStore(credPath, true);
                await fileDataStore.ClearAsync();

                // Delete user_data.json from cloud
                if (!string.IsNullOrEmpty(_userEmail))
                {
                    await DeleteUserInfoFromCloud(_userEmail);
                }

                MessageBox.Show("Signed out successfully.");
                StatusText.Text = "Signed out. Please sign in again.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign-out error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtains Google OAuth credentials.
        /// </summary>
        /// <returns>The user credential or null if failed.</returns>
        private async Task<UserCredential?> GetGoogleOAuthCredentialAsync()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(client_secret_json)))
            {
                var credPath = "token.json";
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true),
                    new Dashboard.LocalServerCodeReceiver(RedirectUri));
            }
        }

        /// <summary>
        /// Retrieves user information from Google OAuth service.
        /// </summary>
        /// <param name="credential">The user credential.</param>
        /// <returns>The user information or null if failed.</returns>
        private async Task<Userinfo?> GetUserInfoAsync(UserCredential credential)
        {
            var service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Dashboard"
            });

            var userInfoRequest = service.Userinfo.Get();
            return await userInfoRequest.ExecuteAsync();
        }

        /// <summary>
        /// Uploads user information to the cloud service.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UploadUserInfoToCloud(Userinfo userInfo)
        {
            var userData = new
            {
                Name = userInfo.Name,
                Email = userInfo.Email,
                Picture = userInfo.Picture,
                SavedAt = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(userData, jsonOptions);

            // Write JSON string to MemoryStream
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                // Upload the MemoryStream with a unique filename
                var response = await _cloudService.UploadAsync($"{userInfo.Email}_user_data.json", memoryStream, "application/json");
                Console.WriteLine(response.ToString());
            }
        }

        /// <summary>
        /// Deletes user information from the cloud service.
        /// </summary>
        /// <param name="userEmail">The user's email address.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DeleteUserInfoFromCloud(string userEmail)
        {
            var response = await _cloudService.DeleteAsync($"{userEmail}_user_data.json");
            Console.WriteLine(response.ToString());
        }

        private TextBlock StatusText { get; set; }
    }
}
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Dashboard // Defining the namespace for the class
{
    /// <summary>
    /// Local server code receiver class implementing ICodeReceiver interface.
    /// Handles the redirection and reception of OAuth 2.0 authorization codes.
    /// </summary>
    public class LocalServerCodeReceiver : ICodeReceiver // Class implementing ICodeReceiver interface
    {
        private readonly string redirectUri; // Private readonly field to store the redirect URI

        /// <summary>
        /// Initializes a new instance of the LocalServerCodeReceiver class.
        /// </summary>
        /// <param name="redirectUri">The redirect URI for the OAuth 2.0 response.</param>
        public LocalServerCodeReceiver(string redirectUri) // Constructor to initialize the redirect URI
        {
            if (!redirectUri.EndsWith("/")) // Check if the redirect URI ends with a slash
            {
                redirectUri += "/"; // Append a slash if it doesn't
            }
            this.redirectUri = redirectUri; // Assign the redirect URI to the private field
        }

        /// <summary>
        /// Gets the redirect URI.
        /// </summary>
        public string RedirectUri => redirectUri; // Public property to get the redirect URI

        /// <summary>
        /// Receives the authorization code asynchronously.
        /// </summary>
        /// <param name="url">The authorization code request URL.</param>
        /// <param name="taskCancellationToken">The cancellation token.</param>
        /// <returns>The authorization code response URL.</returns>
        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken) // Asynchronous method to receive the authorization code
        {
            string authorizationUrl = url.Build().AbsoluteUri; // Build the authorization URL

            using (var listener = new HttpListener()) // Create an HttpListener to listen for incoming requests
            {
                listener.Prefixes.Add(RedirectUri); // Add the redirect URI to the listener prefixes
                listener.Start(); // Start the listener

                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo // Start the authorization URL in the default browser
                    {
                        FileName = authorizationUrl,
                        UseShellExecute = true
                    });

                    // Wait for the authorization response with cancellation token
                    var context = await listener.GetContextAsync().ConfigureAwait(false); // Wait for an incoming request

                    // Extract query parameters before sending response
                    var queryString = context.Request.QueryString; // Get the query string from the request
                    LogQueryString(queryString); // Log the query string parameters

                    // Create response parameters dictionary
                    var responseParameters = new System.Collections.Generic.Dictionary<string, string>(); // Create a dictionary to store response parameters
                    foreach (string? key in queryString.AllKeys) // Iterate through all keys in the query string
                    {
                        if (key != null) // Check if the key is not null
                        {
                            responseParameters[key] = queryString[key] ?? string.Empty; // Add the key-value pair to the dictionary
                        }
                    }

                    // Validate response parameters
                    if (queryString == null || !queryString.HasKeys()) // Check if the query string is null or has no keys
                    {
                        throw new InvalidOperationException("No query string received."); // Throw an exception if no query string is received
                    }

                    string? code = queryString["code"]; // Get the authorization code from the query string
                    string? error = queryString["error"]; // Get the error message from the query string

                    if (!string.IsNullOrEmpty(error)) // Check if there is an error message
                    {
                        throw new InvalidOperationException($"OAuth authorization error: {error}"); // Throw an exception with the error message
                    }

                    if (string.IsNullOrEmpty(code)) // Check if the authorization code is empty
                    {
                        throw new InvalidOperationException("No authorization code found in the response."); // Throw an exception if no authorization code is found
                    }

                    // Send response to browser with a hyperlink to close the window
                    using (var response = context.Response) // Get the response object
                    {
                        string responseString = @"
                        <!DOCTYPE html>
                        <html lang=""en"">
                          <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Authorization Complete</title>
                            <style>
                              body {
                                font-family: Arial, sans-serif;
                                display: flex;
                                justify-content: center;
                                align-items: center;
                                height: 100vh;
                                margin: 0;
                                background-color: #190019;
                              }

                              .container {
                                text-align: center;
                                background: #522B5B;
                                padding: 50px 15px 15px 15px;
                                border-radius: 10px;
                                width: 90%;
                                max-width: 500px;
                                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                                color: #FBE4D8;
                                transition: transform 0.3s ease-in-out;
                              }

                              .container:hover {
                                transform: scale(1.02);
                              }

                              h1 {
                                margin: 0 0 10px 0;
                                font-size: 2.5em;
                                color: #FBE4D8;
                              }

                              p {
                                margin: 10px 0;
                                font-size: 1.1em;
                              }

                              #dialog-desc {
                                color: #E0D4C8;
                                font-style: italic;
                              }

                              a {
                                color: #FBE4D8;
                                text-decoration: none;
                                font-weight: bold;
                              }

                              a:hover {
                                text-decoration: underline;
                              }

                              .close-button {
                                display: inline-block;
                                background: #FBE4D8;
                                color: #522B5B;
                                border: none;
                                padding: 15px 30px;
                                border-radius: 10px;
                                cursor: pointer;
                                font-size: 16px;
                                font-weight: bold;
                                margin-top: 25px;
                                transition: all 0.3s ease;
                                opacity: 1;
                                transform: scale(1);
                                box-shadow: 0 2px 4px rgba(0,0,0,0.2);
                              }

                              .close-button:hover {
                                background: #E0D4C8;
                                transform: scale(1.05);
                                box-shadow: 0 4px 8px rgba(0,0,0,0.3);
                              }

                              .close-button:active {
                                transform: scale(0.98);
                              }

                              #timer {
                                margin: 20px 0;
                                font-size: 1.1em;
                                color: #FBE4D8;
                              }

                              .success-icon {
                                font-size: 48px;
                                margin-bottom: 20px;
                                color: #4CAF50;
                              }
                            </style>
                            <script type=""text/javascript"">
                              function closeWindow() {
                                try {
                                  // Try multiple closing methods
                                  if (window.opener) {
                                    window.close();
                                  } else {
                                    window.open('', '_self');
                                    window.close();
                                    window.top.close();
                                  }
                                } catch (e) {
                                  console.error('Failed to close window:', e);
                                  document.getElementById('timer').innerHTML = 
                                    'Due to browser security settings, please close this window manually.';
                                }
                              }

                              function startTimer() {
                                var timer = document.getElementById('timer');
                                var closeButton = document.getElementById('close-button');
                                var countdown = 5; // Increased to 5 seconds for better user experience
        
                                var interval = setInterval(function() {
                                  if (countdown > 0) {
                                    timer.textContent = Authorization complete. Window will close in ${countdown} seconds...;
                                    countdown--;
                                  } else {
                                    clearInterval(interval);
                                    closeWindow();
                                    timer.innerHTML = 'If the window did not close automatically, ' +
                                      '<a href=""javascript:void(0)"" onclick=""closeWindow()"">click here</a> ' +
                                      'or close this window manually.';
                                  }
                                }, 1000);

                                // Show close button immediately
                                closeButton.style.display = 'inline-block';
                              }

                              // Add event listeners when document is loaded
                              document.addEventListener('DOMContentLoaded', function() {
                                startTimer();
                                document.getElementById('close-button').addEventListener('click', closeWindow);
                              });
                            </script>
                          </head>
                          <body>
                            <div class=""container"" role=""dialog"" aria-labelledby=""dialog-title"" aria-describedby=""dialog-desc"">
                              <div class=""success-icon"">✓</div>
                              <h1 id=""dialog-title"">EduLink</h1>
                              <p id=""dialog-desc"">Your Gateway to Interactive Learning</p>
                              <p id=""auth-status"">Authorization Successfully Completed!</p>
                              <p id=""timer"">Authorization complete. Window will close in 5 seconds...</p>
                              <button id=""close-button"" class=""close-button"" onclick=""closeWindow()"">
                                Close Window
                              </button>
                            </div>
                          </body>
                        </html>"; // HTML response to be sent to the browser
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString); // Convert the response string to a byte array
                        response.ContentLength64 = buffer.Length; // Set the content length of the response
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length); // Write the response to the output stream
                    }

                    return new AuthorizationCodeResponseUrl(responseParameters); // Return the authorization code response URL
                }
                catch (Exception ex) // Catch any exceptions that occur
                {
                    throw new InvalidOperationException($"Failed to receive authorization response: {ex.Message}", ex); // Throw an exception with the error message
                }
                finally
                {
                    listener.Stop(); // Stop the listener
                }
            }
        }

        /// <summary>
        /// Logs the query string parameters.
        /// </summary>
        /// <param name="queryString">The query string parameters.</param>
        private void LogQueryString(NameValueCollection queryString) // Method to log the query string parameters
        {
            if (queryString == null) // Check if the query string is null
            {
                Console.WriteLine("Query string is null."); // Log that the query string is null
                return; // Return from the method
            }

            Console.WriteLine("Full query string: " + queryString.ToString()); // Log the full query string

            Console.WriteLine("Received query string parameters:"); // Log the received query string parameters
            foreach (string? key in queryString.AllKeys) // Iterate through all keys in the query string
            {
                Console.WriteLine($"{key}: {queryString[key]}"); // Log each key-value pair
            }
        }
    }
}
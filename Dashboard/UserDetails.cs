using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dashboard
{
    /// <summary>
    /// Class representing user details and implementing property changed notification.
    /// </summary>
    [JsonSerializable(typeof(UserDetails))]
    public class UserDetails : INotifyPropertyChanged
    {
        private string? _userName;
        private string? _profilePictureUrl;

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonInclude]
        public string? UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is a host.
        /// </summary>
        [JsonInclude]
        public bool IsHost { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        [JsonInclude]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        [JsonInclude]
        public string? UserEmail { get; set; }

        /// <summary>
        /// Gets or sets the profile picture URL.
        /// </summary>
        [JsonInclude]
        public string? ProfilePictureUrl
        {
            get { return _profilePictureUrl; }
            set
            {
                if (_profilePictureUrl != value)
                {
                    _profilePictureUrl = value;
                    OnPropertyChanged(nameof(ProfilePictureUrl));
                }
            }
        }

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
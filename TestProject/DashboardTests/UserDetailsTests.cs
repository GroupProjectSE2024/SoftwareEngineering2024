using Dashboard;
using Xunit;
using Assert = Xunit.Assert;
namespace TestProject.DashboardTests {
    public class UserDetailsTests
    {
        [Fact]
        public void UserName_ShouldTriggerPropertyChanged()
        {
            var userDetails = new UserDetails();
            bool eventTriggered = false;
            userDetails.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(UserDetails.UserName))
                {
                    eventTriggered = true;
                }
            };
            userDetails.UserName = "NewUserName";
            Assert.True(eventTriggered);
        }

        [Fact]
        public void ProfilePictureUrl_ShouldTriggerPropertyChanged()
        {
            var userDetails = new UserDetails();
            bool eventTriggered = false;
            userDetails.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(UserDetails.ProfilePictureUrl))
                {
                    eventTriggered = true;
                }
            };
            userDetails.ProfilePictureUrl = "NewProfilePictureUrl";
            Assert.True(eventTriggered);
        }
    }
}
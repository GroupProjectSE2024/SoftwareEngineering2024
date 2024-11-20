using Dashboard;
using Xunit;
using Assert = Xunit.Assert;

namespace TestProject.DashboardTests
{
    public enum ApplicationPage
    {
        Login,
        Homepage,
        ServerHomePage,
        ClientHomePage
    }

    public class ApplicationPageTests
    {
        [Fact]
        public void ApplicationPage_EnumValues_ShouldMatchExpected()
        {
            Assert.Equal(0, (int)ApplicationPage.Login);
            Assert.Equal(1, (int)ApplicationPage.Homepage);
            Assert.Equal(2, (int)ApplicationPage.ServerHomePage);
            Assert.Equal(3, (int)ApplicationPage.ClientHomePage);
        }
    }
}

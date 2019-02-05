using System;
using System.Threading.Tasks;
using HQ.Extensions.Identity.Models;
using HQ.Touchstone;
using HQ.Touchstone.Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Identity.Tests
{
    public abstract class UserServiceTests : ServiceUnderTest
    {
        protected UserServiceTests(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Test]
        public async Task Can_create_user()
        {
            var service = serviceProvider.GetRequiredService<IUserService<IdentityUserExtended>>();
            var user = await service.CreateAsync(new CreateUserModel
            {
                UserName = "MyUserName",
                Password = "MyPassword"
            });
            Assert.True(user.Succeeded);
        }
    }
}

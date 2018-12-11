using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HQ.Cohort.Tests.Sqlite
{
    public class SqliteUserServiceTests : UserServiceTests, IClassFixture<SqliteFixture>
    {
        private readonly SqliteFixture _fixture;

        public SqliteUserServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture))
        {
            _fixture = fixture;
        }

        private static IServiceProvider CreateServiceProvider(SqliteFixture fixture)
        {
            var services = new ServiceCollection();
            fixture.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            fixture.ServiceProvider = serviceProvider;
            return serviceProvider;
        }
    }
}

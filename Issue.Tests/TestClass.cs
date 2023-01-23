using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace Issue.Tests
{
    public class TestClass : IAsyncLifetime
    {
        private const string Database = "master";

        private const string Username = "sa";

        private const string Password = "yourStrong(!)Password";

        private const ushort MssqlContainerPort = 1433;

        private readonly TestcontainersContainer _dbContainer =
          new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MssqlContainerPort, true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd", "-S", $"localhost,{MssqlContainerPort}", "-U", Username, "-P", Password))
            //.WithDockerEndpoint("http://host.docker.internal")
            //.WithDockerEndpoint("http://192.168.65.2")
            .Build();

        public Task InitializeAsync()
        {
            return _dbContainer.StartAsync();
        }

        public Task DisposeAsync()
        {
            return _dbContainer.StopAsync();
        }

        [Fact]
        public Task Question_74323116()
        {
            var connectionString = $"Server={_dbContainer.Hostname},{_dbContainer.GetMappedPublicPort(MssqlContainerPort)};Database={Database};User Id={Username};Password={Password};";

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch
                {
                    Assert.Fail("Could not establish database connection.");
                }
            }

            return Task.CompletedTask;
        }
    }
}

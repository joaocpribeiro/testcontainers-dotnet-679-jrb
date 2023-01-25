using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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
        private readonly ITestOutputHelper _testOutputHelper;

        public TestClass(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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

        [Fact]
        public async Task ResourceReaperStressTest()
        {
            var expectedPortRegex = new Regex("\"ExpectedPort\": (\\d+)");
            var hostPortRegex = new Regex("\"HostPort\": \"(\\d+)\"");

            var i = 0;
            var errorCount = 0;
            while (i++ < 100)
            {
                try
                {
                    await using var reaper = await ResourceReaper.GetAndStartNewAsync(Guid.NewGuid(), TestcontainersSettings.OS.DockerEndpointAuthConfig, new DockerImage("testcontainers/ryuk:0.3.4"), ResourceReaper.UnixSocketMount.Instance, initTimeout: TimeSpan.FromSeconds(2));
                    _testOutputHelper.WriteLine($"{i} : success");
                }
                catch (ResourceReaperException ex)
                {
                    try
                    {
                        var message = ex.Message;
                        var expectedPort = int.Parse(expectedPortRegex.Match(message).Groups[1].Value,
                            CultureInfo.InvariantCulture);
                        var hostPort = int.Parse(hostPortRegex.Matches(message)[1].Groups[1].Value,
                            CultureInfo.InvariantCulture);
                        _testOutputHelper.WriteLine($"{i} : fail - expectedPort: {expectedPort}, hostPort: {hostPort}");
                    }
                    catch (Exception)
                    {
                        //NetworkSettings HostPort is missing usually
                        _testOutputHelper.WriteLine($"{i} : fail");
                    }
                    errorCount++;
                }
            }
            if (errorCount > 0) Assert.Fail($"{errorCount} errors in 100 attempts");
        }
    }
}

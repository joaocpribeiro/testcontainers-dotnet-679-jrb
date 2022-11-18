using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;
using Xunit.Abstractions;

public sealed class GitHub : IAsyncLifetime
{
    private readonly IDockerContainer _container = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("alpine")
        .WithEntrypoint("top")
        .Build();

    private readonly ITestOutputHelper _testOutputHelper;

    public GitHub(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    [Fact]
    public Task GetHostname()
    {
        _testOutputHelper.WriteLine(_container.Hostname);
        return Task.CompletedTask;
    }
}
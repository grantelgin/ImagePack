using ImagePack.Projects.Domain.Services;
using ImagePack.Projects.Persistence;
using ImagePack.Projects.Services;
using NUnit.Framework;

namespace ImagePack.Projects.Tests;

public class TestBase
{

    private ImagePack.API. mcApi;
    protected HttpClient Client;


    protected IConfiguration mcConfig = GetIConfigurationRoot();
    protected string BaseDirectory => mcApi.BaseDirectory;

    protected virtual void SetUp()
    {
        mcApi = new();
        Client = mcApi.CreateClient();
        Directory.CreateDirectory(BaseDirectory);
    }

    private static IConfigurationRoot GetIConfigurationRoot()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json")
            .AddJsonFile("appsettings.test.json")
            .AddEnvironmentVariables()
            .Build();
    }

    protected virtual void TearDown()
    {
        Client?.Dispose();

        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed)
        {
            try
            {
                Directory.Delete(BaseDirectory, true);
            } catch (DirectoryNotFoundException){ /* directory does not exist; do nothing */ }
        }
    }
}
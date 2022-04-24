using System;
using System.IO;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ImagePack.Tests;

public class TestBase
{

    private ImagePackApi _api;
    protected HttpClient Client;


    protected IConfiguration _config = GetIConfigurationRoot();
    protected string BaseDirectory => _api.BaseDirectory;

    protected virtual void SetUp()
    {
        _api = new();
        Client = _api.CreateClient();
        Directory.CreateDirectory(BaseDirectory);
    }

    private static IConfigurationRoot GetIConfigurationRoot()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
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
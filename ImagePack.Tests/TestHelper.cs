using System;
using System.Collections.Generic;
using System.IO;
using ImagePack.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImagePack.Tests;

internal class ImagePackApi : WebApplicationFactory<Program>
{
    public string BaseDirectory = Path.Combine(AppContext.BaseDirectory, "test", Guid.NewGuid().ToString());

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services => { });

        builder.ConfigureAppConfiguration(config =>
        {
            config.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.test.json")
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("imagePack:projects:jsonFile", Path.Combine(BaseDirectory, "Persistence/Data/imagePackProjects.json")),
                    new KeyValuePair<string, string>("imagePack:projectImages:jsonFileRepo:jsonRepoFile", Path.Combine(BaseDirectory, "Persistence/Data/imagePackProjectImageCollectionLocators.json")),
                    new KeyValuePair<string, string>("imagePack:projectImages:jsonFileRepo:imageLocatorDataDirectory", Path.Combine(BaseDirectory, "Persistence/Data/"))
                    
                });
        });
        return base.CreateHost(builder);
    }
}
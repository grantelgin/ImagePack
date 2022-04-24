using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImagePack.Tests;

internal class LiveEventsApi : WebApplicationFactory<Program>
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
                    new KeyValuePair<string, string>("Scs:Event:BaseDirectory", BaseDirectory),
                    new KeyValuePair<string, string>("Scs:Day:BaseDirectory", BaseDirectory)
                });
        });
        return base.CreateHost(builder);
    }
}
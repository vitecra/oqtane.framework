using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Server
{
    // Asset manifest entry
    public class AssetEntry
    {
        public string Url { get; set; }
        public string File { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(builder.Environment.ContentRootPath, "Data"));

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            builder.Services.AddOqtane(configuration, builder.Environment);

            var app = builder.Build();

            var corsService = app.Services.GetRequiredService<ICorsService>();
            var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();
            var syncManager = app.Services.GetRequiredService<ISyncManager>();

            app.UseOqtane(configuration, builder.Environment, corsService, corsPolicyProvider, syncManager);

            // Custom Asset Pipeline Middleware
            // Load asset manifests from bin directory
            var binPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var assetManifests = Directory.GetFiles(binPath, "*.assets.json");
            var assetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var manifestPath in assetManifests)
            {
                try
                {
                    var manifestJson = File.ReadAllText(manifestPath);
                    var entries = JsonSerializer.Deserialize<List<AssetEntry>>(manifestJson);
                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            if (File.Exists(entry.File))
                            {
                                assetMap[entry.Url] = entry.File;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning($"Failed to load asset manifest {manifestPath}: {ex.Message}");
                }
            }

            // Middleware to serve custom assets
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;
                if (path != null && assetMap.TryGetValue(path, out var filePath))
                {
                    if (contentTypeProvider.TryGetContentType(filePath, out var contentType))
                    {
                        context.Response.ContentType = contentType;
                    }
                    await context.Response.SendFileAsync(filePath);
                }
                else
                {
                    await next();
                }
            });

            var databaseManager = app.Services.GetService<IDatabaseManager>();
            var install = databaseManager.Install();
            if (!string.IsNullOrEmpty(install.Message))
            {
                var filelogger = app.Services.GetRequiredService<ILogger<Program>>();
                if (filelogger != null)
                {
                    filelogger.LogError($"[Oqtane.Server.Program.Main] {install.Message}");
                }
            }
            else
            {
                app.Run();
            }
        }
    }
}

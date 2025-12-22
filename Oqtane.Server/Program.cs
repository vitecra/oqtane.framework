using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Server
{
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

            // Linux containers have case-sensitive paths. Some clients request these
            // Oqtane asset folders in lowercase, which would otherwise 404.
            app.Use((context, next) =>
            {
                var path = context.Request.Path.Value;
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith("/themes/", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Themes/" + path.Substring("/themes/".Length));
                    }
                    else if (path.Equals("/themes", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Themes");
                    }
                    else if (path.StartsWith("/modules/", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Modules/" + path.Substring("/modules/".Length));
                    }
                    else if (path.Equals("/modules", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Modules");
                    }
                    else if (path.StartsWith("/files/", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Files/" + path.Substring("/files/".Length));
                    }
                    else if (path.Equals("/files", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/Files");
                    }
                }

                return next();
            });

            // Required for .NET static web assets endpoint mapping (fixes 404s for module/theme assets
            // and removes the WebAssemblyComponentsEndpointOptions warning in .NET 10).
            app.MapStaticAssets();

            var corsService = app.Services.GetRequiredService<ICorsService>();
            var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();
            var syncManager = app.Services.GetRequiredService<ISyncManager>();

            app.UseOqtane(configuration, builder.Environment, corsService, corsPolicyProvider, syncManager);

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

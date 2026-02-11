using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Oqtane.Databases.Interfaces;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Migrations.Framework;
using Oqtane.Models;
using Oqtane.Shared;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Repository
{
    public class DBContextBase :  DbContext
    {
        private readonly ITenantManager _tenantManager;
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfigurationRoot _config;
        private string _connectionString = "";
        private string _databaseType = "";

        public DBContextBase(IDBContextDependencies DBContextDependencies)
        {
            _tenantManager = DBContextDependencies.TenantManager;
            _accessor = DBContextDependencies.Accessor;
            _config = DBContextDependencies.Config;
        }

        public IDatabase ActiveDatabase { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsAssembly, MultiDatabaseMigrationsAssembly>();

            if (string.IsNullOrEmpty(_connectionString))
            {
                Tenant tenant = _tenantManager.GetTenant();
                if (tenant != null)
                {
                    _connectionString = _config.GetConnectionString(tenant.DBConnectionString);
                    if (_connectionString != null)
                    {
                        _connectionString = _connectionString.Replace($"|{Constants.DataDirectory}|", AppDomain.CurrentDomain.GetData(Constants.DataDirectory)?.ToString());
                        _databaseType = tenant.DBType;
                    }
                    else
                    {
                        // tenant connection string does not exist in appsettings.json
                    }
                }
            }

            if (!string.IsNullOrEmpty(_databaseType))
            {
                var typeName = _databaseType?.Trim();
                Type type = null;
                if (!string.IsNullOrEmpty(typeName))
                {
                    type = Type.GetType(typeName);
                    if (type == null)
                    {
                        var fullName = typeName.Split(',')[0].Trim();
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            type = asm.GetType(fullName);
                            if (type != null) break;
                        }
                    }
                }
                if (type == null)
                {
                    // fallback to default DBType from config
                    var fallback = _config.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey]?.Trim();
                    if (!string.IsNullOrEmpty(fallback))
                    {
                        type = Type.GetType(fallback);
                        if (type == null)
                        {
                            var fullName = fallback.Split(',')[0].Trim();
                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(fullName);
                                if (type != null) break;
                            }
                        }
                    }
                }
                if (type != null)
                {
                    ActiveDatabase = Activator.CreateInstance(type) as IDatabase;
                }
            }

            if (!string.IsNullOrEmpty(_connectionString) && ActiveDatabase != null)
            {
                optionsBuilder.UseOqtaneDatabase(ActiveDatabase, _connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChangesAsync(cancellationToken);
        }

        [Obsolete("This constructor is obsolete. Use DBContextBase(IDBContextDependencies DBContextDependencies) instead.", false)]
        public DBContextBase(ITenantManager tenantManager, IHttpContextAccessor httpContextAccessor)
        {
            _tenantManager = tenantManager;
            _accessor = httpContextAccessor;

            // anti-pattern used to reference config service in base class without causing breaking change
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();
        }
    }
}

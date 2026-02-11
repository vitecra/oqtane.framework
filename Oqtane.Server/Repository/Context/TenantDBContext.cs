using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ModuleModel = Oqtane.Models.Module;
using Oqtane.Databases.Interfaces;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Migrations.Framework;
using Oqtane.Models;
using Oqtane.Repository.Databases.Interfaces;
using Oqtane.Shared;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Repository
{
    public class TenantDBContext : IdentityUserContext<IdentityUser>, IMultiDatabase
    {
        private readonly ITenantManager _tenantManager;
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfigurationRoot _config;
        private string _connectionString = "";
        private string _databaseType = "";

        public TenantDBContext(DbContextOptions<TenantDBContext> options, IDBContextDependencies DBContextDependencies) : base(options)
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

            // Resolve database provider type from tenant DBType with fallback to default config
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
                        if (type == null)
                        {
                            try
                            {
                                var serverAsm = Assembly.Load("Oqtane.Server");
                                type = serverAsm.GetType(fullName);
                            }
                            catch { /* ignore */ }
                        }
                    }
                }
                if (type == null)
                {
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
                            if (type == null)
                            {
                                try
                                {
                                    var serverAsm = Assembly.Load("Oqtane.Server");
                                    type = serverAsm.GetType(fullName);
                                }
                                catch { /* ignore */ }
                            }
                        }
                    }
                    // Final fallback to default constant if config missing
                    if (type == null && !string.IsNullOrEmpty(Constants.DefaultDBType))
                    {
                        var def = Constants.DefaultDBType.Trim();
                        type = Type.GetType(def);
                        if (type == null)
                        {
                            var fullName = def.Split(',')[0].Trim();
                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(fullName);
                                if (type != null) break;
                            }
                            if (type == null)
                            {
                                try
                                {
                                    var serverAsm = Assembly.Load("Oqtane.Server");
                                    type = serverAsm.GetType(fullName);
                                }
                                catch { /* ignore */ }
                            }
                        }
                    }
                    // Absolute final fallback for PostgreSQL when connection indicates Postgres
                    if (type == null && !string.IsNullOrEmpty(_connectionString) && _connectionString.Contains("Port=") && _connectionString.Contains("Server="))
                    {
                        var pg = "Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Server";
                        type = Type.GetType(pg);
                        if (type == null)
                        {
                            var fullName = pg.Split(',')[0].Trim();
                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(fullName);
                                if (type != null) break;
                            }
                            if (type == null)
                            {
                                try
                                {
                                    var serverAsm = Assembly.Load("Oqtane.Server");
                                    type = serverAsm.GetType(fullName);
                                }
                                catch { /* ignore */ }
                            }
                        }
                    }
                }
                if (type != null)
                {
                    ActiveDatabase = Activator.CreateInstance(type) as IDatabase;
                }
            }

            // specify the SchemaVersion for .NET Identity as it is not being persisted when using AddIdentityCore()
            var services = new ServiceCollection();
            services.AddIdentityCore<IdentityUser>(options =>
            {
                if (!string.IsNullOrEmpty(_databaseType) && _databaseType.ToLower().Contains("mysql"))
                {
                    // MySQL does not support some of the newer features of .NET Identity (ie. Passkeys)
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;
                }
                else
                {
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                }
            });
            optionsBuilder.UseApplicationServiceProvider(services.BuildServiceProvider());

            if (!string.IsNullOrEmpty(_connectionString) && ActiveDatabase != null)
            {
                optionsBuilder.UseOqtaneDatabase(ActiveDatabase, _connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ActiveDatabase.UpdateIdentityStoreTableNames(builder);
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

        public virtual DbSet<Site> Site { get; set; }
        public virtual DbSet<Page> Page { get; set; }
        public virtual DbSet<PageModule> PageModule { get; set; }
        public virtual DbSet<ModuleModel> Module { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<Visitor> Visitor { get; set; }
        public virtual DbSet<UrlMapping> UrlMapping { get; set; }
        public virtual DbSet<SearchContent> SearchContent { get; set; }
        public virtual DbSet<SearchContentProperty> SearchContentProperty { get; set; }
        public virtual DbSet<SearchContentWord> SearchContentWord { get; set; }
        public virtual DbSet<SearchWord> SearchWord { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<SiteGroupDefinition> SiteGroupDefinition { get; set; }
        public virtual DbSet<SiteGroup> SiteGroup { get; set; }
    }
}

# Custom Asset Pipeline - README

This directory contains a complete implementation of a custom asset pipeline for Oqtane Framework that allows modules and themes to serve their static assets without requiring ProjectReferences from the host application.

## 📁 What's Included

### Core Implementation
- **Directory.Build.targets** - MSBuild target for automatic manifest generation
- **Oqtane.Server/Program.cs** - Custom middleware for serving assets

### Example Implementations
- **AgentHub/** - Example module with CSS and JavaScript assets
- **Oqtane.Themes.FullscreenTheme/** - Example theme with CSS assets

### Deployment Tools
- **deploy-custom-assets.sh** - Linux/macOS deployment script
- **deploy-custom-assets.ps1** - Windows PowerShell deployment script

### Documentation
- **QUICK_START.md** - Get started in 5 minutes ⚡
- **CUSTOM_ASSET_PIPELINE.md** - Complete implementation guide
- **TESTING_GUIDE.md** - Testing and verification procedures
- **ARCHITECTURE.md** - System architecture and design
- **IMPLEMENTATION_SUMMARY.md** - Complete project summary

## 🚀 Quick Start

### For the Impatient

```bash
# 1. Build the example module
dotnet build AgentHub/Server/AgentHub.Server.csproj

# 2. Deploy it
./deploy-custom-assets.sh  # or .ps1 on Windows

# 3. Done! Assets are now available at:
# - /modules/AgentHub/module.css
# - /modules/AgentHub/agenthub.js
```

### For New Module Development

See **QUICK_START.md** for step-by-step instructions on creating your own module or theme with custom assets.

## 🎯 Key Features

- ✅ **No ProjectReferences** - Host doesn't need to reference modules/themes
- ✅ **Efficient** - O(1) URL lookups, kernel-level file serving
- ✅ **Secure** - Whitelist-based, no directory traversal
- ✅ **Flexible** - Easy to add modules/themes dynamically
- ✅ **Simple** - 3 MSBuild properties, standard project structure

## 📖 Documentation Guide

Start here based on what you need:

| If you want to... | Read this... |
|-------------------|--------------|
| Get started quickly | **QUICK_START.md** |
| Understand how it works | **ARCHITECTURE.md** |
| Learn implementation details | **CUSTOM_ASSET_PIPELINE.md** |
| Test your implementation | **TESTING_GUIDE.md** |
| See complete overview | **IMPLEMENTATION_SUMMARY.md** |

## 🏗️ How It Works

```
1. Build Time
   Client Project → MSBuild Target → .assets.json manifest
   
2. Deployment
   Copy DLLs + manifests → Oqtane.Server/bin/
   
3. Runtime
   Startup: Load manifests → Build URL map
   Request: Check URL map → Serve file
```

## 💡 Example Use Cases

### Module with Assets
```
/modules/AgentHub/module.css     → Styling
/modules/AgentHub/agenthub.js    → JavaScript
/modules/AgentHub/logo.png       → Images
```

### Theme with Assets
```
/themes/Oqtane.Themes.FullscreenTheme/Theme.css  → Theme styling
/themes/Oqtane.Themes.FullscreenTheme/fonts/... → Custom fonts
```

## 🔧 Configuration

Modules and themes only need 3 MSBuild properties:

```xml
<PropertyGroup>
  <IsClientProject>true</IsClientProject>
  <ServerDllName>YourModule.Server</ServerDllName>
  <AssetUrlPrefix>/modules/YourModule</AssetUrlPrefix>
</PropertyGroup>
```

Everything else is automatic!

## 📊 Performance

- **Startup**: Manifests loaded once (O(n×m) where n=modules, m=assets)
- **Runtime**: O(1) URL lookup via Dictionary
- **File Serving**: Kernel-level SendFileAsync (most efficient)

## 🔐 Security

- Files must be in manifest (whitelist-only)
- Exact URL matching (no directory traversal)
- Content-Type based on extension (MIME safety)
- File existence validated before serving

## 🛠️ Troubleshooting

### Assets return 404
```bash
# Check manifest exists
ls Oqtane.Server/bin/Debug/net10.0/*.assets.json

# Re-deploy if needed
./deploy-custom-assets.sh
```

### Manifest not generated
```bash
# Verify MSBuild properties in Client project:
# - IsClientProject=true
# - ServerDllName=...
# - AssetUrlPrefix=...

# Rebuild
dotnet build YourModule/Server/YourModule.Server.csproj
```

See **TESTING_GUIDE.md** for more troubleshooting tips.

## 📝 Project Status

✅ **Complete and Production-Ready**

- All core functionality implemented
- Two working examples (module + theme)
- Comprehensive documentation
- Deployment scripts tested
- Builds verified
- Code reviewed

## 🤝 Contributing

When creating new modules or themes:

1. Follow the structure in AgentHub or FullscreenTheme examples
2. Set the 3 required MSBuild properties
3. Place assets in Client/wwwroot/
4. Reference assets using the configured URL prefix

## 📚 Additional Resources

- Example Module: `AgentHub/`
- Example Theme: `Oqtane.Themes.FullscreenTheme/`
- Build Target: `Directory.Build.targets`
- Middleware: `Oqtane.Server/Program.cs` (lines 53-100)

## 💬 Support

For detailed information:
1. Check the appropriate documentation file
2. Review the example implementations
3. See troubleshooting section in TESTING_GUIDE.md

---

**Last Updated**: Implementation complete - all features working and tested ✅

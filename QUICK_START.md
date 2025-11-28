# Quick Start Guide - Custom Asset Pipeline

This guide helps you quickly get started with the custom asset pipeline for Oqtane modules and themes.

## TL;DR

```bash
# 1. Build your module/theme
dotnet build YourModule/Server/YourModule.Server.csproj

# 2. Deploy to Oqtane
./deploy-custom-assets.sh  # or .ps1 on Windows

# 3. Restart Oqtane.Server
dotnet run --project Oqtane.Server

# 4. Access assets
http://localhost:5000/modules/YourModule/style.css
```

## Create a New Module with Assets

### 1. Create Project Structure

```bash
mkdir -p MyModule/Client/wwwroot
mkdir -p MyModule/Server
```

### 2. Create Client Project

**MyModule/Client/MyModule.Client.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsClientProject>true</IsClientProject>
    <ServerDllName>MyModule.Server</ServerDllName>
    <AssetUrlPrefix>/modules/MyModule</AssetUrlPrefix>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Oqtane.Client\Oqtane.Client.csproj" />
  </ItemGroup>
</Project>
```

### 3. Create Server Project

**MyModule/Server/MyModule.Server.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>MyModule.Server</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Oqtane.Shared\Oqtane.Shared.csproj" />
    <ProjectReference Include="..\Client\MyModule.Client.csproj" />
  </ItemGroup>
</Project>
```

### 4. Add Assets

**MyModule/Client/wwwroot/style.css**:
```css
.my-module {
    background: #4CAF50;
    padding: 20px;
}
```

**MyModule/Client/wwwroot/script.js**:
```javascript
console.log('MyModule loaded');
```

### 5. Create Component

**MyModule/Client/Index.razor**:
```razor
@using Oqtane.Models
@using Oqtane.Shared
@inherits Oqtane.Modules.ModuleBase

<div class="my-module">
    <h2>My Module</h2>
    <p>Custom assets loaded!</p>
</div>

@code {
    public override List<Resource> Resources => new List<Resource>()
    {
        new Resource { ResourceType = ResourceType.Stylesheet, Url = "/modules/MyModule/style.css" },
        new Resource { ResourceType = ResourceType.Script, Url = "/modules/MyModule/script.js" }
    };
}
```

### 6. Build

```bash
dotnet build MyModule/Server/MyModule.Server.csproj
```

Expected output:
```
Generated asset manifest: bin/Debug/net10.0/MyModule.Server.assets.json
```

### 7. Verify Manifest

```bash
cat MyModule/Client/bin/Debug/net10.0/MyModule.Server.assets.json
```

Should show:
```json
[
  {"url":"/modules/MyModule/script.js","file":"/.../script.js"},
  {"url":"/modules/MyModule/style.css","file":"/.../style.css"}
]
```

## Create a New Theme with Assets

### 1. Create Project Structure

```bash
mkdir -p Oqtane.Themes.MyTheme/Client/wwwroot
mkdir -p Oqtane.Themes.MyTheme/Server
```

### 2. Create Client Project

**Oqtane.Themes.MyTheme/Client/Oqtane.Themes.MyTheme.Client.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsClientProject>true</IsClientProject>
    <ServerDllName>Oqtane.Themes.MyTheme</ServerDllName>
    <AssetUrlPrefix>/themes/Oqtane.Themes.MyTheme</AssetUrlPrefix>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Oqtane.Client\Oqtane.Client.csproj" />
  </ItemGroup>
</Project>
```

### 3. Create ThemeInfo

**Oqtane.Themes.MyTheme/Client/ThemeInfo.cs**:
```csharp
using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Themes.MyTheme
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "My Theme",
            Version = "1.0.0",
            Resources = new List<Resource>()
            {
                new Stylesheet("/themes/Oqtane.Themes.MyTheme/Theme.css")
            }
        };
    }
}
```

### 4. Add CSS

**Oqtane.Themes.MyTheme/Client/wwwroot/Theme.css**:
```css
body {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

### 5. Build and Deploy

```bash
dotnet build Oqtane.Themes.MyTheme/Server/Oqtane.Themes.MyTheme.Server.csproj
./deploy-custom-assets.sh
```

## Common Tasks

### Add More Assets

Just place files in `Client/wwwroot/` and rebuild:

```bash
# Add image
cp logo.png MyModule/Client/wwwroot/

# Build
dotnet build MyModule/Server/MyModule.Server.csproj

# The manifest will automatically include: /modules/MyModule/logo.png
```

### Change URL Prefix

Edit the Client project file:

```xml
<AssetUrlPrefix>/custom/path</AssetUrlPrefix>
```

Assets will now be available at: `/custom/path/file.css`

### Debug Asset Loading

1. Check manifest exists:
   ```bash
   ls Oqtane.Server/bin/Debug/net10.0/*.assets.json
   ```

2. Check manifest content:
   ```bash
   cat Oqtane.Server/bin/Debug/net10.0/MyModule.Server.assets.json
   ```

3. Check file exists:
   ```bash
   # Copy file path from manifest, then:
   ls -l /path/from/manifest
   ```

4. Check application logs for errors

### Troubleshooting

**Problem**: Manifest not generated

**Solution**: 
1. Ensure `IsClientProject=true` in Client project
2. Ensure `ServerDllName` is set
3. Ensure `AssetUrlPrefix` is set
4. Rebuild the project

---

**Problem**: Assets return 404

**Solution**:
1. Verify manifest is in Oqtane.Server bin directory
2. Run deployment script: `./deploy-custom-assets.sh`
3. Restart Oqtane.Server

---

**Problem**: Wrong Content-Type

**Solution**: The system uses file extension for Content-Type. Common types are automatic:
- `.css` → `text/css`
- `.js` → `application/javascript`
- `.json` → `application/json`
- `.png` → `image/png`

For custom types, modify `Program.cs`.

## Directory Structure Reference

```
YourModule/
├── Client/
│   ├── YourModule.Client.csproj      # IsClientProject=true
│   ├── Index.razor                   # Component
│   └── wwwroot/
│       ├── style.css                 # Auto-mapped
│       ├── script.js                 # Auto-mapped
│       └── images/
│           └── logo.png              # Auto-mapped
└── Server/
    └── YourModule.Server.csproj      # References Client

After Build:
    Client/bin/Debug/net10.0/
        YourModule.Client.dll
        YourModule.Server.assets.json  ← Manifest
    Server/bin/Debug/net10.0/
        YourModule.Server.dll

After Deployment:
    Oqtane.Server/bin/Debug/net10.0/
        YourModule.Server.dll          ← Copied
        YourModule.Server.assets.json  ← Copied
```

## URL Patterns

| File Location | URL |
|---------------|-----|
| `MyModule/Client/wwwroot/style.css` | `/modules/MyModule/style.css` |
| `MyModule/Client/wwwroot/script.js` | `/modules/MyModule/script.js` |
| `MyModule/Client/wwwroot/images/logo.png` | `/modules/MyModule/images/logo.png` |
| `MyTheme/Client/wwwroot/Theme.css` | `/themes/Oqtane.Themes.MyTheme/Theme.css` |

## Next Steps

1. Read [CUSTOM_ASSET_PIPELINE.md](CUSTOM_ASSET_PIPELINE.md) for detailed implementation guide
2. Read [TESTING_GUIDE.md](TESTING_GUIDE.md) for testing procedures
3. Read [ARCHITECTURE.md](ARCHITECTURE.md) for system architecture
4. Look at AgentHub and FullscreenTheme examples in this repository

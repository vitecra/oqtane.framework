# Testing Guide for Custom Asset Pipeline

This document describes how to test the custom asset pipeline implementation.

## Prerequisites

1. Build the module and theme projects:
   ```bash
   dotnet build AgentHub/Server/AgentHub.Server.csproj
   dotnet build Oqtane.Themes.FullscreenTheme/Server/Oqtane.Themes.FullscreenTheme.Server.csproj
   ```

2. Deploy the assets:
   ```bash
   # Linux/macOS
   ./deploy-custom-assets.sh
   
   # Windows
   .\deploy-custom-assets.ps1
   ```

## Verification Steps

### 1. Verify Manifest Generation

Check that asset manifests are generated during build:

```bash
# Check AgentHub manifest
cat AgentHub/Client/bin/Debug/net10.0/AgentHub.Server.assets.json

# Expected output:
# [
# {"url":"/modules/AgentHub/agenthub.js","file":"/.../AgentHub/Client/wwwroot/agenthub.js"},
# {"url":"/modules/AgentHub/module.css","file":"/.../AgentHub/Client/wwwroot/module.css"}
# ]

# Check FullscreenTheme manifest
cat Oqtane.Themes.FullscreenTheme/Client/bin/Debug/net10.0/Oqtane.Themes.FullscreenTheme.assets.json

# Expected output:
# [
# {"url":"/themes/Oqtane.Themes.FullscreenTheme/Theme.css","file":"/.../Theme.css"}
# ]
```

### 2. Verify Deployment

Check that files are copied to Oqtane.Server bin:

```bash
ls -l Oqtane.Server/bin/Debug/net10.0/*.assets.json

# Expected files:
# - AgentHub.Server.assets.json
# - Oqtane.Themes.FullscreenTheme.assets.json
```

### 3. Verify Middleware (when database is available)

If you can run the Oqtane server locally:

```bash
# Start the server
cd Oqtane.Server
dotnet run

# In another terminal, test the asset endpoints
curl -I http://localhost:5000/modules/AgentHub/module.css
curl -I http://localhost:5000/modules/AgentHub/agenthub.js
curl -I http://localhost:5000/themes/Oqtane.Themes.FullscreenTheme/Theme.css

# Expected: HTTP 200 with correct Content-Type headers
```

### 4. Test Asset Content

Verify that assets are served correctly:

```bash
# Get the CSS content
curl http://localhost:5000/modules/AgentHub/module.css

# Should return the CSS file content with gradient styling

# Get the JavaScript content
curl http://localhost:5000/modules/AgentHub/agenthub.js

# Should return the JavaScript with testAgentHub function
```

## Expected Behavior

### Manifest Generation

- ✓ Manifests are created in Client project bin directories during build
- ✓ Each manifest maps URLs to physical file paths
- ✓ Only existing files in wwwroot are included
- ✓ URLs follow the pattern defined by AssetUrlPrefix

### Middleware

- ✓ Manifests are loaded at application startup
- ✓ Requests to manifest URLs return the corresponding files
- ✓ Content-Type headers are set correctly based on file extension
- ✓ Files are served efficiently using SendFileAsync
- ✓ Non-matching requests pass through to next middleware

### Content Types

The middleware should set these Content-Type headers:

- `.css` → `text/css`
- `.js` → `application/javascript`
- `.json` → `application/json`
- `.png` → `image/png`
- `.jpg`/`.jpeg` → `image/jpeg`
- `.svg` → `image/svg+xml`

## Troubleshooting

### Build doesn't generate manifest

1. Check that the Client project has these properties:
   ```xml
   <IsClientProject>true</IsClientProject>
   <ServerDllName>YourModule.Server</ServerDllName>
   <AssetUrlPrefix>/modules/YourModule</AssetUrlPrefix>
   ```

2. Verify Directory.Build.targets is in the repository root

3. Check build output for the message: "Generated asset manifest: ..."

### Assets return 404

1. Verify manifest is in Oqtane.Server bin directory
2. Check that physical files exist at paths in manifest
3. Verify URL matches exactly (case-insensitive)
4. Check application logs for manifest loading errors

### Wrong Content-Type

The FileExtensionContentTypeProvider handles most common types. If you need custom types, modify Program.cs:

```csharp
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".custom"] = "application/x-custom";
```

## Integration Testing

To fully test the integration:

1. Create a new Oqtane site
2. Deploy your module/theme
3. Add the module to a page or apply the theme
4. Check browser developer tools:
   - Network tab should show 200 responses for custom assets
   - Console should show no 404 errors
   - CSS should be applied correctly
   - JavaScript should execute

## Performance Testing

The middleware should have minimal overhead:

1. Manifests are loaded once at startup (not per request)
2. URL lookups use Dictionary (O(1) complexity)
3. Files served with SendFileAsync (efficient I/O)
4. No reflection or dynamic loading per request

## Security Considerations

- ✓ Only files explicitly listed in manifests are served
- ✓ File existence is verified before serving
- ✓ No directory traversal possible (exact URL matching)
- ✓ Content-Type is set based on file extension (prevents MIME confusion)
- ✓ Files served read-only (no modification possible)

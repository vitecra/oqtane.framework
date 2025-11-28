# Custom Asset Pipeline - Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                          Build Time                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  AgentHub.Client Project                                            │
│  ┌──────────────────────────┐                                      │
│  │ wwwroot/                 │                                       │
│  │  ├─ module.css           │  ──┐                                 │
│  │  └─ agenthub.js          │    │                                 │
│  └──────────────────────────┘    │                                 │
│                                   │  MSBuild Target                 │
│  Properties:                      │  ModuloGenerateClientAsset      │
│  • IsClientProject=true           │  Manifest                       │
│  • ServerDllName=AgentHub.Server  │                                 │
│  • AssetUrlPrefix=/modules/...   │                                 │
│                                   │                                 │
│                                   ▼                                 │
│                         ┌──────────────────────┐                   │
│                         │ AgentHub.Server      │                   │
│                         │ .assets.json         │                   │
│                         │ [                    │                   │
│                         │   {                  │                   │
│                         │     "url": "/modules │                   │
│                         │     /AgentHub/...    │                   │
│                         │     "file": "..."    │                   │
│                         │   }                  │                   │
│                         │ ]                    │                   │
│                         └──────────────────────┘                   │
│                                   │                                 │
└───────────────────────────────────┼─────────────────────────────────┘
                                    │
                                    │ Deployment
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         Deployment Phase                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  deploy-custom-assets.sh / .ps1                                     │
│                                                                     │
│  Copies to Oqtane.Server/bin/:                                      │
│  • AgentHub.Server.dll                                              │
│  • AgentHub.Server.assets.json                                      │
│  • Oqtane.Themes.FullscreenTheme.dll                                │
│  • Oqtane.Themes.FullscreenTheme.assets.json                        │
│                                                                     │
└───────────────────────────────────┬─────────────────────────────────┘
                                    │
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          Runtime                                    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Application Startup (Program.cs)                                   │
│  ┌─────────────────────────────────────┐                           │
│  │ 1. Load *.assets.json from bin/     │                           │
│  │                                     │                           │
│  │ 2. Parse JSON manifests             │                           │
│  │                                     │                           │
│  │ 3. Build URL → FilePath mapping     │                           │
│  │    /modules/AgentHub/module.css     │                           │
│  │      → /path/to/module.css          │                           │
│  │                                     │                           │
│  │ 4. Register middleware              │                           │
│  └─────────────────────────────────────┘                           │
│                    │                                                │
│                    ▼                                                │
│  Request Pipeline                                                   │
│  ┌─────────────────────────────────────┐                           │
│  │ HTTP Request                        │                           │
│  │ GET /modules/AgentHub/module.css    │                           │
│  └─────────────────────────────────────┘                           │
│                    │                                                │
│                    ▼                                                │
│  Custom Asset Middleware                                            │
│  ┌─────────────────────────────────────┐                           │
│  │ 1. Check if URL in asset map        │                           │
│  │                                     │                           │
│  │ 2. If found:                        │                           │
│  │    • Get physical file path         │                           │
│  │    • Set Content-Type header        │                           │
│  │    • SendFileAsync()                │                           │
│  │    • Return file                    │                           │
│  │                                     │                           │
│  │ 3. If not found:                    │                           │
│  │    • Call next middleware           │                           │
│  └─────────────────────────────────────┘                           │
│                    │                                                │
│                    ▼                                                │
│            ┌───────────────┐                                        │
│            │ HTTP Response │                                        │
│            │ 200 OK        │                                        │
│            │ Content-Type  │                                        │
│            │ [file data]   │                                        │
│            └───────────────┘                                        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

### Directory.Build.targets
- **Purpose**: MSBuild automation
- **Trigger**: After Build target
- **Input**: wwwroot files from Client projects
- **Output**: {ServerDllName}.assets.json manifest
- **Processing**: 
  - Scans wwwroot directory
  - Generates JSON mapping URLs to file paths
  - Uses MSBuild item transformations

### AssetEntry Class
- **Purpose**: Data model for manifest entries
- **Properties**:
  - `Url`: Request path (e.g., `/modules/AgentHub/module.css`)
  - `File`: Physical file path (e.g., `/path/to/wwwroot/module.css`)

### Custom Middleware
- **Purpose**: Serve assets from manifests
- **Lifecycle**: Registered after UseOqtane()
- **Performance**: O(1) URL lookup via Dictionary
- **Features**:
  - Content-Type detection via FileExtensionContentTypeProvider
  - Efficient file serving via SendFileAsync
  - Graceful error handling with logging

## URL Structure

### Modules
```
/modules/{ModuleName}/{asset}
Example: /modules/AgentHub/module.css
```

### Themes
```
/themes/{ThemeName}/{asset}
Example: /themes/Oqtane.Themes.FullscreenTheme/Theme.css
```

## Key Benefits

1. **Decoupling**: No ProjectReferences needed from host to modules/themes
2. **Flexibility**: Modules/themes can be deployed independently
3. **Performance**: Assets loaded once at startup, O(1) lookups
4. **Simplicity**: Standard ASP.NET middleware pattern
5. **Development**: Files served directly from source

## Deployment Workflow

```
Developer:
1. Build module/theme
   → dotnet build MyModule/Server/MyModule.Server.csproj

2. Deploy files
   → ./deploy-custom-assets.sh

3. Restart Oqtane.Server
   → Manifests loaded automatically
   → Assets available at configured URLs

User Browser:
1. Request page with module/theme
2. Browser requests asset URLs
3. Middleware serves files
4. CSS/JS applied to page
```

## Extension Points

### Adding New Asset Types

1. Ensure file extension is in FileExtensionContentTypeProvider defaults
2. Or add custom mapping in Program.cs:
   ```csharp
   contentTypeProvider.Mappings[".wasm"] = "application/wasm";
   ```

### Supporting Additional URL Patterns

Modify the AssetUrlPrefix property in project files:
```xml
<AssetUrlPrefix>/custom/path/prefix</AssetUrlPrefix>
```

### Dynamic Module Loading

The current implementation loads manifests at startup. For runtime loading:
1. Watch bin directory for new .assets.json files
2. Reload asset map when changes detected
3. Consider using IOptionsMonitor pattern

## Security Model

- **Whitelist-based**: Only files in manifests are served
- **No traversal**: Exact URL matching prevents directory traversal
- **Read-only**: Files served as-is, no modification
- **Type safety**: Content-Type set by extension, prevents MIME confusion
- **Validation**: File existence checked before serving

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Startup manifest loading | O(n×m) | n=manifests, m=files per manifest |
| URL lookup | O(1) | Dictionary hash lookup |
| File serving | O(1) | Direct file path, kernel sendfile |
| Memory usage | O(n×m) | All URLs cached in memory |

## Comparison with ASP.NET Static Web Assets

| Feature | Custom Pipeline | Static Web Assets |
|---------|----------------|-------------------|
| ProjectReferences needed | ❌ No | ✅ Yes |
| Works without build deps | ✅ Yes | ❌ No |
| Custom URL patterns | ✅ Full control | ⚠️ Limited |
| Development experience | ✅ Direct files | ✅ Direct files |
| Production deployment | ⚠️ Manual copy | ✅ Automatic |
| Learning curve | ⚠️ Custom | ✅ Standard |

## Future Enhancements

1. **Hot Reload**: Watch for manifest changes, reload without restart
2. **Compression**: Gzip/Brotli support for assets
3. **Caching**: ETag/Last-Modified headers
4. **CDN Support**: Configurable URL prefixes
5. **Versioning**: Hash-based cache busting
6. **Bundling**: Combine multiple assets into one request

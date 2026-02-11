# Custom Asset Pipeline for Oqtane Modules and Themes

This implementation provides a custom asset pipeline that allows modules and themes to serve their static assets (CSS, JS, images) without requiring ProjectReferences from the Oqtane.Server host to the module/theme projects.

## How It Works

### 1. Asset Manifest Generation (`Directory.Build.targets`)

The build system automatically generates `.assets.json` manifest files for each module/theme Client project:

- **Target**: `ModuloGenerateClientAssetManifest`
- **Trigger**: Runs after build for projects with `IsClientProject=true` and `ServerDllName` properties
- **Output**: `{ServerDllName}.assets.json` in the Client project's bin directory
- **Content**: JSON array mapping URLs to physical file paths

Example manifest entry:
```json
[
  {"url":"/modules/AgentHub/module.css","file":"/path/to/AgentHub/Client/wwwroot/module.css"},
  {"url":"/modules/AgentHub/agenthub.js","file":"/path/to/AgentHub/Client/wwwroot/agenthub.js"}
]
```

### 2. Custom Middleware (`Program.cs`)

The Oqtane.Server application loads all `.assets.json` files from its bin directory at startup:

1. Scans for `*.assets.json` files in the application bin directory
2. Loads and parses each manifest
3. Builds a URL-to-file mapping in memory
4. Intercepts HTTP requests matching these URLs
5. Serves the files directly using `SendFileAsync`

### 3. Project Structure

#### Module Example (AgentHub):

```
AgentHub/
├── Client/
│   ├── AgentHub.Client.csproj          # Razor SDK, IsClientProject=true
│   ├── Index.razor                     # Module component
│   └── wwwroot/
│       ├── module.css                  # Module styles
│       └── agenthub.js                 # Module JavaScript
└── Server/
    └── AgentHub.Server.csproj          # Standard SDK, produces AgentHub.Server.dll
```

**Client Project Properties**:
```xml
<PropertyGroup>
  <IsClientProject>true</IsClientProject>
  <ServerDllName>AgentHub.Server</ServerDllName>
  <AssetUrlPrefix>/modules/AgentHub</AssetUrlPrefix>
</PropertyGroup>
```

#### Theme Example (FullscreenTheme):

```
Oqtane.Themes.FullscreenTheme/
├── Client/
│   ├── Oqtane.Themes.FullscreenTheme.Client.csproj
│   ├── ThemeInfo.cs                    # Theme metadata
│   └── wwwroot/
│       └── Theme.css                   # Theme styles
└── Server/
    └── Oqtane.Themes.FullscreenTheme.Server.csproj
```

**Client Project Properties**:
```xml
<PropertyGroup>
  <IsClientProject>true</IsClientProject>
  <ServerDllName>Oqtane.Themes.FullscreenTheme</ServerDllName>
  <AssetUrlPrefix>/themes/Oqtane.Themes.FullscreenTheme</AssetUrlPrefix>
</PropertyGroup>
```

## Usage

### For Module Developers

1. Create Client and Server projects as shown above
2. Set the required MSBuild properties in the Client project
3. Place static assets in `Client/wwwroot/`
4. Reference assets using the URL prefix defined in your project:
   ```razor
   @code {
       public override List<Resource> Resources => new List<Resource>()
       {
           new Resource { ResourceType = ResourceType.Stylesheet, Url = "/modules/AgentHub/module.css" },
           new Resource { ResourceType = ResourceType.Script, Url = "/modules/AgentHub/agenthub.js" }
       };
   }
   ```

### For Theme Developers

1. Create Client and Server projects as shown above
2. Set the required MSBuild properties in the Client project
3. Place static assets in `Client/wwwroot/`
4. Reference assets in your ThemeInfo:
   ```csharp
   Resources = new List<Resource>()
   {
       new Stylesheet("/themes/Oqtane.Themes.FullscreenTheme/Theme.css")
   }
   ```

## Deployment

To deploy a module or theme to an Oqtane installation:

1. Build the Server project (this automatically builds the Client project)
2. Copy the Server DLL from `Server/bin/Debug/net10.0/{ServerDllName}.dll` to the Oqtane installation's bin directory
3. Copy the manifest from `Client/bin/Debug/net10.0/{ServerDllName}.assets.json` to the Oqtane installation's bin directory

The manifest file must be in the same directory as the Oqtane.Server executable for the middleware to find and load it.

## Benefits

- **No ProjectReferences**: Host doesn't need to reference every module/theme
- **Dynamic Loading**: Modules/themes can be added by simply copying DLLs and manifests
- **Development Workflow**: Assets are served directly from source during development
- **Production Ready**: Files served with proper content types via `FileExtensionContentTypeProvider`
- **Minimal Dependencies**: Uses standard ASP.NET Core middleware patterns

## Technical Details

### MSBuild Target

The `ModuloGenerateClientAssetManifest` target:
- Runs after the `Build` target
- Uses MSBuild item transformations to generate JSON
- Only processes files when `wwwroot` directory exists
- Skips generation if no asset files are found

### Middleware

The custom middleware:
- Runs early in the pipeline (after `UseOqtane`)
- Uses case-insensitive URL matching
- Validates file existence before serving
- Sets appropriate Content-Type headers
- Falls through to next middleware if URL not matched

### Performance Considerations

- Manifests are loaded once at application startup
- URL lookups use `Dictionary<string, string>` with O(1) performance
- Files are served using `SendFileAsync` for efficient I/O
- No runtime file system scanning required

## Example Assets

### AgentHub Module

- **module.css**: Gradient background styling for the module container
- **agenthub.js**: JavaScript with test function to verify asset loading

### FullscreenTheme

- **Theme.css**: Fullscreen layout with gradient background and backdrop blur effects

## Troubleshooting

### Assets return 404

1. Verify the manifest file exists in the Oqtane.Server bin directory
2. Check that the manifest contains the correct URL and file path
3. Verify the physical file exists at the path specified in the manifest
4. Check application logs for manifest loading errors

### Assets not updating

1. Rebuild the Client project to regenerate the manifest
2. Ensure the updated manifest is copied to the Oqtane.Server bin directory
3. Restart the application to reload manifests

### Content-Type issues

The middleware uses `FileExtensionContentTypeProvider` which handles common file types. For custom file types, the content type mapping can be extended in the middleware code.

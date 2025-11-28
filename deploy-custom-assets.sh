#!/bin/bash
# Deploy custom modules and themes to Oqtane.Server
# This script copies module/theme DLLs and asset manifests to the Oqtane.Server bin directory

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SERVER_BIN="$SCRIPT_DIR/Oqtane.Server/bin/Debug/net10.0"

echo "Deploying custom modules and themes to Oqtane.Server..."

# Deploy AgentHub Module
if [ -d "$SCRIPT_DIR/AgentHub/Server" ]; then
    echo "Deploying AgentHub module..."
    
    # Copy Server DLL
    cp "$SCRIPT_DIR/AgentHub/Server/bin/Debug/net10.0/AgentHub.Server.dll" "$SERVER_BIN/" 2>/dev/null || true
    
    # Copy asset manifest
    cp "$SCRIPT_DIR/AgentHub/Client/bin/Debug/net10.0/AgentHub.Server.assets.json" "$SERVER_BIN/" 2>/dev/null || true
    
    # Copy Client DLL (may be needed for Blazor)
    cp "$SCRIPT_DIR/AgentHub/Client/bin/Debug/net10.0/AgentHub.Client.dll" "$SERVER_BIN/" 2>/dev/null || true
    
    echo "  ✓ AgentHub deployed"
fi

# Deploy FullscreenTheme
if [ -d "$SCRIPT_DIR/Oqtane.Themes.FullscreenTheme/Server" ]; then
    echo "Deploying FullscreenTheme..."
    
    # Copy Server DLL
    cp "$SCRIPT_DIR/Oqtane.Themes.FullscreenTheme/Server/bin/Debug/net10.0/Oqtane.Themes.FullscreenTheme.dll" "$SERVER_BIN/" 2>/dev/null || true
    
    # Copy asset manifest
    cp "$SCRIPT_DIR/Oqtane.Themes.FullscreenTheme/Client/bin/Debug/net10.0/Oqtane.Themes.FullscreenTheme.assets.json" "$SERVER_BIN/" 2>/dev/null || true
    
    # Copy Client DLL (may be needed for Blazor)
    cp "$SCRIPT_DIR/Oqtane.Themes.FullscreenTheme/Client/bin/Debug/net10.0/Oqtane.Themes.FullscreenTheme.Client.dll" "$SERVER_BIN/" 2>/dev/null || true
    
    echo "  ✓ FullscreenTheme deployed"
fi

echo ""
echo "Deployment complete! Deployed files:"
ls -lh "$SERVER_BIN"/*.assets.json 2>/dev/null || echo "  No asset manifests found"
echo ""
echo "To verify deployment, check that the following files exist:"
echo "  - $SERVER_BIN/AgentHub.Server.dll"
echo "  - $SERVER_BIN/AgentHub.Server.assets.json"
echo "  - $SERVER_BIN/Oqtane.Themes.FullscreenTheme.dll"
echo "  - $SERVER_BIN/Oqtane.Themes.FullscreenTheme.assets.json"

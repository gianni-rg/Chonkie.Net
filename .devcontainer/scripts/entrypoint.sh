#!/bin/bash
set -e

# Fix permissions for vscode directories that might have been created by Docker/Podman with wrong ownership
# This runs as root before switching to the vscode user
if [ "$(id -u)" = "0" ]; then
    echo "Fixing permissions for vscode user..."
    
    # Fix vscode server directories
    for dir in /home/vscode/.vscode-server /home/vscode/.vscode-server-insiders; do
        if [ -d "$dir" ]; then
            chown -R vscode:vscode "$dir" 2>/dev/null || echo "Warning: Could not change ownership of $dir"
        fi
    done
    
    # Fix workspace permissions for .NET build outputs
    if [ -d "/workspace" ]; then
        echo "Fixing workspace build output permissions..."
        # One-time full ownership fix on first start (can be heavy on large repos)
        if [ ! -f "/workspace/.permissions_fixed" ]; then
            echo "Performing one-time ownership fix for /workspace (this may take a while)..."
            chown -R vscode:vscode /workspace 2>/dev/null || echo "Warning: Could not recursively change ownership of /workspace"
            # Mark as done to avoid repeating on next starts
            su -s /bin/bash -c 'touch /workspace/.permissions_fixed' vscode 2>/dev/null || true
        fi

        # Always ensure obj/bin are writable and owned by vscode
        find /workspace -type d \( -name "obj" -o -name "bin" \) -print0 2>/dev/null | xargs -0 -r chown -R vscode:vscode 2>/dev/null || true
        find /workspace -type d \( -name "obj" -o -name "bin" \) -print0 2>/dev/null | xargs -0 -r chmod -R u+rwX 2>/dev/null || true
        # Ensure workspace root is writable by vscode
        chown vscode:vscode /workspace 2>/dev/null || echo "Warning: Could not change ownership of /workspace"
    fi
    
    echo "Switching to vscode user..."
    # Switch to vscode user and execute the command
    exec gosu vscode "$@"
else
    # Already running as vscode user
    exec "$@"
fi

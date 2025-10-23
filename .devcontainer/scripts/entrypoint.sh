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
    
    # Fix workspace permissions - give full write access to vscode user
    if [ -d "/workspace" ]; then
        echo "Fixing workspace permissions for full write access..."
        # One-time full ownership and permission fix on first start
        if [ ! -f "/workspace/.permissions_fixed" ]; then
            echo "Performing one-time ownership and permission fix for /workspace (this may take a while)..."
            # Try to change ownership to vscode user (requires CAP_CHOWN)
            if chown -R vscode:vscode /workspace 2>/dev/null; then
                # Also ensure user has write on everything
                chmod -R u+rwX /workspace 2>/dev/null || echo "Warning: Could not recursively change permissions of /workspace"
            else
                echo "Ownership change not permitted on /workspace. Falling back to world-writable permissions."
                # If chown isn't allowed on the mount, make all files/dirs writable by everyone
                chmod -R a+rwX /workspace 2>/dev/null || echo "Warning: Could not set world-writable permissions on /workspace"
            fi
            # Mark as done to avoid repeating on next starts
            touch /workspace/.permissions_fixed 2>/dev/null || true
        fi

        # Always ensure workspace root is accessible
        chown vscode:vscode /workspace 2>/dev/null || true
        chmod a+rwx /workspace 2>/dev/null || true
        
        # Quick permission fix for common build directories on every start
        find /workspace -maxdepth 3 -type d \( -name "obj" -o -name "bin" \) -print0 2>/dev/null | xargs -0 -r chmod -R a+rwX 2>/dev/null || true
    fi
    
    echo "Switching to vscode user..."
    # Switch to vscode user and execute the command
    exec gosu vscode "$@"
else
    # Already running as vscode user
    exec "$@"
fi

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
    
    echo "Switching to vscode user..."
    # Switch to vscode user and execute the command
    exec gosu vscode "$@"
else
    # Already running as vscode user
    exec "$@"
fi

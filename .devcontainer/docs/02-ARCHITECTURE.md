# Dev Container Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HOST MACHINE (Windows)                        │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                      VS Code Editor                             │ │
│  │                                                                  │ │
│  │  ┌──────────────────────────────────────────────────────────┐  │ │
│  │  │        Remote - Containers Extension                      │  │ │
│  │  └──────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────┬─────────────────────────────────────┘ │
│                                │                                       │
│  ┌────────────────────────────▼─────────────────────────────────────┐ │
│  │                     Podman Engine                                 │ │
│  │                                                                    │ │
│  │  ┌──────────────────────────────────────────────────────────┐   │ │
│  │  │           devcontainer-network (bridge)                   │   │ │
│  │  │                                                            │   │ │
│  │  │  ┌─────────────────────┐    ┌──────────────────────┐     │   │ │
│  │  │  │   proxy container   │    │  devcontainer        │     │   │ │
│  │  │  │   (Squid Proxy)     │◄───┤  (Your workspace)    │     │   │ │
│  │  │  │                     │    │                      │     │   │ │
│  │  │  │  Port: 3128         │    │  User: vscode        │     │   │ │
│  │  │  │  Filters:           │    │  Read-only: Yes      │     │   │ │
│  │  │  │  - allowed-domains  │    │  Capabilities: Drop  │     │   │ │
│  │  │  └──────────┬──────────┘    └──────────┬───────────┘     │   │ │
│  │  │             │                           │                 │   │ │
│  │  └─────────────┼───────────────────────────┼─────────────────┘   │ │
│  │                │ FILTERED                  │                     │ │
│  │                │ INTERNET                  │ MOUNTED             │ │
│  └────────────────┼───────────────────────────┼─────────────────────┘ │
│                   │                           │                       │
│                   ▼                           ▼                       │
│              Internet                   ┌──────────────────┐          │
│           (Whitelisted                  │  Host Volumes    │          │
│            Domains Only)                │                  │          │
│                                         │  - Workspace     │          │
│   Allowed:                              │  - .ssh (ro)     │          │
│   ✓ api.openai.com                      │  - .gitconfig    │          │
│   ✓ api.anthropic.com                   │  - External dirs │          │
│   ✓ generativelanguage.googleapis.com   └──────────────────┘          │
│   ✓ github.com                                                        │
│   ✓ nuget.org                          Note: Podman runs rootless    │
│   ✗ facebook.com (blocked)              for enhanced security         │
│   ✗ twitter.com (blocked)                                             │
└───────────────────────────────────────────────────────────────────────┘
```

## Container Filesystem Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ Container Filesystem (Read-Only Root)                            │
│                                                                   │
│  /workspace/                 ← Mounted from host (read-write)    │
│    ├── src/                  ← Your source code                  │
│    ├── tests/                                                     │
│    ├── samples/                                                   │
│    └── Chonkie.Net.sln                                            │
│                                                                   │
│  /mnt/                       ← External mounts (configurable)    │
│    ├── data/                 ← External data (read-only)         │
│    ├── libs/                 ← External libraries (read-only)    │
│    └── output/               ← External output (read-write)      │
│                                                                   │
│  /home/vscode/               ← User home (writable areas)        │
│    ├── .nuget/               ← Persistent volume                 │
│    ├── .vscode-server/       ← Persistent volume                 │
│    ├── .ssh/                 ← Mounted from host (read-only)     │
│    ├── .gitconfig            ← Mounted from host (read-only)     │
│    ├── bin/                  ← User scripts (writable)           │
│    └── commandhistory/       ← Persistent volume                 │
│                                                                   │
│  /tmp/                       ← Tmpfs (writable, noexec, 1GB)     │
│  /var/tmp/                   ← Tmpfs (writable, noexec, 512MB)   │
│  /run/                       ← Tmpfs (writable, noexec, 256MB)   │
│                                                                   │
│  /usr/, /bin/, /etc/, ...    ← Read-only system files ✗          │
└───────────────────────────────────────────────────────────────────┘
```

## Security Layers

```
┌────────────────────────────────────────────────────────────┐
│  Layer 1: Network Security                                  │
│  ┌────────────────────────────────────────────────────┐    │
│  │ • Squid proxy with domain whitelist                │    │
│  │ • All HTTP/HTTPS traffic filtered                  │    │
│  │ • DNS locked to 8.8.8.8, 1.1.1.1                   │    │
│  │ • Optional: Complete network isolation             │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 2: Container Security                                │
│  ┌────────────────────────────────────────────────────┐    │
│  │ • Read-only root filesystem                        │    │
│  │ • All capabilities dropped (CAP_DROP ALL)          │    │
│  │ • No new privileges (prevents escalation)          │    │
│  │ • Seccomp profile (optional)                       │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 3: User Security                                     │
│  ┌────────────────────────────────────────────────────┐    │
│  │ • Non-root user (vscode, UID 1000)                 │    │
│  │ • Limited filesystem access                        │    │
│  │ • Sudo available but auditable                     │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 4: Filesystem Security                               │
│  ┌────────────────────────────────────────────────────┐    │
│  │ • Workspace only (no access to host files)         │    │
│  │ • Selective mounts (ro/rw as needed)               │    │
│  │ • Temp directories with noexec                     │    │
│  │ • SSH keys mounted read-only                       │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 5: Resource Security                                 │
│  ┌────────────────────────────────────────────────────┐    │
│  │ • CPU limit: 4 cores max                           │    │
│  │ • Memory limit: 8GB max                            │    │
│  │ • Reservations: 2 cores, 4GB                       │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
```

## Data Flow: AI Tool Request

```
┌─────────────┐
│   VS Code   │  1. User runs: ask-openai "Hello"
│   Editor    │
└──────┬──────┘
       │
       ▼
┌─────────────────────────┐
│  devcontainer terminal  │  2. Command executes in container
│  (vscode user)          │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  openai-cli tool        │  3. Makes HTTPS request
│                         │     to api.openai.com
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  http_proxy env var     │  4. Request routed to proxy
│  → proxy:3128           │     (http://proxy:3128)
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  Squid Proxy            │  5. Check domain whitelist
│  allowed-domains.txt    │     ✓ api.openai.com → ALLOW
│                         │     ✗ malicious.com  → DENY
└──────┬──────────────────┘
       │ ALLOWED
       ▼
┌─────────────────────────┐
│  Internet               │  6. Request reaches OpenAI
│  api.openai.com         │
└──────┬──────────────────┘
       │
       ▼ Response
┌─────────────────────────┐
│  Back through proxy     │  7. Response logged, returned
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  openai-cli displays    │  8. User sees response
│  in terminal            │
└─────────────────────────┘
```

## Volume Persistence

```
┌──────────────────────────────────────────────────────────┐
│  Persistent Podman Volumes (survive container restart)    │
│                                                            │
│  devcontainer-nuget                                       │
│    → /home/vscode/.nuget/                                 │
│    → NuGet package cache                                  │
│                                                            │
│  devcontainer-vscode-extensions                           │
│    → /home/vscode/.vscode-server/extensions/              │
│    → Installed VS Code extensions                         │
│                                                            │
│  devcontainer-bash-history                                │
│    → /home/vscode/commandhistory/                         │
│    → Command history persistence                          │
│                                                            │
│  squid-cache                                              │
│    → /var/spool/squid/                                    │
│    → HTTP cache (speeds up repeated requests)             │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│  Bind Mounts (direct host access)                         │
│                                                            │
│  Host: C:\Projects\Personal\Chonkie.Net                   │
│    → Container: /workspace (read-write)                   │
│                                                            │
│  Host: C:\Users\YourName\.ssh                             │
│    → Container: /home/vscode/.ssh (read-only)             │
│                                                            │
│  Host: C:\Users\YourName\.gitconfig                       │
│    → Container: /home/vscode/.gitconfig (read-only)       │
│                                                            │
│  (Optional) Host: D:\Data                                 │
│    → Container: /mnt/data (read-only)                     │
└──────────────────────────────────────────────────────────┘
```

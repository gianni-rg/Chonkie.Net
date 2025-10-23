# Base image with .NET SDK
FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Build arguments
ARG USER_UID=1000
ARG USER_GID=1000

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    ASPNETCORE_ENVIRONMENT=Development

# Install additional packages
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    git \
    vim \
    nano \
    jq \
    tree \
    htop \
    net-tools \
    iputils-ping \
    dnsutils \
    netcat-openbsd \
    ca-certificates \
    gnupg \
    lsb-release \
    sudo \
    gosu \
    # Python and pip for AI tools
    python3 \
    python3-pip \
    python3-venv \
    # Node.js tools (already installed via feature, but ensuring npm)
    npm \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Ensure vscode user exists with correct UID/GID
RUN if [ "$USER_GID" != "1000" ] || [ "$USER_UID" != "1000" ]; then \
        groupmod -g $USER_GID vscode && \
        usermod -u $USER_UID -g $USER_GID vscode; \
    fi

# Add vscode user to sudoers (for convenience, can be removed for stricter security)
RUN echo "vscode ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/vscode \
    && chmod 0440 /etc/sudoers.d/vscode

# Copy and set up entrypoint script (must be done as root)
COPY scripts/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Create directories for writable areas
RUN mkdir -p \
    /home/vscode/.nuget \
    /home/vscode/.vscode-server \
    /home/vscode/.vscode-server-insiders \
    /home/vscode/commandhistory \
    /home/vscode/.local \
    /home/vscode/.cache \
    /home/vscode/.config \
    /home/vscode/bin \
    /workspace \
    && chown -R vscode:vscode /home/vscode /workspace

# Switch to vscode user
USER vscode

# Install global .NET tools
RUN dotnet tool install -g dotnet-ef \
    && dotnet tool install -g dotnet-format \
    && dotnet tool install -g dotnet-outdated-tool

# Add .NET tools to PATH
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# Install Python AI CLI tools (will be completed in post-create script)
# Pre-install some common packages to speed up container creation
# Using --break-system-packages is safe in containers (PEP 668)
RUN python3 -m pip install --user --no-cache-dir --break-system-packages \
    openai \
    anthropic \
    google-generativeai \
    requests \
    rich

# Set up bash history persistence
RUN SNIPPET="export PROMPT_COMMAND='history -a' && export HISTFILE=/home/vscode/commandhistory/.bash_history" \
    && mkdir -p /home/vscode/commandhistory \
    && echo "$SNIPPET" >> "/home/vscode/.bashrc"

# Create workspace directory
WORKDIR /workspace

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet --list-sdks || exit 1

# Set entrypoint to fix permissions before starting
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Default command
CMD ["sleep", "infinity"]

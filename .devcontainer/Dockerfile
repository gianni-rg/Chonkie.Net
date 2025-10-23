# Base image with .NET SDK
FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# Build arguments
ARG USER_UID=1000
ARG USER_GID=1000

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    ASPNETCORE_ENVIRONMENT=Development \
    PIP_DEFAULT_TIMEOUT=100 \
    PIP_TRUSTED_HOST="pypi.org pypi.python.org files.pythonhosted.org"

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

# Copy pip configuration for proxy support (will be applied in post-create)
# NOTE: Do NOT apply during build as proxy container doesn't exist yet
# The Python feature installation will fail if it tries to use proxy during build
COPY pip.conf /tmp/pip.conf

# Copy npm configuration for proxy support
COPY npmrc /home/vscode/.npmrc
RUN chown vscode:vscode /home/vscode/.npmrc

# Copy wget configuration for proxy support
COPY wgetrc /etc/wgetrc

# Copy curl configuration for proxy support  
COPY curlrc /home/vscode/.curlrc
RUN chown vscode:vscode /home/vscode/.curlrc

# Copy git proxy configuration (will be applied in post-create)
COPY gitconfig-proxy /tmp/gitconfig-proxy

# Copy NuGet proxy configuration (will be applied in post-create)
COPY nuget.config /tmp/nuget.config

# Copy APT proxy configuration (will be applied in post-create)
# NOTE: Do NOT apply during build as proxy container doesn't exist yet
COPY apt-proxy.conf /tmp/apt-proxy.conf

# Copy Docker CLI proxy configuration (for docker-in-docker scenarios)
COPY docker-config.json /home/vscode/.docker/config.json
RUN mkdir -p /home/vscode/.docker && chown -R vscode:vscode /home/vscode/.docker

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
    /home/vscode/.dotnet-build \
    /workspace \
    && chown -R vscode:vscode /home/vscode /workspace

# NOTE: Do NOT set USER here - entrypoint.sh will switch to vscode user
# This allows entrypoint to run as root and fix permissions first

# NOTE: .NET tools will be installed in post-create script
# This is because they need network access through the proxy,
# which is only available after the container is running

# Add .NET tools to PATH (tools will be installed later)
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# NOTE: Python packages will be installed in post-create script
# This is because they need network access through the proxy,
# which is only available after the container is running with docker-compose
# The proxy service must be up before pip can access PyPI

# Set up bash history persistence
RUN SNIPPET="export PROMPT_COMMAND='history -a' && export HISTFILE=/home/vscode/commandhistory/.bash_history" \
    && mkdir -p /home/vscode/commandhistory \
    && echo "$SNIPPET" >> "/home/vscode/.bashrc" \
    && chown -R vscode:vscode /home/vscode/commandhistory

# Create workspace directory
WORKDIR /workspace

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet --list-sdks || exit 1

# Set entrypoint to fix permissions before starting
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Default command
CMD ["sleep", "infinity"]

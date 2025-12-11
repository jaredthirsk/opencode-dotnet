FROM mcr.microsoft.com/dotnet/sdk:8.0

ENV DEBIAN_FRONTEND=noninteractive

# Install essential development tools and locale support
RUN apt-get update && apt-get install -y \
    curl \
    git \
    vim \
    neovim \
    less \
    wget \
    unzip \
    sudo \
    tmux \
    htop \
    jq \
    build-essential \
    python3 \
    python3-pip \
    procps \
    locales \
    dos2unix \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Generate and set up locales
RUN sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen \
    && locale-gen \
    && update-locale LANG=en_US.UTF-8

# Create non-root user for development
ARG USERNAME=dev
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Handle existing users in base image (node images often have UID 1000)
RUN if ! getent group $USER_GID > /dev/null 2>&1; then groupadd --gid $USER_GID $USERNAME; fi \
    && if ! id -u $USER_UID > /dev/null 2>&1; then \
         useradd --uid $USER_UID --gid $USER_GID -m $USERNAME -s /bin/bash; \
       else \
         existing_user=$(getent passwd $USER_UID | cut -d: -f1); \
         usermod -l $USERNAME -d /home/$USERNAME -m $existing_user && groupmod -n $USERNAME $(getent group $USER_GID | cut -d: -f1); \
       fi \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# Create /run/user/1000 with correct permissions and ownership
RUN mkdir -p /run/user/1000 \
    && chown 1000:1000 /run/user/1000 \
    && chmod 700 /run/user/1000

# Copy tmux configuration
COPY --chmod=644 .tmux.conf /home/$USERNAME/.tmux.conf
RUN chown $USERNAME:$USERNAME /home/$USERNAME/.tmux.conf

# Switch to the non-root user
USER $USERNAME
WORKDIR /home/$USERNAME

# Install nvm and Node.js (for tooling and web-based test runners)
ENV NVM_DIR=/home/$USERNAME/.nvm
ENV NODE_VERSION=22.17.1

RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash \
    && . $NVM_DIR/nvm.sh \
    && nvm install $NODE_VERSION \
    && nvm alias default $NODE_VERSION

# Update PATH to include Claude and local binaries
ENV PATH="$NVM_DIR/versions/node/v$NODE_VERSION/bin:/home/$USERNAME/.dotnet/tools:/home/$USERNAME/.local/bin:/home/$USERNAME/.claude/local/node_modules/.bin:$PATH"

# Install global npm packages (as user after node installation)
RUN . $NVM_DIR/nvm.sh \
    && npm install -g \
    typescript \
    eslint \
    prettier

# Install .NET tools (dotnet-format is built into .NET 8 SDK)
RUN dotnet tool install --global dotnet-watch

# Install chezmoi for dotfiles management
RUN sh -c "$(curl -fsLS get.chezmoi.io)" -- -b /home/$USERNAME/.local/bin

# Configure git to handle line endings correctly (prevent CRLF from being introduced)
RUN git config --global core.autocrlf input \
    && git config --global core.eol lf

# Set up workspace directory and VS Code server directory
USER root
RUN mkdir -p /src && chown -R $USERNAME:$USERNAME /src \
    && mkdir -p /home/$USERNAME/.vscode-server \
    && chown -R $USERNAME:$USERNAME /home/$USERNAME/.vscode-server \
    && chmod 755 /home/$USERNAME/.vscode-server
USER $USERNAME
WORKDIR /src/opencode-dotnet

# Set environment variables
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV NUGET_XMLDOC_MODE=skip
ENV ASPNETCORE_ENVIRONMENT=Development

# Set locale environment variables
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8
ENV LANGUAGE=en_US:en

CMD ["/bin/bash", "-c", "while sleep 1000; do :; done"]

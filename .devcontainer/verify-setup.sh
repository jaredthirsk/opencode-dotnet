#!/bin/bash
# DevContainer Setup Verification Script

set -e

echo "================================"
echo "OpenCode .NET SDK DevContainer"
echo "Setup Verification"
echo "================================"
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

check_pass() {
    echo -e "${GREEN}✓${NC} $1"
}

check_fail() {
    echo -e "${RED}✗${NC} $1"
}

check_warn() {
    echo -e "${YELLOW}⚠${NC} $1"
}

echo "Checking required tools..."

# Check .NET SDK
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    check_pass ".NET SDK installed: $DOTNET_VERSION"
else
    check_fail ".NET SDK not found"
    exit 1
fi

# Check Node.js
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    check_pass "Node.js installed: $NODE_VERSION"
else
    check_warn "Node.js not found (optional)"
fi

# Check Git
if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version)
    check_pass "Git installed: $GIT_VERSION"
else
    check_fail "Git not found"
    exit 1
fi

# Check tmux
if command -v tmux &> /dev/null; then
    TMUX_VERSION=$(tmux -V)
    check_pass "tmux installed: $TMUX_VERSION"
else
    check_warn "tmux not found (optional)"
fi

# Check neovim
if command -v nvim &> /dev/null; then
    check_pass "neovim installed"
else
    check_warn "neovim not found (optional)"
fi

echo ""
echo "Checking .NET tools..."

# Check dotnet-format
if dotnet tool list -g | grep -q dotnet-format; then
    check_pass "dotnet-format installed"
else
    check_warn "dotnet-format not installed"
fi

# Check dotnet-ef
if dotnet tool list -g | grep -q dotnet-ef; then
    check_pass "dotnet-ef installed"
else
    check_warn "dotnet-ef not installed"
fi

echo ""
echo "Checking environment..."

# Check workspace directory
if [ -d "/src/opencode-dotnet" ]; then
    check_pass "Workspace directory exists: /src/opencode-dotnet"
else
    check_fail "Workspace directory not found: /src/opencode-dotnet"
fi

# Check if we're on WSL native filesystem
CURRENT_DIR=$(pwd)
if [[ $CURRENT_DIR == /mnt/* ]]; then
    check_warn "Current directory is on Windows filesystem (/mnt/*)."
    check_warn "For better performance, consider moving to WSL native filesystem (/src/ or /home/)"
else
    check_pass "Using WSL native filesystem (optimal performance)"
fi

# Check line endings
echo ""
echo "Checking line endings..."
if command -v dos2unix &> /dev/null; then
    check_pass "dos2unix available for line ending conversion"
else
    check_warn "dos2unix not installed"
fi

# Check .gitattributes
if [ -f "/src/opencode-dotnet/.gitattributes" ]; then
    check_pass ".gitattributes configured for proper line endings"
else
    check_warn ".gitattributes not found - shell scripts may have incorrect line endings"
fi

# Check .editorconfig
if [ -f "/src/opencode-dotnet/.editorconfig" ]; then
    check_pass ".editorconfig present for consistent code style"
else
    check_warn ".editorconfig not found"
fi

echo ""
echo "Checking volumes..."

# Check NuGet cache
if [ -d "$HOME/.nuget" ]; then
    check_pass "NuGet cache directory exists"
else
    check_warn "NuGet cache directory not found"
fi

# Check VS Code server
if [ -d "$HOME/.vscode-server" ]; then
    check_pass "VS Code server directory exists"
else
    check_warn "VS Code server directory not found (will be created on first VS Code connection)"
fi

echo ""
echo "================================"
echo "Verification Complete!"
echo "================================"
echo ""
echo "Next steps:"
echo "1. Run 'dotnet build' to build the project"
echo "2. Run 'dotnet test' to run tests"
echo "3. Run 'dotnet pack -c Release' to create NuGet package"
echo ""

#!/bin/bash

# Read the host's hostname
HOST_HOSTNAME=$(cat /etc/host_hostname 2>/dev/null || echo "unknown")

# Set the container hostname to PROJECT_NAME.HOST_HOSTNAME
NEW_HOSTNAME="${PROJECT_NAME:-opencode-dotnet}.${HOST_HOSTNAME}"

# Update the hostname
echo "$NEW_HOSTNAME" > /etc/hostname
hostname "$NEW_HOSTNAME"

# Update /etc/hosts to include the new hostname
echo "127.0.0.1 $NEW_HOSTNAME" >> /etc/hosts

echo "Container hostname set to: $NEW_HOSTNAME"

# Execute the original command
exec "$@"

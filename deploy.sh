#!/bin/bash

# Configuration
REMOTE_HOST="192.168.0.205"
REMOTE_USER="david"
REMOTE_PATH="/share/David/Chores"
REMOTE_DATA_PATH="/share/David/ChoresData"
DOCKER_PATH="/share/CACHEDEV1_DATA/.qpkg/container-station/bin/docker"
DOCKER_IMAGE_NAME="chores:latest"
DOCKER_CONTAINER_NAME="chores-app"
DOCKER_PORT="5209:8080"

# SSH options to reuse connection (avoid repeated password prompts)
SSH_OPTS="-o ControlMaster=auto -o ControlPath=~/.ssh/control-%h-%p-%r -o ControlPersist=600"
SSH_CMD="ssh $SSH_OPTS"

echo "🚀 Starting deployment to $REMOTE_HOST..."

# Step 1: Copy files to remote host and create data directory
echo "📁 Copying files to remote host..."
$SSH_CMD "$REMOTE_USER@$REMOTE_HOST" "mkdir -p $REMOTE_PATH $REMOTE_DATA_PATH"
rsync -av -e "ssh $SSH_OPTS" --exclude='bin' --exclude='obj' --exclude='.vs' --exclude='*.dll' --exclude='*.pdb' . "$REMOTE_USER@$REMOTE_HOST:$REMOTE_PATH/"

if [ $? -ne 0 ]; then
    echo "❌ Failed to copy files"
    exit 1
fi
echo "✅ Files copied successfully"

# Step 2: Build Docker image
echo "🐳 Building Docker image on remote host..."
$SSH_CMD "$REMOTE_USER@$REMOTE_HOST" "cd $REMOTE_PATH && $DOCKER_PATH build -t $DOCKER_IMAGE_NAME ."

if [ $? -ne 0 ]; then
    echo "❌ Failed to build Docker image"
    exit 1
fi
echo "✅ Docker image built successfully"

# Step 3: Stop and remove existing container (if running)
echo "🛑 Stopping and removing existing container (if any)..."
$SSH_CMD "$REMOTE_USER@$REMOTE_HOST" "$DOCKER_PATH stop $DOCKER_CONTAINER_NAME 2>/dev/null && $DOCKER_PATH rm $DOCKER_CONTAINER_NAME 2>/dev/null || true"

# Step 4: Run Docker container
echo "🚀 Running Docker container..."
$SSH_CMD "$REMOTE_USER@$REMOTE_HOST" "$DOCKER_PATH run -d \
  --name $DOCKER_CONTAINER_NAME \
  -p $DOCKER_PORT \
  -v $REMOTE_DATA_PATH:/app/Data \
  $DOCKER_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo "❌ Failed to run Docker container"
    exit 1
fi
echo "✅ Docker container started successfully"

# Step 5: Display status
echo ""
echo "📊 Deployment Summary:"
echo "  Host: $REMOTE_HOST"
echo "  Container: $DOCKER_CONTAINER_NAME"
echo "  Image: $DOCKER_IMAGE_NAME"
echo "  Port: $DOCKER_PORT"
echo "  App path: $REMOTE_PATH"
echo "  Data path: $REMOTE_DATA_PATH"
echo ""
echo "🌐 Access the app at: http://$REMOTE_HOST:5209"
echo ""
echo "📝 Useful commands:"
echo "  View logs: ssh $SSH_OPTS $REMOTE_USER@$REMOTE_HOST '$DOCKER_PATH logs -f $DOCKER_CONTAINER_NAME'"
echo "  Stop container: ssh $SSH_OPTS $REMOTE_USER@$REMOTE_HOST '$DOCKER_PATH stop $DOCKER_CONTAINER_NAME'"
echo "  Remove container: ssh $SSH_OPTS $REMOTE_USER@$REMOTE_HOST '$DOCKER_PATH rm $DOCKER_CONTAINER_NAME'"

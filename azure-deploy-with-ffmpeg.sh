#!/bin/bash

# Azure Docker Deployment Script with FFmpeg
# This script builds and deploys the backend API container with FFmpeg to Azure

# Variables
RESOURCE_GROUP="taktiqRecursos"
APP_NAME="taktiq-api"
ACR_NAME="taktiqacr"  # Replace with your actual Azure Container Registry name
IMAGE_NAME="gymhero-api"
TAG="latest"

echo "=========================================="
echo "Building and Deploying Backend with FFmpeg"
echo "=========================================="

# Step 1: Build the Docker image locally
echo ""
echo "Step 1: Building Docker image with FFmpeg..."
cd "$(dirname "$0")"
docker build -f src/GymHero.Api/Dockerfile -t $IMAGE_NAME:$TAG .

if [ $? -ne 0 ]; then
    echo "Error: Docker build failed!"
    exit 1
fi

echo "Docker image built successfully!"

# Step 2: Verify FFmpeg is installed
echo ""
echo "Step 2: Verifying FFmpeg installation..."
docker run --rm $IMAGE_NAME:$TAG ffmpeg -version

# Step 3: Login to Azure Container Registry (if you have one)
echo ""
echo "Step 3: Azure Container Registry..."
echo "Do you have an Azure Container Registry? (y/n)"
read -r HAS_ACR

if [ "$HAS_ACR" = "y" ]; then
    echo "Enter your Azure Container Registry name:"
    read -r ACR_NAME

    echo "Logging into Azure Container Registry..."
    az acr login --name $ACR_NAME

    # Tag the image for ACR
    ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"
    docker tag $IMAGE_NAME:$TAG $ACR_LOGIN_SERVER/$IMAGE_NAME:$TAG

    # Push to ACR
    echo "Pushing image to Azure Container Registry..."
    docker push $ACR_LOGIN_SERVER/$IMAGE_NAME:$TAG

    # Configure App Service to use the ACR image
    echo "Configuring App Service to use ACR image..."
    az webapp config container set \
        --name $APP_NAME \
        --resource-group $RESOURCE_GROUP \
        --docker-custom-image-name $ACR_LOGIN_SERVER/$IMAGE_NAME:$TAG \
        --docker-registry-server-url https://$ACR_LOGIN_SERVER

    echo "Container deployed from ACR!"
else
    # Alternative: Deploy from Docker Hub or create ACR
    echo ""
    echo "Option 1: Create an Azure Container Registry (Recommended)"
    echo "  az acr create --resource-group $RESOURCE_GROUP --name [UNIQUE-NAME] --sku Basic"
    echo "  Then re-run this script"
    echo ""
    echo "Option 2: Push to Docker Hub"
    echo "  docker login"
    echo "  docker tag $IMAGE_NAME:$TAG yourusername/$IMAGE_NAME:$TAG"
    echo "  docker push yourusername/$IMAGE_NAME:$TAG"
    echo "  Then update App Service to use: yourusername/$IMAGE_NAME:$TAG"
fi

# Step 4: Restart the App Service
echo ""
echo "Step 4: Restarting App Service..."
az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP

echo ""
echo "=========================================="
echo "Deployment Complete!"
echo "=========================================="
echo ""
echo "FFmpeg is now available in your Azure container!"
echo "You can test video processing by uploading a video through your API."
echo ""
echo "API URL: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net"

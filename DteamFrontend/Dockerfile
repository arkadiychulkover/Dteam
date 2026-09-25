# Stage 1: Build Svelte 5 / Vite SPA
FROM node:20-alpine AS builder
WORKDIR /app

# Copy package manifests and install dependencies
COPY dteam-app/package*.json ./
RUN npm ci

# Copy application source code and build production assets
COPY dteam-app/ ./
RUN npm run build

# Stage 2: Production Nginx Web Server
FROM nginx:alpine AS runner
WORKDIR /usr/share/nginx/html

# Clean default static files
RUN rm -rf ./*

# Copy Nginx reverse proxy configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy compiled SPA from builder stage
COPY --from=builder /app/dist ./

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

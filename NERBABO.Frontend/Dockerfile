# Build stage
FROM node:18-alpine as build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install ALL dependencies (including devDependencies for build tools)
RUN npm ci

# Copy source code
COPY . .

# Clear any build cache and build the app for production
RUN rm -rf dist/ node_modules/.cache/ .angular/cache/
RUN npm run build -- --configuration production

# Production stage
FROM nginx:alpine

# Copy the built app
COPY --from=build /app/dist/nerbabo.frontend/browser/ /usr/share/nginx/html/

# Copy nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
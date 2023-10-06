# Use an official Node.js runtime as a parent image
FROM node:16 as build

# Set the working directory in the container to /app
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY package*.json ./

# Install Angular CLI globally (you can skip this step if you've already installed it globally)
RUN npm install -g @angular/cli

# Install project dependencies
RUN npm install

# Copy the entire project directory into the container
COPY . .

# Build the Angular app for production
RUN ng build --configuration production

# Use NGINX as a lightweight web server to serve the Angular app
FROM nginx:alpine

# Copy the built Angular app from the 'build' stage to the NGINX web server directory
COPY --from=build /app/dist/* /usr/share/nginx/html/

# Expose port 80 for the NGINX web server
EXPOSE 80
EXPOSE 4200

# Start NGINX when the container starts
CMD ["nginx", "-g", "daemon off;"]

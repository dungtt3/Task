FROM node:22-alpine AS build
WORKDIR /app
COPY src/Frontend/package.json src/Frontend/package-lock.json ./
RUN npm ci
COPY src/Frontend/ .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY src/Frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

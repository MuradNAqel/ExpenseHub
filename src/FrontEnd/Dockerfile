FROM node:24-alpine AS build
WORKDIR /app

COPY src/FrontEnd/package*.json ./
RUN npm ci

COPY src/FrontEnd/ ./
RUN npm run build-only

FROM nginx:1.29-alpine
COPY src/FrontEnd/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

# Этап сборки
FROM node:18-alpine AS build
WORKDIR /app

# Копируем файлы зависимостей
COPY follow-me/follow-me-admin/package.json follow-me/follow-me-admin/package-lock.json* ./

# Устанавливаем зависимости
RUN npm install

# Копируем файлы исходного кода
COPY follow-me/follow-me-admin/ ./

# Собираем приложение
RUN npm run build

# Этап продакшена
FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html

# Копируем конфигурацию nginx
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf

# Копируем собранное приложение из этапа сборки
COPY --from=build /app/build /usr/share/nginx/html

# Порт, который будет использоваться
EXPOSE 80

# Запускаем nginx
CMD ["nginx", "-g", "daemon off;"]

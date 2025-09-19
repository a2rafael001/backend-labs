# Лабораторная работа №1 — WebAPI + PostgreSQL + Docker

##  Описание
Простейший сервис заказов (**Orders API**) на ASP.NET Core 9.0.  
Хранение данных — PostgreSQL, доступ через pgbouncer.  
Сервис завернут в Docker и запускается через `docker compose`.


##  Запуск

1. Собрать проект:
   ```bash
   dotnet publish WebApi/WebApi.csproj -c Release -o publish
   docker build -t universelabs-webapi:1.0.0 -f WebApi/Dockerfile.runtime .

2. Запустить сервисы:

docker compose up -d

3. Проверить здоровье API:

curl http://localhost:5113/api/health/ping

## Основные endpoints

POST /api/v1/order/batch-create — создание заказов

POST /api/v1/order/query — поиск заказов

GET /api/v1/order/{id} — получение заказа по id

PUT /api/v1/order/{id} — обновление заказа

DELETE /api/v1/order/{id} — удаление заказа

## Технологии

.NET 9

ASP.NET Core Web API

PostgreSQL + pgBouncer

Docker / Docker Compose

## Автор
Арутюнян Рафаэль Гарегинович, группа ФИ 42/2, 2025 год

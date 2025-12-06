# Backend Labs – Orders Service (.NET 9)

Учебный проект по курсу Backend-разработки.  
Сервис управляет заказами интернет-магазина и демонстрирует работу:

- с PostgreSQL через Dapper;
- с миграциями базы данных (FluentMigrator);
- с пулом подключений PgBouncer;
- с брокером сообщений RabbitMQ;
- с отдельным Consumer-сервисом;
- с Docker / Docker Compose.

Репозиторий использует структуру и требования из `universe-labs` (теги `1.0.0` и `2.0.0`).

---

## Структура решения

- **WebApp** – основной Web API (.NET 9)
  - контроллеры `V1/OrderController`
  - бизнес-логика `BLL/Services`
  - работа с БД через Dapper (`DAL/Repositories`)
  - фоновые задачи (`Jobs/OrderGenerator`)
  - валидация запросов через FluentValidation (`Validators`)
- **Models** – доменные модели и DTO (V1)
- **Migrations** – миграции FluentMigrator + консольный раннер
- **Common**, **Messages** – общие контракты и сообщения для RabbitMQ
- **Consumer** – отдельный worker-сервис, который подписывается на очередь RabbitMQ и обрабатывает события заказов
- **docker-compose.yml** – инфраструктура (Postgres, PgBouncer, RabbitMQ, WebApi, Consumer)

---

## Лабораторная работа 1 – REST API + PostgreSQL + Docker

### Функционал

1. **Схема БД (FluentMigrator)**  
   При старте приложения автоматически применяются миграции и создаются таблицы:

   - `orders`
   - `order_items`
   - `audit_log_order`
   - типы `v1_order`, `v1_order_item`, `v1_audit_log_order`

2. **Основные эндпоинты API**

   - `POST /v1/orders/batch`  
     Пакетное создание заказов с позициями (`order_items`).

   - `POST /v1/orders/query`  
     Фильтрация заказов по:
     - `ids`
     - `customer_ids`
     - пагинация `page`, `page_size`
     - опциональный флаг `include_order_items`.

   - `GET /v1/orders/{id}`  
     Получить заказ по идентификатору (с деталями при `includeOrderItems=true`).

   - `PUT /v1/orders/{id}`  
     Обновление данных заказа.

   - `DELETE /v1/orders/{id}`  
     Удаление заказа.

   - `GET /health`  
     Проверка здоровья сервиса.

3. **Валидация (FluentValidation)**

   - `V1CreateOrderRequestValidator`
     - список `orders` не пустой;
     - `CustomerId > 0`;
     - не пустой `DeliveryAddress`;
     - `TotalPriceCents > 0`;
     - `OrderItems` не пустой;
     - `TotalPriceCents` равен сумме `PriceCents * Quantity` по всем позициям;
     - все `PriceCurrency` в позициях одинаковые и совпадают с `TotalPriceCurrency`.

   - `V1QueryOrdersRequestValidator`
     - все `Ids` и `CustomerIds` > 0;
     - `Page > 0`, `PageSize > 0` (если заданы);
     - хотя бы одно из полей `Ids` или `CustomerIds` должно быть заполнено.

4. **Инфраструктура (Docker)**

   В `docker-compose.yml` поднимаются:

   - `postgres:16` – основная база данных;
   - `public.ecr.aws/bitnami/pgbouncer:1.24.1` – пул подключений к Postgres;
   - `backend_labs-…-webapi` – Web API (.NET 9);
   - (для ЛР2) `rabbitmq:3.13-management-alpine` – брокер сообщений;
   - (для ЛР2) `backend_labs-…-consumer` – worker-consumer.

   Внутри Docker-сети Web API и Consumer ходят в БД по строке подключения вида:

   ```text
   Host=pgbouncer;Port=6432;Database=postgres;Username=user;Password=mypassword


   Лабораторная работа 2 – RabbitMQ и Consumer

Вторая лабораторная расширяет проект асинхронным взаимодействием.

Что реализовано

RabbitMQ в docker-compose

сервис rabbitmq (порт 5672 – AMQP, 15672 – веб-интерфейс);

в переменных окружения Web API и Consumer задаются настройки RabbitMQ__Host, RabbitMQ__Port.

Публикация событий из Web API

после успешного создания заказов в POST /v1/orders/batch Web API формирует сообщения (например, OrderCreated) и публикует их в очередь RabbitMQ (по методичке – очередь oms.order.created);

для сообщений используются контракты из проекта Messages.

Сервис Consumer

отдельный .NET-worker (Consumer), запускаемый через Docker;

подписывается на очередь RabbitMQ;

читает сообщения о созданных заказах и выполняет обработку (логирование, возможная работа с таблицей audit_log_order – по требованиям методички);

конфигурация Consumer также вынесена в docker-compose.yml.

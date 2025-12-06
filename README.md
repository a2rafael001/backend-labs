Backend labs – Orders Service

Репозиторий для лабораторных работ по backend-разработке (.NET 9, PostgreSQL, RabbitMQ).
Реализован сервис заказов (Orders API) и отдельный consumer для обработки сообщений из очереди.

Технологический стек

.NET 9, ASP.NET Core Web API

PostgreSQL 16 + PgBouncer

RabbitMQ 3.13 (management UI)

Dapper

FluentMigrator

FluentValidation

Docker / Docker Compose

Swagger (Swashbuckle)

Структура решения

Common/ — общие модели и вспомогательный код, используемый в WebAPI и Consumer.

Messages/ — контракты сообщений, отправляемых в RabbitMQ.

Models/ — модели доступа к данным (Dapper).

Migrations/ — миграции базы данных (FluentMigrator).

WebApp/ — основной Web API сервис:

контроллеры (Controllers/V1/OrderController.cs);

бизнес-логика (BLL/Services/OrderService.cs);

фоновые задания (Jobs/OrderGenerator.cs);

валидаторы входных моделей (Validators/).

Consumer/ — отдельный сервис-consumer для чтения сообщений из очереди RabbitMQ.

docker-compose.yml — описание всего окружения (Postgres, PgBouncer, RabbitMQ, WebAPI, Consumer).

postgresql.conf — конфигурация PostgreSQL.

Лабораторная работа 1. Orders WebAPI + PostgreSQL
Цель

Создать REST API для работы с заказами, подключить PostgreSQL, настроить миграции и валидацию входных данных.

Схема БД

Миграции создают:

таблицу orders;

таблицу order_items;

таблицу audit_log_order (для логирования операций с заказами);

служебную таблицу version_info и типы v1_order, v1_order_item, v1_audit_log_order для работы с FluentMigrator.

Миграции выполняются автоматически при старте WebApp (это видно в логах контейнера: создаются таблицы и индексы).

Основные эндпоинты

Базовый маршрут контроллера: api/v1/order.

POST /api/v1/order/batch-create

Принимает массив заказов.

Для каждого заказа создаются строки в таблицах orders и order_items.

Ответ содержит созданные заказы с присвоенными id и метаданными (created_at, updated_at).

POST /api/v1/order/query

Поиск заказов по фильтрам:

ids — список ID заказов;

customer_ids — список ID клиентов;

постраничный вывод: page, page_size;

include_order_items — включать ли позиции заказа.

Возвращает список заказов (опционально вместе с order_items).

Валидация запросов

Используется FluentValidation.

V1CreateOrderRequestValidator

Orders — не пустой массив.

Для каждого заказа:

CustomerId > 0;

DeliveryAddress не пустая строка;

TotalPriceCents > 0;

TotalPriceCurrency не пустая строка;

OrderItems не пустой массив.

Для каждого товара (OrderItemValidator):

ProductId > 0;

Quantity > 0;

PriceCents > 0;

PriceCurrency, ProductTitle не пустые.

Дополнительные правила:

TotalPriceCents должен совпадать с суммой OrderItems.PriceCents * Quantity;

валюта всех позиций должна быть одинаковой;

валюта позиций должна совпадать с TotalPriceCurrency.

V1QueryOrdersRequestValidator

Все Ids и CustomerIds должны быть > 0;

Page и PageSize (если заданы) > 0;

Должен быть указан хотя бы один фильтр: Ids или CustomerIds.

При нарушении правил API возвращает 400 Bad Request с детальным описанием ошибок.

Лабораторная работа 2. RabbitMQ + Consumer
Цель

Добавить обмен сообщениями между сервисами через RabbitMQ и вынести часть логики в отдельный consumer.

Что сделано

В docker-compose.yml добавлен сервис rabbitmq (порт 5672 для приложений и 15672 для web-интерфейса).

Web API отправляет сообщения о созданных заказах в очередь RabbitMQ (контракты лежат в проекте Messages).

Проект Consumer:

подключается к тому же PostgreSQL через PgBouncer;

слушает очередь RabbitMQ;

обрабатывает сообщения (логирует/записывает данные в таблицу audit_log_order).

Таким образом:

WebApp отвечает за HTTP-API и создание заказов;

Consumer — за асинхронную обработку событий.

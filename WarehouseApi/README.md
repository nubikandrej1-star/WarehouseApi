# WarehouseApi — Pure CQRS (.NET 10)

У проєкті реалізовано Pure CQRS на звичайних C#-інтерфейсах `ICommand`, `ICommandHandler`, `IQuery` та `IQueryHandler`. Хендлери реєструються через `AddScoped`; нова бізнес-логіка не використовує MediatR.

## Запуск

1. Відкрийте `WarehouseApi.csproj` у Visual Studio 2026 / .NET 10 SDK.
2. Переконайтеся, що доступний SQL Server LocalDB, налаштований у `appsettings.json`.
3. Запустіть проєкт із профілем `https` або виконайте `dotnet run`.
4. У Development середовищі відкрийте Scalar: `https://localhost:56581/scalar/v1`.

Під час запуску `WarehouseDataSeeder` виконує `Database.MigrateAsync()`, тому нова міграція `AddSuppliersTable` застосовується автоматично. Її також можна застосувати вручну з папки проєкту командою `dotnet ef database update`.

## Ендпоінти товарів

- `GET /api/pure-cqrs-demo?searchTerm=dell&category=Electronics&onlyInStock=true` — пошук за назвою або SKU, категорією та залишком.
- `GET /api/pure-cqrs-demo/{id}` — товар за ID.
- `POST /api/pure-cqrs-demo` — створити товар. Поля команди: `name`, `sku`, `description`, `price`, `stock`, `category`.
- `PATCH /api/pure-cqrs-demo/{id}/stock` — змінити залишок. Тіло: `{"quantityChange": -2, "reason": "Внутрішнє списання"}`.
- `DELETE /api/pure-cqrs-demo/{id}` — видалити товар і пов'язані транзакції (згідно з налаштованим каскадним видаленням).

Для `onlyInStock=true` повертаються лише товари з `StockQuantity > 0`. Якщо параметр не задано або він дорівнює `false`, фільтр залишку не застосовується. Для постачальників `onlyActive=true` показує лише активних, а `false` вимикає цей фільтр.

## Ендпоінти постачальників і поставок

- `GET /api/suppliers?searchTerm=41234567&onlyActive=true` — список з пошуком за назвою або кодом. `onlyActive` за замовчуванням `true`; `onlyActive=false` вимикає фільтр і показує постачальників будь-якого статусу.
- `GET /api/suppliers/{id}` — постачальник за ID.
- `POST /api/suppliers` — створити постачальника.
- `POST /api/suppliers/restock` — оформити поставку товару.

Приклад тіла створення постачальника:

```json
{
  "companyName": "ТОВ Електропостач",
  "edrpouCode": "41234567",
  "contactPerson": "Олена Коваль",
  "phone": "+380501234567",
  "email": "office@example.com",
  "address": "м. Київ, вул. Прикладна, 10"
}
```

Приклад тіла оформлення поставки:

```json
{
  "supplierId": 1,
  "productId": 1,
  "quantity": 25,
  "purchasePrice": 1200.00,
  "invoiceNumber": "INV-2026/09-44"
}
```

Поставка приймається лише від активного постачальника, кількість має бути більшою за нуль, а закупівельна ціна не може бути від'ємною. Зміна залишку товару й новий запис `StockTransaction` зберігаються одним викликом `SaveChangesAsync`.

## Міграція

`20261013153000_AddSuppliersTable` створює таблицю `Suppliers`, додає унікальний індекс `IX_Suppliers_EdrpouCode` та збільшує поле `StockTransactions.Reason` до 1000 символів, щоб у ньому можна було зберігати назву постачальника, номер накладної й закупівельну ціну.


## Домашнє завдання 15.2: Shipments, MediatR та Vertical Slices

### Реалізовано

- `Domain/Entities/Shipment.cs`, конфігурація таблиці `Shipments`, унікальний індекс `TrackingNumber` і FK на `Products`.
- Міграція `20261013160000_AddShipmentsTable`.
- `CreateShipmentCommand` + `CreateShipmentValidator`: перевірка вхідних даних, контроль залишку, списання товару, запис `StockTransaction` та створення відвантаження.
- `UpdateShipmentStatusCommand`: перехід до `InTransit`, `Delivered` або `Cancelled`; для `Delivered` встановлюється `DeliveredAt`, для `Cancelled` залишок повертається на склад і додається окремий запис у журнал.
- `GetShipmentsListQuery` і `GetShipmentByIdQuery`: читання через `AsNoTracking()` та проекція в `ShipmentDto`.
- `ValidationBehavior<,>` запускає всі зареєстровані FluentValidation-валідатори перед хендлером.
- `ShipmentsController` використовує тільки `ISender` як сервісну залежність.
- `ApiExceptionMiddleware` повертає `400` для помилок валідації/бізнес-правил і `404` для `NotFoundException`.

### Запуск

З каталогу `WarehouseApi` виконайте:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Після запуску відкрийте Scalar UI за адресою, яку показує профіль запуску (зазвичай `https://localhost:56580/scalar/v1` або `https://localhost:56581/scalar/v1`). Seeder у `Program.cs` також викликає `Database.MigrateAsync()` під час запуску.

### Приклад створення відвантаження

```json
{
  "recipientName": "Olena Kovalenko",
  "destinationAddress": "Kyiv, Khreshchatyk Street 1",
  "productId": 1,
  "quantity": 2
}
```

Відповідь повертає `id` і `trackingNumber`. Для скасування надішліть `PUT /api/shipments/{id}/status` з тілом `{"newStatus":"Cancelled"}`. Відвантаження з кінцевим статусом `Delivered` або `Cancelled` не можна змінювати повторно, що запобігає повторному поверненню одного й того самого товару.

### HTTP-коди

- `201 Created` — відвантаження створене.
- `200 OK` — список/деталі або зміна статусу успішні.
- `400 Bad Request` — FluentValidation або бізнес-правило (наприклад, недостатньо товару).
- `404 Not Found` — товар або відвантаження не знайдені.

> Примітка: у цьому середовищі немає встановленого `dotnet` CLI, тому перед здачею слід виконати `dotnet build` та `dotnet ef database update` у локальному середовищі з .NET 10 SDK.

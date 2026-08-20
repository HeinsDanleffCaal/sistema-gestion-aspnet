# Sistema de Gestión

Aplicación web de gestión (productos, clientes y pedidos) construida con
**ASP.NET Core MVC 8**, **C#**, **SQL Server**, **Dapper** y **Bootstrap 5**.
Proyecto propio, hecho como práctica dirigida a reforzar el stack de
Visual Studio + SQL Server: autenticación con roles, procedimientos
almacenados con transacciones, y módulos front-end/back-end completos.

## Características

- **Autenticación por cookies con roles** (Administrador / Usuario), sin
  depender de ASP.NET Identity ni Entity Framework: el usuario y sus roles
  se manejan con tablas propias y Dapper.
- **Autorización granular** por controlador/acción con `[Authorize(Roles = "...")]`.
- **100% de acceso a datos vía procedimientos almacenados** (ADO.NET no se
  usa directo, todo pasa por Dapper -> stored procedures).
- **Transacciones reales en SQL Server**:
  - `sp_Usuario_Crear`: crea el usuario y le asigna el rol en una sola transacción.
  - `sp_Pedido_Crear`: crea el encabezado del pedido, inserta el detalle
    (vía un parámetro de tabla / TVP) y descuenta el stock de cada producto,
    todo dentro de `BEGIN TRANSACTION ... COMMIT`, con `ROLLBACK` automático
    (`SET XACT_ABORT ON`) si algo falla, por ejemplo stock insuficiente.
  - `sp_Pedido_Cancelar`: revierte el stock del pedido dentro de otra transacción.
- **Módulos CRUD completos**: Productos, Clientes, Pedidos y Usuarios.
- **UI con Bootstrap 5**, formularios con validación del lado del cliente
  y servidor, mensajes de éxito con TempData.

## Estructura del proyecto

```
SistemaGestion/
├── Database/                     Scripts SQL (tablas + procedimientos)
│   ├── 01_Schema.sql
│   ├── 02_StoredProcedures_Usuarios.sql
│   ├── 03_StoredProcedures_Productos.sql
│   ├── 04_StoredProcedures_Clientes.sql
│   └── 05_StoredProcedures_Pedidos.sql
├── SistemaGestion.sln
└── SistemaGestion/                Proyecto ASP.NET Core MVC
    ├── Controllers/
    ├── Models/                    Entidades de dominio
    ├── ViewModels/                Modelos de formularios
    ├── Data/                      Factory de conexión (Dapper)
    ├── Services/                  Capa de acceso a datos (Dapper + SPs)
    ├── Views/
    ├── wwwroot/
    ├── Program.cs
    └── appsettings.json
```

## Cómo ejecutarlo

### 1. Base de datos

Ejecuta, en orden, los scripts de la carpeta `Database/` contra tu instancia
de SQL Server (con SQL Server Management Studio o `sqlcmd`):

1. `01_Schema.sql` – crea la base `GestionDB`, las tablas y los roles base.
2. `02_StoredProcedures_Usuarios.sql`
3. `03_StoredProcedures_Productos.sql`
4. `04_StoredProcedures_Clientes.sql`
5. `05_StoredProcedures_Pedidos.sql`

### 2. Cadena de conexión

Ajusta `SistemaGestion/appsettings.json` con el nombre de tu instancia:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=GestionDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Restaurar y ejecutar

Desde Visual Studio: abre `SistemaGestion.sln`, restaura paquetes NuGet y
presiona F5.

O desde la terminal:

```bash
cd SistemaGestion
dotnet restore
dotnet run
```

### 4. Ingresar

La aplicación crea automáticamente, en el primer arranque, un usuario
administrador si todavía no existe ninguno:

- **Usuario:** `admin`
- **Contraseña:** `Admin123$`

Cambia esta contraseña (o crea otro administrador y desactiva este) antes
de usar el sistema en un entorno real.

## Flujo de la transacción de pedidos

1. El usuario arma el pedido (cliente + una o varias líneas de producto/cantidad).
2. `PedidoService.CrearAsync` construye un `DataTable` (tipo `dbo.DetallePedidoType`)
   y lo envía como parámetro con valores de tabla al procedimiento `sp_Pedido_Crear`.
3. El procedimiento, dentro de una transacción:
   - Valida que haya stock suficiente para **todas** las líneas.
   - Inserta el encabezado del pedido.
   - Inserta cada línea de detalle con el precio vigente del producto.
   - Descuenta el stock de cada producto.
   - Recalcula y guarda el total del pedido.
   - Si cualquier paso falla, revierte todo con `ROLLBACK`/`XACT_ABORT`.

## Tecnologías

- ASP.NET Core MVC (.NET 8)
- C#
- SQL Server (T-SQL, procedimientos almacenados, tipos de tabla)
- Dapper
- Bootstrap 5
- Autenticación por cookies (`Microsoft.AspNetCore.Authentication.Cookies`)

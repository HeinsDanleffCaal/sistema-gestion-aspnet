/* =====================================================================
   Sistema de Gestión - Script de creación de base de datos
   Motor: SQL Server
   ===================================================================== */

IF DB_ID('GestionDB') IS NULL
BEGIN
    CREATE DATABASE GestionDB;
END
GO

USE GestionDB;
GO

-- =======================
-- Tabla: Roles
-- =======================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id      INT IDENTITY(1,1) PRIMARY KEY,
        Nombre  NVARCHAR(50) NOT NULL UNIQUE
    );
END
GO

-- =======================
-- Tabla: Usuarios
-- =======================
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        NombreUsuario   NVARCHAR(50)  NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(300) NOT NULL,
        NombreCompleto  NVARCHAR(100) NOT NULL,
        Email           NVARCHAR(150) NOT NULL,
        Activo          BIT NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- =======================
-- Tabla: UsuarioRoles (N:M)
-- =======================
IF OBJECT_ID('dbo.UsuarioRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioRoles (
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES dbo.Usuarios(Id) ON DELETE CASCADE,
        RolId     INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
        PRIMARY KEY (UsuarioId, RolId)
    );
END
GO

-- =======================
-- Tabla: Productos
-- =======================
IF OBJECT_ID('dbo.Productos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Productos (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Nombre          NVARCHAR(120) NOT NULL,
        Descripcion     NVARCHAR(400) NULL,
        Precio          DECIMAL(10,2) NOT NULL,
        Stock           INT NOT NULL DEFAULT 0,
        Activo          BIT NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- =======================
-- Tabla: Clientes
-- =======================
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Nombre          NVARCHAR(150) NOT NULL,
        Email           NVARCHAR(150) NULL,
        Telefono        NVARCHAR(30)  NULL,
        Direccion       NVARCHAR(250) NULL,
        FechaRegistro   DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- =======================
-- Tabla: Pedidos
-- =======================
IF OBJECT_ID('dbo.Pedidos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId    INT NOT NULL FOREIGN KEY REFERENCES dbo.Clientes(Id),
        UsuarioId    INT NOT NULL FOREIGN KEY REFERENCES dbo.Usuarios(Id),
        FechaPedido  DATETIME NOT NULL DEFAULT GETDATE(),
        Total        DECIMAL(10,2) NOT NULL DEFAULT 0,
        Estado       NVARCHAR(30) NOT NULL DEFAULT 'Completado'
    );
END
GO

-- =======================
-- Tabla: DetallePedidos
-- =======================
IF OBJECT_ID('dbo.DetallePedidos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallePedidos (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        PedidoId        INT NOT NULL FOREIGN KEY REFERENCES dbo.Pedidos(Id) ON DELETE CASCADE,
        ProductoId      INT NOT NULL FOREIGN KEY REFERENCES dbo.Productos(Id),
        Cantidad        INT NOT NULL,
        PrecioUnitario  DECIMAL(10,2) NOT NULL,
        Subtotal        DECIMAL(10,2) NOT NULL
    );
END
GO

-- =======================
-- Tipo de tabla para el detalle del pedido (usado como parámetro
-- del procedimiento almacenado que crea el pedido completo)
-- =======================
IF TYPE_ID('dbo.DetallePedidoType') IS NULL
BEGIN
    CREATE TYPE dbo.DetallePedidoType AS TABLE
    (
        ProductoId INT NOT NULL,
        Cantidad   INT NOT NULL
    );
END
GO

-- =======================
-- Datos semilla: roles y usuario administrador
-- Password del admin: Admin123$ (hash generado con PasswordHasher de ASP.NET Core)
-- Debes actualizar este hash ejecutando la app una vez y registrando
-- al admin desde /Account/Register, o generarlo con el propio hasher.
-- =======================
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Administrador')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Usuario')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Usuario');
GO

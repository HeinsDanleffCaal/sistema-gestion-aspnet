USE GestionDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, Precio, Stock, Activo, FechaCreacion
    FROM dbo.Productos
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, Precio, Stock, Activo, FechaCreacion
    FROM dbo.Productos
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Crear
    @Nombre      NVARCHAR(120),
    @Descripcion NVARCHAR(400) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @NuevoId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Productos (Nombre, Descripcion, Precio, Stock, Activo, FechaCreacion)
    VALUES (@Nombre, @Descripcion, @Precio, @Stock, 1, GETDATE());

    SET @NuevoId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Actualizar
    @Id          INT,
    @Nombre      NVARCHAR(120),
    @Descripcion NVARCHAR(400) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Productos
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock
    WHERE Id = @Id;
END
GO

-- Eliminación lógica (soft delete), buena práctica cuando el producto
-- ya pudo haberse usado en pedidos anteriores.
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Productos SET Activo = 0 WHERE Id = @Id;
END
GO

USE GestionDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Email, Telefono, Direccion, FechaRegistro
    FROM dbo.Clientes
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Email, Telefono, Direccion, FechaRegistro
    FROM dbo.Clientes
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Crear
    @Nombre    NVARCHAR(150),
    @Email     NVARCHAR(150) = NULL,
    @Telefono  NVARCHAR(30)  = NULL,
    @Direccion NVARCHAR(250) = NULL,
    @NuevoId   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Clientes (Nombre, Email, Telefono, Direccion, FechaRegistro)
    VALUES (@Nombre, @Email, @Telefono, @Direccion, GETDATE());

    SET @NuevoId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Actualizar
    @Id        INT,
    @Nombre    NVARCHAR(150),
    @Email     NVARCHAR(150) = NULL,
    @Telefono  NVARCHAR(30)  = NULL,
    @Direccion NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Clientes
    SET Nombre = @Nombre, Email = @Email, Telefono = @Telefono, Direccion = @Direccion
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Clientes WHERE Id = @Id
    AND NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE ClienteId = @Id);
END
GO

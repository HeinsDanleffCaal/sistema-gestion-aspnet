USE GestionDB;
GO

-- =====================================================================
-- sp_Usuario_ObtenerPorNombre
-- Devuelve el usuario (con su hash) para validar el login en la app.
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerPorNombre
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, NombreUsuario, PasswordHash, NombreCompleto, Email, Activo, FechaCreacion
    FROM dbo.Usuarios
    WHERE NombreUsuario = @NombreUsuario;
END
GO

-- =====================================================================
-- sp_Usuario_ObtenerRoles
-- Devuelve los roles asignados a un usuario.
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerRoles
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT R.Nombre
    FROM dbo.UsuarioRoles UR
    INNER JOIN dbo.Roles R ON R.Id = UR.RolId
    WHERE UR.UsuarioId = @UsuarioId;
END
GO

-- =====================================================================
-- sp_Usuario_Listar
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.Id, U.NombreUsuario, U.NombreCompleto, U.Email, U.Activo, U.FechaCreacion,
           STRING_AGG(R.Nombre, ', ') AS RolesTexto
    FROM dbo.Usuarios U
    LEFT JOIN dbo.UsuarioRoles UR ON UR.UsuarioId = U.Id
    LEFT JOIN dbo.Roles R ON R.Id = UR.RolId
    GROUP BY U.Id, U.NombreUsuario, U.NombreCompleto, U.Email, U.Activo, U.FechaCreacion
    ORDER BY U.NombreCompleto;
END
GO

-- =====================================================================
-- sp_Usuario_Crear
-- Crea el usuario y le asigna un rol dentro de UNA TRANSACCIÓN,
-- para garantizar que nunca quede un usuario sin rol asignado.
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Crear
    @NombreUsuario  NVARCHAR(50),
    @PasswordHash   NVARCHAR(300),
    @NombreCompleto NVARCHAR(100),
    @Email          NVARCHAR(150),
    @RolId          INT,
    @NuevoUsuarioId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- cualquier error revierte automáticamente la transacción

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = @NombreUsuario)
    BEGIN
        RAISERROR('El nombre de usuario ya existe.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    INSERT INTO dbo.Usuarios (NombreUsuario, PasswordHash, NombreCompleto, Email, Activo, FechaCreacion)
    VALUES (@NombreUsuario, @PasswordHash, @NombreCompleto, @Email, 1, GETDATE());

    SET @NuevoUsuarioId = SCOPE_IDENTITY();

    INSERT INTO dbo.UsuarioRoles (UsuarioId, RolId)
    VALUES (@NuevoUsuarioId, @RolId);

    COMMIT TRANSACTION;
END
GO

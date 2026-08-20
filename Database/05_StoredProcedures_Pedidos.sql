USE GestionDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.Id, P.ClienteId, C.Nombre AS ClienteNombre,
           P.UsuarioId, U.NombreCompleto AS UsuarioNombre,
           P.FechaPedido, P.Total, P.Estado
    FROM dbo.Pedidos P
    INNER JOIN dbo.Clientes C ON C.Id = P.ClienteId
    INNER JOIN dbo.Usuarios U ON U.Id = P.UsuarioId
    ORDER BY P.FechaPedido DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pedido_ObtenerDetalle
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Id, P.ClienteId, C.Nombre AS ClienteNombre,
           P.UsuarioId, U.NombreCompleto AS UsuarioNombre,
           P.FechaPedido, P.Total, P.Estado
    FROM dbo.Pedidos P
    INNER JOIN dbo.Clientes C ON C.Id = P.ClienteId
    INNER JOIN dbo.Usuarios U ON U.Id = P.UsuarioId
    WHERE P.Id = @PedidoId;

    SELECT D.Id, D.PedidoId, D.ProductoId, PR.Nombre AS ProductoNombre,
           D.Cantidad, D.PrecioUnitario, D.Subtotal
    FROM dbo.DetallePedidos D
    INNER JOIN dbo.Productos PR ON PR.Id = D.ProductoId
    WHERE D.PedidoId = @PedidoId;
END
GO

-- =====================================================================
-- sp_Pedido_Crear
-- Procedimiento con TRANSACCIÓN: crea el encabezado del pedido, inserta
-- cada línea de detalle y descuenta el stock del producto. Si cualquier
-- paso falla (p. ej. stock insuficiente) TODO se revierte con ROLLBACK,
-- para no dejar datos inconsistentes entre las tres tablas afectadas.
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Crear
    @ClienteId  INT,
    @UsuarioId  INT,
    @Detalle    dbo.DetallePedidoType READONLY,
    @NuevoPedidoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM @Detalle)
    BEGIN
        RAISERROR('El pedido debe tener al menos un producto.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    -- Verifica que haya stock suficiente para TODAS las líneas antes de tocar nada
    IF EXISTS (
        SELECT 1
        FROM @Detalle D
        INNER JOIN dbo.Productos P ON P.Id = D.ProductoId
        WHERE P.Stock < D.Cantidad OR P.Activo = 0
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Stock insuficiente para uno o más productos del pedido.', 16, 1);
        RETURN;
    END

    -- Encabezado del pedido (el total se calcula y actualiza al final)
    INSERT INTO dbo.Pedidos (ClienteId, UsuarioId, FechaPedido, Total, Estado)
    VALUES (@ClienteId, @UsuarioId, GETDATE(), 0, 'Completado');

    SET @NuevoPedidoId = SCOPE_IDENTITY();

    -- Detalle del pedido, tomando el precio actual del producto
    INSERT INTO dbo.DetallePedidos (PedidoId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
    SELECT @NuevoPedidoId, D.ProductoId, D.Cantidad, P.Precio, (D.Cantidad * P.Precio)
    FROM @Detalle D
    INNER JOIN dbo.Productos P ON P.Id = D.ProductoId;

    -- Descuenta el stock de cada producto vendido
    UPDATE P
    SET P.Stock = P.Stock - D.Cantidad
    FROM dbo.Productos P
    INNER JOIN @Detalle D ON D.ProductoId = P.Id;

    -- Actualiza el total del pedido con la suma real del detalle
    UPDATE dbo.Pedidos
    SET Total = (SELECT SUM(Subtotal) FROM dbo.DetallePedidos WHERE PedidoId = @NuevoPedidoId)
    WHERE Id = @NuevoPedidoId;

    COMMIT TRANSACTION;
END
GO

-- =====================================================================
-- sp_Pedido_Cancelar
-- Revierte el stock de todos los productos del pedido y lo marca como
-- Cancelado, también dentro de una transacción.
-- =====================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Cancelar
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE Id = @PedidoId AND Estado = 'Completado')
    BEGIN
        RAISERROR('El pedido no existe o ya está cancelado.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    UPDATE P
    SET P.Stock = P.Stock + D.Cantidad
    FROM dbo.Productos P
    INNER JOIN dbo.DetallePedidos D ON D.ProductoId = P.Id
    WHERE D.PedidoId = @PedidoId;

    UPDATE dbo.Pedidos SET Estado = 'Cancelado' WHERE Id = @PedidoId;

    COMMIT TRANSACTION;
END
GO

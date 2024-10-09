CREATE DATABASE Reservaciones;
USE Reservaciones;
go

-- Tabla Clientes --
CREATE TABLE Clientes (
    ClienteID INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100),
    Email VARCHAR(100)
);
go

-- Tabla Habitaciones --
CREATE TABLE Habitaciones (
    HabitacionID INT PRIMARY KEY IDENTITY(1,1),
    TipoHabitacion VARCHAR(100),
    PrecioPorNoche DECIMAL(10, 2)
);
go

-- Tabla Reservas --
CREATE TABLE Reservas (
    ReservaID INT PRIMARY KEY IDENTITY(1,1),
    ClienteID INT FOREIGN KEY REFERENCES Clientes(ClienteID),
    HabitacionID INT FOREIGN KEY REFERENCES Habitaciones(HabitacionID),
    FechaEntrada DATETIME,
    FechaSalida DATETIME,
    Total DECIMAL(10, 2)
);
go

-- Obtener el total de ingresos generados por cliente --
SELECT C.Nombre, SUM(R.Total) AS TotalIngresos
FROM Reservas R
JOIN Clientes C ON R.ClienteID = C.ClienteID
GROUP BY C.Nombre;
go

-- Obtener las reservas activas (donde la fecha de salida es posterior a la fecha actual) --
SELECT R.ReservaID, C.Nombre, H.TipoHabitacion, R.FechaEntrada, R.FechaSalida
FROM Reservas R
JOIN Clientes C ON R.ClienteID = C.ClienteID
JOIN Habitaciones H ON R.HabitacionID = H.HabitacionID
WHERE R.FechaSalida > GETDATE();
go

-- Habitaciones Disponibles --
SELECT H.HabitacionID, H.TipoHabitacion, H.PrecioPorNoche
FROM Habitaciones H
LEFT JOIN Reservas R 
    ON H.HabitacionID = R.HabitacionID 
    AND R.FechaEntrada <= GETDATE() 
    AND R.FechaSalida >= GETDATE()
WHERE R.ReservaID IS NULL;
go

-- Procedimiento almacenado --
CREATE PROCEDURE RegistrarReserva
    @ClienteID INT,
    @HabitacionID INT,
    @FechaEntrada DATETIME,
    @FechaSalida DATETIME
AS
BEGIN
    DECLARE @PrecioPorNoche DECIMAL(10, 2);
    DECLARE @TotalNoches INT;
    DECLARE @Total DECIMAL(10, 2);

    -- Obtener el precio por noche de la habitación
    SELECT @PrecioPorNoche = PrecioPorNoche
    FROM Habitaciones
    WHERE HabitacionID = @HabitacionID;

    -- Calcular el total de noches
    SET @TotalNoches = DATEDIFF(DAY, @FechaEntrada, @FechaSalida);

    -- Calcular el total de la reserva
    SET @Total = @TotalNoches * @PrecioPorNoche;

    -- Insertar la nueva reserva
    INSERT INTO Reservas (ClienteID, HabitacionID, FechaEntrada, FechaSalida, Total)
    VALUES (@ClienteID, @HabitacionID, @FechaEntrada, @FechaSalida, @Total);
END;

-- INSERTS de Habitaciones --
INSERT INTO 
	Habitaciones (TipoHabitacion, PrecioPorNoche)
VALUES 
	('Estándar', 50),
	('Premium', 100),
	('Penthouse', 150);

-- INSERT Cliente --
INSERT INTO 
	Clientes (Nombre, Email)
VALUES
	('Jonathan Otoniel Guerra López', 'jguerra@gmail.com');
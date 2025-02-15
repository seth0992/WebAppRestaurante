CREATE DATABASE dbWebAppRestaurante;

USE dbWebAppRestaurante;


/*****************************************************/
/*                Crear las tablas                   */
/*****************************************************/
CREATE TABLE Products(
	ID INT PRIMARY KEY IDENTITY,
	ProductName NVARCHAR(250) NOT NULL UNIQUE,
	Quantity INT NOT NULL,
	Price DECIMAL(10,2) NOT NULL,
	Description NVARCHAR(MAX),
	CreateAt DATETIME2(7) NOT NULL
);

CREATE TABLE Roles(
	ID INT IDENTITY PRIMARY KEY,
	RoleName NVARCHAR(250) NOT NULL
);

CREATE TABLE Users(
	ID INT PRIMARY KEY IDENTITY,	
	FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
	Email NVARCHAR(255) NOT NULL,
	Username NVARCHAR(250) NOT NULL,
	PasswordHash NVARCHAR(MAX) NOT NULL,
	IsActive BIT DEFAULT 1,
	CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME2(7) NULL
);

CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email
ON Users(Email) 
WHERE Email IS NOT NULL;

CREATE TABLE UserRoles(
	ID INT IDENTITY(1,1) NOT NULL,
	UserID INT NULL,
	RoleID INT NULL,
);


CREATE TABLE RefreshTokens(
	ID INT PRIMARY KEY IDENTITY,
	UserID INT,
	RefreshToken NVARCHAR(500)
);

/*****************************************************/
/*                Core. Restaurante                 */
/*****************************************************/
-- Tablas principales para comida rápida
CREATE TABLE Categories (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
	UpdatedAt DATETIME2
);

CREATE TABLE Ingredients (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    StockQuantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    UnitOfMeasure NVARCHAR(20) NOT NULL, -- g, kg, l, ml, unidad, etc.
    MinimumStock DECIMAL(10,2) NOT NULL DEFAULT 0,
    Cost DECIMAL(10,2) NOT NULL DEFAULT 0,
    LastRestockDate DATETIME2,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);

CREATE TABLE FastFoodItems (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CategoryID INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    IsCombo BIT DEFAULT 0,
    IsAvailable BIT DEFAULT 1,
    ImageUrl NVARCHAR(500),
    EstimatedPreparationTime INT, -- tiempo en minutos
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (CategoryID) REFERENCES Categories(ID)
);

CREATE TABLE ItemIngredients (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FastFoodItemID INT NOT NULL,
    IngredientID INT NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL,
    IsOptional BIT DEFAULT 0,
    CanBeExtra BIT DEFAULT 0,
    ExtraPrice DECIMAL(10,2) DEFAULT 0,
    FOREIGN KEY (FastFoodItemID) REFERENCES FastFoodItems(ID),
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID)
);

CREATE TABLE ComboDetails (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ComboID INT NOT NULL,
    ItemID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    FOREIGN KEY (ComboID) REFERENCES FastFoodItems(ID),
    FOREIGN KEY (ItemID) REFERENCES FastFoodItems(ID)
);

CREATE TABLE Orders (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(20) NOT NULL UNIQUE,
    UserID INT NOT NULL, -- Cajero que toma la orden
    OrderType NVARCHAR(20) NOT NULL, -- TakeOut, Delivery, Counter
    Status NVARCHAR(20) NOT NULL, -- Pending, InProgress, Ready, Delivered, Cancelled
    SubTotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    Tax DECIMAL(10,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(10,2) NOT NULL DEFAULT 0,
    Total DECIMAL(10,2) NOT NULL DEFAULT 0,
    CustomerName NVARCHAR(100),
    CustomerPhone NVARCHAR(20),
    DeliveryAddress NVARCHAR(500),
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (UserID) REFERENCES Users(ID)
);

CREATE TABLE OrderDetails (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ItemID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    SpecialInstructions NVARCHAR(200),
    FOREIGN KEY (OrderID) REFERENCES Orders(ID),
    FOREIGN KEY (ItemID) REFERENCES FastFoodItems(ID)
);

CREATE TABLE OrderItemCustomizations (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderDetailID INT NOT NULL,
    IngredientID INT NOT NULL,
    ModificationType NVARCHAR(20) NOT NULL, -- Remove, Extra, Double
    Quantity INT NOT NULL DEFAULT 1,
    ExtraPrice DECIMAL(10,2) DEFAULT 0,
    FOREIGN KEY (OrderDetailID) REFERENCES OrderDetails(ID),
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID)
);

CREATE TABLE InventoryTransactions (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    IngredientID INT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL, -- Purchase, Consumption, Adjustment, Loss
    Quantity DECIMAL(10,2) NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(500),
    TransactionDate DATETIME2 DEFAULT GETDATE(),
    UserID INT NOT NULL,
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID),
    FOREIGN KEY (UserID) REFERENCES Users(ID)
);

-- Tablas para expansión futura (desactivadas inicialmente)
CREATE TABLE Tables (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TableNumber NVARCHAR(10) NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Available',
    IsActive BIT DEFAULT 0  -- Desactivado por defecto
);

CREATE TABLE Reservations (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TableID INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20),
    CustomerEmail NVARCHAR(100),
    ReservationDate DATETIME2 NOT NULL,
    Duration INT NOT NULL DEFAULT 120,
    GuestCount INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedByUserID INT NOT NULL,
    IsActive BIT DEFAULT 0,  -- Desactivado por defecto
    FOREIGN KEY (TableID) REFERENCES Tables(ID),
    FOREIGN KEY (CreatedByUserID) REFERENCES Users(ID)
);

-- Índices
CREATE INDEX IX_Ingredients_Name ON Ingredients(Name);
CREATE INDEX IX_FastFoodItems_CategoryID ON FastFoodItems(CategoryID);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_OrderDetails_OrderID ON OrderDetails(OrderID);
CREATE INDEX IX_InventoryTransactions_IngredientID ON InventoryTransactions(IngredientID);


/*****************************************************/
/*                Datos Iniciales                   */
/*****************************************************/

-- Insertar roles básicos
INSERT INTO [dbo].[Roles] ([RoleName]) 
VALUES ('SuperAdmin'), ('Admin'), ('User');

-- Crear usuario SuperAdmin (password: Admin123!)
-- La contraseña se actualizará después con el hash correcto
INSERT INTO [dbo].[Users] 
([Username], [PasswordHash], [FirstName], [LastName], [Email]) 
VALUES 
('superadmin', '$2a$11$XEyJPaiE7dT2u3UnS4MGOOyXeH4.bosU3k/nJ9.TgJBWoCJh7w6ge', 'Super', 'Admin', 'admin@restaurant.com');
--Admin123!

-- Asignar rol SuperAdmin
INSERT INTO [dbo].[UserRoles] ([UserID], [RoleID])
SELECT u.ID, r.ID 
FROM [dbo].[Users] u
CROSS JOIN [dbo].[Roles] r
WHERE u.Username = 'superadmin' 
AND r.RoleName = 'SuperAdmin';

select * from Users
select * from UserRoles
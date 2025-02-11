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
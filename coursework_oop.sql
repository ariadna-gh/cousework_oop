CREATE DATABASE IF NOT EXISTS coursework_oop;
USE coursework_oop;

CREATE TABLE owners (
    OwnerId INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Phone VARCHAR(30),
    Address VARCHAR(255)
);

CREATE TABLE vehicles (
    VehicleId INT AUTO_INCREMENT PRIMARY KEY,
    RegistrationNumber VARCHAR(100),
    Make VARCHAR(100),
    Model VARCHAR(100),
    VehicleType INT,
    OwnerId INT,
    NeedsRepair BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (OwnerId) REFERENCES owners(OwnerId) ON DELETE SET NULL
);

CREATE TABLE maintenance_records (
    MaintenanceId INT AUTO_INCREMENT PRIMARY KEY,
    VehicleId INT NOT NULL,
    ServiceDate DATE NOT NULL,
    Description TEXT,
    NextDueDate DATE,
    FOREIGN KEY (VehicleId) REFERENCES vehicles(VehicleId) ON DELETE CASCADE
);

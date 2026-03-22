CREATE DATABASE IF NOT EXISTS tourdb;
USE tourdb;

CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) CHARACTER SET utf8mb4 UNIQUE,
    Password VARCHAR(255)
	);

CREATE TABLE Locations (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) CHARACTER SET utf8mb4,
    Description TEXT,
    Image VARCHAR(255),
    Audio VARCHAR(255),
    Latitude DOUBLE,
    Longitude DOUBLE
);

INSERT INTO Locations (Name, Description, Image, Audio, Latitude, Longitude) 
VALUES 
('Ốc Vĩnh Khánh', 'Phố Vĩnh Khánh nổi tiếng với các món ốc...', 'oc.jpg', 'oc.mp3', 10.7593, 106.7046),
('Phá lấu Vĩnh Khánh', 'Phá lấu là món ăn đường phố nổi tiếng...', 'phalau.jpg', 'phalau.mp3', 10.7596, 106.7049),
('Chè Vĩnh Khánh', 'Khu phố có nhiều quán chè...', 'che.jpg', 'che.mp3', 10.7599, 106.7052),
('Hải sản nướng', 'Hải sản nướng...', 'seafood.jpg', 'seafood.mp3', 10.7601, 106.7053);


SELECT * FROM Locations;
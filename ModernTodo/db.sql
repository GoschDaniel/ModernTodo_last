USE master
GO
CREATE DATABASE AAPY_DB_Todo
GO

USE AAPY_DB_Todo
GO

CREATE TABLE AAPY_Role
(
    Id INT PRIMARY KEY IDENTITY,
    RoleName VARCHAR(20) NOT NULL UNIQUE
);

INSERT INTO AAPY_Role (RoleName) VALUES
    ('Free-Tier'),
    ('Premium-Tier'),
    ('Admin');

CREATE TABLE AAPY_User
(
    Id INT PRIMARY KEY IDENTITY,
    Mail VARCHAR(50) NOT NULL UNIQUE,
    Username VARCHAR(50) NOT NULL,
    PasswordHash BINARY(32) NOT NULL,
    Salt BINARY(32) NOT NULL,
    RegisteredOn DATETIME NOT NULL,
    RoleId INT NOT NULL DEFAULT(1),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES AAPY_Role(Id)
);

CREATE TABLE AAPY_ToDoList
(
    Id INT PRIMARY KEY IDENTITY,
    Name VARCHAR(50) NOT NULL,
    UserId INT NOT NULL,
    CONSTRAINT FK_UserId FOREIGN KEY (UserId) REFERENCES AAPY_User(Id)
);

CREATE TABLE AAPY_Task
(
    Id INT PRIMARY KEY IDENTITY,
    [Description] VARCHAR(100) NOT NULL,
    ToDoListId INT NOT NULL,
    CONSTRAINT Fk_ToDoList FOREIGN KEY (ToDoListId) REFERENCES AAPY_ToDoList(Id),
    CreatedOn DATETIME NOT NULL,
    CompletedOn DATETIME NULL,
    Deadline DATETIME NULL,
    Priority INT NOT NULL
);

CREATE TABLE AAPY_Tags
(
    Id INT PRIMARY KEY IDENTITY,
    Name VARCHAR(50) NOT NULL,
    HexColor CHAR(6) NOT NULL,
    UserId INT
);

CREATE TABLE AAPY_TaskTags
(
    TaskId INT,
    TagsId INT,
    CONSTRAINT Fk_Task FOREIGN KEY (TaskId) REFERENCES AAPY_Task(Id) ON DELETE CASCADE,
    CONSTRAINT Fk_Tags FOREIGN KEY (TagsId) REFERENCES AAPY_Tags(Id),
    CONSTRAINT PK_Tags_Todo PRIMARY KEY (TaskId, TagsId)
);

ALTER TABLE AAPY_TaskTags
DROP CONSTRAINT Fk_Task;

ALTER TABLE AAPY_TaskTags
ADD CONSTRAINT Fk_Task FOREIGN KEY (TaskId) REFERENCES AAPY_Task(Id) ON DELETE CASCADE;



DECLARE @ServerName NVARCHAR(128) = @@SERVERNAME;
DECLARE @DBName NVARCHAR(128) = 'AAPY_DB_Todo';

PRINT 'Server=' + @ServerName + ';Database=' + @DBName + ';Integrated Security=True;';
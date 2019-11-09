CREATE TABLE [dbo].[Users]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Status] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
	[CreatedBy] UNIQUEIDENTIFIER NULL, 
    [ModifiedDate] DATETIME NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NULL,
    [Username] NVARCHAR(32) NOT NULL,
	[Email] NVARCHAR(256) NOT NULL,
	[PasswordHash] NVARCHAR(256) NOT NULL, 
    [PasswordSalt] NVARCHAR(256) NOT NULL,
	CONSTRAINT [FK_Users_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Users_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id])
)

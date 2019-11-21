CREATE TABLE [dbo].[Users]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Status] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
	[CreatedBy] UNIQUEIDENTIFIER NOT NULL, 
    [ModifiedDate] DATETIME NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NOT NULL,
    [Username] NVARCHAR(32) NOT NULL,
	[Email] NVARCHAR(256) NOT NULL,
	[AvatarFileExtension] VARCHAR(5) NULL, 
	CONSTRAINT [UC_Users] UNIQUE (Username),
    CONSTRAINT [FK_Users_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Users_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id])
)

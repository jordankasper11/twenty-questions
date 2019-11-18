CREATE TABLE [dbo].[UserCredentials]
(
	[UserId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ModifiedDate] DATETIME NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NOT NULL,
	[PasswordHash] NVARCHAR(256) NOT NULL, 
    [PasswordSalt] NVARCHAR(256) NOT NULL,
	[RefreshToken] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_UserCredentials_ToUsersId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_UserCredentials_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id])
)

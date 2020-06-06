CREATE TABLE [dbo].[Friends]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Status] INT NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL, 
	[CreatedBy] UNIQUEIDENTIFIER NOT NULL, 
    [ModifiedDate] DATETIME2 NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NOT NULL,
	[FriendId] UNIQUEIDENTIFIER NOT NULL,
	CONSTRAINT [FK_Friends_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Friends_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Friends_ToUsersFriendId] FOREIGN KEY ([FriendId]) REFERENCES [Users]([Id])
)

CREATE TABLE [dbo].[Games]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Status] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
	[CreatedBy] UNIQUEIDENTIFIER NOT NULL, 
    [ModifiedDate] DATETIME NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NOT NULL,
    [Subject] NVARCHAR(256) NOT NULL,
	[OpponentId] UNIQUEIDENTIFIER NOT NULL,
	[MaxQuestions] INT NOT NULL,
	[Completed] BIT NOT NULL,
	[Questions] NVARCHAR(MAX),
	CONSTRAINT [FK_Games_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Games_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Games_ToUsersOpponentId] FOREIGN KEY ([OpponentId]) REFERENCES [Users]([Id])
)

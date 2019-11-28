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
	[AvatarFileName] NVARCHAR(256) NULL, 
	CONSTRAINT [UC_Users] UNIQUE (Username),
    CONSTRAINT [FK_Users_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Users_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id])
)
GO

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
GO

CREATE TABLE [dbo].[Friends]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Status] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
	[CreatedBy] UNIQUEIDENTIFIER NOT NULL, 
    [ModifiedDate] DATETIME NOT NULL, 
    [ModifiedBy] UNIQUEIDENTIFIER NOT NULL,
	[FriendId] UNIQUEIDENTIFIER NOT NULL,
	CONSTRAINT [FK_Friends_ToUsersCreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Friends_ToUsersModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [Users]([Id]),
	CONSTRAINT [FK_Friends_ToUsersFriendId] FOREIGN KEY ([FriendId]) REFERENCES [Users]([Id])
)
GO

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
GO

CREATE TYPE [dbo].[EntityIdsType] AS TABLE (
	Id	UNIQUEIDENTIFIER
)
GO

CREATE PROCEDURE [dbo].[Friend_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@Status			INT = NULL,
	@UserId			UNIQUEIDENTIFIER,
	@FriendId		UNIQUEIDENTIFIER = NULL,
	@OrderBy		NVARCHAR(64) = 'Username ASC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	DECLARE @Friends TABLE (
		Id					UNIQUEIDENTIFIER,
		FriendId			UNIQUEIDENTIFIER,
		Username			NVARCHAR(32),
		AvatarFileName		VARCHAR(256)
	)

	INSERT INTO @Friends (Id, FriendId, Username, AvatarFileName)
		SELECT	F.Id,
				CASE WHEN F.CreatedBy = @UserId THEN F.FriendId ELSE F.CreatedBy END,
				CASE WHEN F.CreatedBy = @UserId THEN U2.Username ELSE U1.Username END,
				CASE WHEN F.CreatedBy = @UserId THEN U2.AvatarFileName ELSE U1.AvatarFileName END
		FROM	Friends AS F
				INNER JOIN Users AS U1 ON U1.Id = F.CreatedBy
				INNER JOIN Users AS U2 ON U2.Id = F.FriendId
		WHERE	F.CreatedBy = @UserId OR
				F.FriendId = @UserId

	SELECT		F.Id, F.[Status], F.CreatedBy, F.CreatedDate, F.ModifiedBy, F.ModifiedDate,
				T.FriendId, T.Username, T.AvatarFileName,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'Username ASC' THEN T.Username END ASC,
					CASE WHEN @OrderBy = 'Username DESC' THEN T.Username END DESC
				) AS RowNumber
	INTO		#Friends
	FROM		@Friends AS T
				INNER JOIN Friends AS F ON F.Id = T.Id
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND F.[Status] = 1) OR (@Status IS NOT NULL AND F.[Status] & @Status > 0)) AND
				(@Id IS NULL OR F.Id = @Id) AND
				(@FilterIds = 0 OR F.Id IN (SELECT Id FROM @Ids)) AND
				(@FriendId IS NULL OR T.FriendId = @FriendId)

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Friends
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO

CREATE PROCEDURE [dbo].[Friend_Save] (
	@Id				UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status			INT,
    @ModifiedBy		UNIQUEIDENTIFIER = NULL,
	@ModifiedDate	DATETIME = NULL,
	@FriendId		UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedBy IS NULL)
		SET @ModifiedBy = @Id

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Friends WHERE Id = @Id))
	BEGIN
		UPDATE	Friends
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		IF @FriendId IS NULL
			THROW 51000, '@FriendId cannot be null', 1;

		INSERT INTO Friends (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, FriendId)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @FriendId)
	END
END
GO

CREATE PROCEDURE [dbo].[Game_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@Status			INT = NULL,
	@UserId			UNIQUEIDENTIFIER,
	@Completed		BIT = NULL,
	@OrderBy		NVARCHAR(64) = 'ModifiedDate DESC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		G.Id, G.[Status], G.CreatedBy, G.CreatedDate, G.ModifiedBy, G.ModifiedDate, G.OpponentId,
				CASE WHEN G.CreatedBy = @UserId THEN G.[Subject] ELSE NULL END AS [Subject],
				CASE WHEN G.CreatedBy = @UserId THEN U2.Id ELSE U1.Id END AS FriendId,
				CASE WHEN G.CreatedBy = @UserId THEN U2.Username ELSE U1.Username END AS FriendUsername,
				CASE WHEN G.CreatedBy = @UserId THEN U2.AvatarFileName ELSE U1.AvatarFileName END AS FriendAvatarFileName,
				G.MaxQuestions, G.Completed, G.Questions,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'ModifiedDate ASC' THEN G.ModifiedDate END ASC,
					CASE WHEN @OrderBy = 'ModifiedDate DESC' THEN G.ModifiedDate END DESC
				) AS RowNumber
	INTO		#Games
	FROM		Games AS G
				INNER JOIN Users AS U1 ON U1.Id = G.CreatedBy
				INNER JOIN Users AS U2 ON U2.Id = G.OpponentId
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND G.[Status] = 1) OR (@Status IS NOT NULL AND G.[Status] & @Status > 0)) AND
				(@Id IS NULL OR G.Id = @Id) AND
				(@FilterIds = 0 OR G.Id IN (SELECT Id FROM @Ids)) AND
				(@UserId IS NULL OR G.CreatedBy = @UserId OR G.OpponentId = @UserId) AND
				(@Completed IS NULL OR G.Completed = @Completed) AND
				(@Completed IS NOT NULL OR (G.Completed = 0 OR G.ModifiedDate <= DATEADD(DAY, 7, GETUTCDATE())))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Games
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO

CREATE PROCEDURE [dbo].[Game_Save] (
	@Id					UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status				INT,
    @ModifiedBy			UNIQUEIDENTIFIER,
	@ModifiedDate		DATETIME = NULL,
	@OpponentId			UNIQUEIDENTIFIER,
	@Subject			NVARCHAR(256) = NULL,	
	@MaxQuestions		INT,
	@Completed			BIT,
	@Questions			NVARCHAR(MAX)
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Games WHERE Id = @Id))
	BEGIN
		UPDATE	Games
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				Completed = @Completed,
				Questions = @Questions
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		IF @Subject IS NULL
			THROW 51000, '@Subject cannot be null', 1;

		INSERT INTO Games (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, OpponentId, [Subject], MaxQuestions, Completed, Questions)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @OpponentId, @Subject, @MaxQuestions, @Completed, @Questions)
	END
END
GO

CREATE PROCEDURE [dbo].[User_Get] (
	@Id					UNIQUEIDENTIFIER = NULL,
	@Ids				EntityIdsType READONLY,
	@Status				INT = NULL,
	@Username			NVARCHAR(32) = NULL,
	@FriendSearchUserId	UNIQUEIDENTIFIER = NULL,
	@OrderBy			NVARCHAR(64) = 'Username ASC',
	@PageNumber			INT = 1,
	@PageSize			INT = 2147483647,
	@TotalRecords		INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate,
				Username, Email, AvatarFileName,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'Username ASC' THEN Username END ASC,
					CASE WHEN @OrderBy = 'Username DESC' THEN Username END DESC
				) AS RowNumber
	INTO		#Users
	FROM		Users
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND [Status] = 1) OR (@Status IS NOT NULL AND [Status] & @Status > 0)) AND
				(@Id IS NULL OR Id = @Id) AND
				(@FilterIds = 0 OR Id IN (SELECT Id FROM @Ids)) AND
				(@Username IS NULL OR Username = @Username) AND
				(@FriendSearchUserId IS NULL OR (Id != @FriendSearchUserId AND Id NOT IN (
					SELECT	CASE WHEN CreatedBy = @FriendSearchUserId THEN FriendId ELSE CreatedBy END
					FROM	Friends
					WHERE	(CreatedBy = @FriendSearchUserId OR FriendId = @FriendSearchUserId) AND
							[Status] != 8
				)))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Users
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO

CREATE PROCEDURE [dbo].[User_GetUsernameAvailability] (
	@UserId		UNIQUEIDENTIFIER = NULL,
	@Username	NVARCHAR(32)
)
AS
BEGIN
	DECLARE @UsernameAvailable BIT = 1

	IF EXISTS (
		SELECT	*
		FROM	Users
		WHERE	(@UserId IS NULL OR Id != @UserId) AND
				Username = @Username
	)
		SET @UsernameAvailable = 0

	SELECT @UsernameAvailable AS UsernameAvailable
END
GO

CREATE PROCEDURE [dbo].[User_Save] (
	@Id						UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status					INT,
    @ModifiedBy				UNIQUEIDENTIFIER = NULL,
	@ModifiedDate			DATETIME = NULL,
	@Username				NVARCHAR(32),
	@Email					NVARCHAR(256),
	@AvatarFileName			NVARCHAR(256) = NULL
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedBy IS NULL)
		SET @ModifiedBy = @Id

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Users WHERE Id = @Id))
	BEGIN
		UPDATE	Users
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				Username = @Username,
				Email = @Email,
				AvatarFileName = @AvatarFileName
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		INSERT INTO Users (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, Username, Email, AvatarFileName)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @Username, @Email, @AvatarFileName)
	END
END
GO

CREATE PROCEDURE [dbo].[UserCredentials_Get] (
	@UserId			UNIQUEIDENTIFIER = NULL,
	@Username		NVARCHAR(256) = NULL,
	@RefreshToken	NVARCHAR(MAX) = NULL
)
AS
BEGIN
	IF (@UserId IS NULL AND @Username IS NULL AND @RefreshToken IS NULL)
		THROW 51000, '@UserId, @Username, and @RefreshToken cannot all be null', 1;

	SELECT	U.Id AS UserId, U.Username, U.Email, UC.PasswordHash, UC.PasswordSalt, UC.RefreshToken
	FROM	Users AS U
			INNER JOIN UserCredentials AS UC ON UC.UserId = U.Id
	WHERE	U.[Status] = 1 AND
			(@UserId IS NULL OR U.Id = @UserId) AND
			(@Username IS NULL OR U.Username = @Username) AND
			(@RefreshToken IS NULL OR UC.RefreshToken = @RefreshToken)
END
GO

CREATE PROCEDURE [dbo].[UserCredentials_Save] (
	@UserId			UNIQUEIDENTIFIER,
	@ModifiedBy		UNIQUEIDENTIFIER = NULL,
	@ModifiedDate	DATETIME = NULL,
	@PasswordHash	NVARCHAR(256), 
    @PasswordSalt	NVARCHAR(256)
)
AS
BEGIN
	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT UserId FROM UserCredentials WHERE UserId = @UserId))
	BEGIN
		UPDATE	UserCredentials
		SET		ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				PasswordHash = @PasswordHash,
				PasswordSalt = @PasswordSalt
		WHERE	UserId = @UserId
	END
	ELSE
	BEGIN
		IF (@ModifiedBy IS NULL)
			SET @ModifiedBy = @UserId

		INSERT INTO UserCredentials (UserId, ModifiedBy, ModifiedDate, PasswordHash, PasswordSalt)
			VALUES (@UserId, @ModifiedBy, @ModifiedDate, @PasswordHash, @PasswordSalt)
	END
END
GO

CREATE PROCEDURE [dbo].[UserCredentials_SaveRefreshToken] (
	@UserId			UNIQUEIDENTIFIER,
	@RefreshToken	NVARCHAR(MAX) = NULL
)
AS
BEGIN
	UPDATE	UserCredentials
	SET		RefreshToken = @RefreshToken
	WHERE	UserId = @UserId
END
GO
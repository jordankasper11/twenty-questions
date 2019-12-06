CREATE PROCEDURE [dbo].[Notifications_Get]
	@UserId				UNIQUEIDENTIFIER,
	@GamesLastChecked	DATETIME = NULL
AS
BEGIN
	DECLARE @FriendNotifications BIT = CASE WHEN EXISTS (
		SELECT	Id
		FROM	Friends
		WHERE	FriendId = @UserId AND
				[Status] = 2
	) THEN 1 ELSE 0 END

	DECLARE @GameNotifications BIT = CASE WHEN EXISTS (
		SELECT	Id
		FROM	Games
		WHERE	(CreatedBy = @UserId OR OpponentId = @UserId) AND
				ModifiedBy != @UserId AND
				(Completed = 0 OR @GamesLastChecked IS NULL OR ModifiedDate > @GamesLastChecked) AND
				[Status] IN (1, 2)
	) THEN 1 ELSE 0 END

	SELECT	@FriendNotifications AS FriendNotifications,
			@GameNotifications AS GameNotifications
END
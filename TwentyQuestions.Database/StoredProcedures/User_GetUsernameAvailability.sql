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
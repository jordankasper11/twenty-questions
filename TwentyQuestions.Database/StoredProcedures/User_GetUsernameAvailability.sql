CREATE PROCEDURE [dbo].[User_GetUsernameAvailability] (
	@Username			NVARCHAR(32) = NULL
)
AS
BEGIN
	DECLARE @UsernameAvailable BIT = 1

	IF EXISTS (SELECT * FROM Users WHERE Username = @Username)
		SET @UsernameAvailable = 0

	SELECT @UsernameAvailable AS UsernameAvailable
END
GO
CREATE PROCEDURE [dbo].[User_GetCredentials] (
	@Username	NVARCHAR(256)
)
AS
BEGIN
	SELECT	Id, Username, Email, PasswordHash, PasswordSalt
	FROM	Users
	WHERE	[Status] = 1 AND
			Username = @Username
END
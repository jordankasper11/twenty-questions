CREATE PROCEDURE [dbo].[User_GetCredentials] (
	@Username		NVARCHAR(256) = NULL,
	@RefreshToken	NVARCHAR(MAX) = NULL
)
AS
BEGIN
	IF (@Username IS NULL AND @RefreshToken IS NULL)
		THROW 51000, '@Username and @RefreshToken cannot both be null', 1;

	SELECT	Id, Username, Email, PasswordHash, PasswordSalt, RefreshToken
	FROM	Users
	WHERE	[Status] = 1 AND
			(@Username IS NULL OR Username = @Username) AND
			(@RefreshToken IS NULL OR RefreshToken = @RefreshToken)
END
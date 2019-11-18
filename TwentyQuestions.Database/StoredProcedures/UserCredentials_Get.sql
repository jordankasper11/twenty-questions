CREATE PROCEDURE [dbo].[UserCredentials_Get] (
	@Username		NVARCHAR(256) = NULL,
	@RefreshToken	NVARCHAR(MAX) = NULL
)
AS
BEGIN
	IF (@Username IS NULL AND @RefreshToken IS NULL)
		THROW 51000, '@Username and @RefreshToken cannot both be null', 1;

	SELECT	U.Id AS UserId, U.Username, U.Email, UC.PasswordHash, UC.PasswordSalt, UC.RefreshToken
	FROM	Users AS U
			INNER JOIN UserCredentials AS UC ON UC.UserId = U.Id
	WHERE	U.[Status] = 1 AND
			(@Username IS NULL OR U.Username = @Username) AND
			(@RefreshToken IS NULL OR UC.RefreshToken = @RefreshToken)
END
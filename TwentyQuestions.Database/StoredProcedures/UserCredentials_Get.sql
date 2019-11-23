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
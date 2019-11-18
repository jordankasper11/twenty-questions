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
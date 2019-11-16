CREATE PROCEDURE [dbo].[User_SaveRefreshToken] (
	@Id					UNIQUEIDENTIFIER,
	@RefreshToken		NVARCHAR(MAX) = NULL
)
AS
BEGIN
	UPDATE	Users
	SET		RefreshToken = @RefreshToken
	WHERE	Id = @Id
END
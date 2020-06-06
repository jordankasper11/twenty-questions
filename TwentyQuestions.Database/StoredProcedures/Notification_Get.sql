CREATE PROCEDURE [dbo].[Notifications_Get]
	@UserId	UNIQUEIDENTIFIER
AS
BEGIN
	SELECT	Id, UserId, CreatedDate, [Type], RecordId
	FROM	Notifications
	WHERE	UserId = @UserId
END
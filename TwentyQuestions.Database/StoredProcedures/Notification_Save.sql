CREATE PROCEDURE [dbo].[Notification_Save] (
	@Id				UNIQUEIDENTIFIER = NULL OUTPUT,
	@UserId			UNIQUEIDENTIFIER,
	@Type			INT,
	@RecordId		UNIQUEIDENTIFIER = NULL,
	@CreatedDate	DATETIME2 = NULL
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@CreatedDate IS NULL)
		SET @CreatedDate = GETUTCDATE()

	INSERT INTO Notifications(Id, UserId, CreatedDate, [Type], RecordId)
		VALUES (@Id, @UserId, @CreatedDate, @Type, @RecordId)

	SELECT	Id, UserId, [Type], RecordId, CreatedDate
	FROM	Notifications
	WHERE	Id = @Id
END
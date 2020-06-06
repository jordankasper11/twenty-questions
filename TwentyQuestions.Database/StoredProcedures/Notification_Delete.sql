CREATE PROCEDURE [dbo].[Notification_Delete] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@UserId			UNIQUEIDENTIFIER = NULL,
	@Type			INT = NULL,
	@RecordId		UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	IF @Id IS NULL AND @UserId IS NULL AND @Type IS NULL AND @RecordId IS NULL
		THROW 51000, '@Id, @UserId, @Type, and @RecordId cannot all be null', 1;

	DECLARE @Notifications TABLE (
		Id			UNIQUEIDENTIFIER,
		UserId		UNIQUEIDENTIFIER,
		[Type]		INT,
		RecordId	UNIQUEIDENTIFIER,
		CreatedDate	DATETIME2
	)

	INSERT INTO @Notifications (Id, UserId, [Type], RecordId, CreatedDate)
		SELECT	Id, UserId, [Type], RecordId, CreatedDate
		FROM	Notifications
		WHERE	(@Id IS NULL OR Id = @Id) AND
				(@UserId IS NULL OR UserId = @UserId) AND
				(@Type IS NULL OR [Type] = @Type) AND
				(@RecordId IS NULL OR RecordId = @RecordId)

	DELETE	N
	FROM	Notifications AS N
			INNER JOIN @Notifications AS I ON I.Id = N.Id
	
	SELECT	*
	FROM	@Notifications
END
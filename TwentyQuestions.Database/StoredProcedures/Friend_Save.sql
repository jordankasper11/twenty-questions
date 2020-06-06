CREATE PROCEDURE [dbo].[Friend_Save] (
	@Id				UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status			INT,
    @ModifiedBy		UNIQUEIDENTIFIER = NULL,
	@ModifiedDate	DATETIME2 = NULL,
	@FriendId		UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedBy IS NULL)
		SET @ModifiedBy = @Id

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Friends WHERE Id = @Id))
	BEGIN
		UPDATE	Friends
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		IF @FriendId IS NULL
			THROW 51000, '@FriendId cannot be null', 1;

		INSERT INTO Friends (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, FriendId)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @FriendId)
	END
END
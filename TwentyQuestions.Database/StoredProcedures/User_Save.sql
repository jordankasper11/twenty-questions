CREATE PROCEDURE [dbo].[User_Save] (
	@Id						UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status					INT,
    @ModifiedBy				UNIQUEIDENTIFIER = NULL,
	@ModifiedDate			DATETIME = NULL,
	@Username				NVARCHAR(32),
	@Email					NVARCHAR(256),
	@AvatarFileName			NVARCHAR(256) = NULL
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedBy IS NULL)
		SET @ModifiedBy = @Id

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Users WHERE Id = @Id))
	BEGIN
		UPDATE	Users
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				Username = @Username,
				Email = @Email,
				AvatarFileName = @AvatarFileName
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		INSERT INTO Users (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, Username, Email, AvatarFileName)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @Username, @Email, @AvatarFileName)
	END
END

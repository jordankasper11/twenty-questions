CREATE PROCEDURE [dbo].[UserCredentials_Save] (
	@UserId			UNIQUEIDENTIFIER,
	@ModifiedBy		UNIQUEIDENTIFIER = NULL,
	@ModifiedDate	DATETIME = NULL,
	@PasswordHash	NVARCHAR(256), 
    @PasswordSalt	NVARCHAR(256)
)
AS
BEGIN
	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT UserId FROM UserCredentials WHERE UserId = @UserId))
	BEGIN
		UPDATE	UserCredentials
		SET		ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				PasswordHash = @PasswordHash,
				PasswordSalt = @PasswordSalt
		WHERE	UserId = @UserId
	END
	ELSE
	BEGIN
		IF (@ModifiedBy IS NULL)
			SET @ModifiedBy = @UserId

		INSERT INTO UserCredentials (UserId, ModifiedBy, ModifiedDate, PasswordHash, PasswordSalt)
			VALUES (@UserId, @ModifiedBy, @ModifiedDate, @PasswordHash, @PasswordSalt)
	END
END
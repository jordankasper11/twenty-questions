CREATE PROCEDURE [dbo].[Game_Save] (
	@Id					UNIQUEIDENTIFIER = NULL OUTPUT,
	@Status				INT,
    @ModifiedBy			UNIQUEIDENTIFIER,
	@ModifiedDate		DATETIME = NULL,
	@OpponentId			UNIQUEIDENTIFIER,
	@Subject			NVARCHAR(256),	
	@MaxQuestions		INT,
	@Completed			BIT,
	@Questions			NVARCHAR(MAX)
)
AS
BEGIN
	IF (@Id IS NULL)
		SET @Id = NEWID()

	IF (@ModifiedDate IS NULL)
		SET @ModifiedDate = GETUTCDATE()

	IF (EXISTS(SELECT Id FROM Games WHERE Id = @Id))
	BEGIN
		UPDATE	Games
		SET		[Status] = @Status,
				ModifiedBy = @ModifiedBy,
				ModifiedDate = @ModifiedDate,
				Completed = @Completed,
				Questions = @Questions
		WHERE	Id = @Id
	END
	ELSE
	BEGIN
		INSERT INTO Games (Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, OpponentId, [Subject], MaxQuestions, Completed, Questions)
			VALUES (@Id, @Status, @ModifiedBy, @ModifiedDate, @ModifiedBy, @ModifiedDate, @OpponentId, @Subject, @MaxQuestions, @Completed, @Questions)
	END
END

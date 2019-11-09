CREATE PROCEDURE [dbo].[Game_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@OrderBy		NVARCHAR(64) = 'ModifiedDate DESC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate,
				[Subject], OpponentId, MaxQuestions, Completed, Questions,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'ModifiedDate ASC' THEN ModifiedDate END ASC,
					CASE WHEN @OrderBy = 'ModifiedDate DESC' THEN ModifiedDate END DESC
				) AS RowNumber
	INTO		#Games
	FROM		Games
	WHERE		[Status] = 1 AND
				(@Id IS NULL OR Id = @Id) AND
				(@FilterIds = 0 OR Id IN (SELECT Id FROM @Ids))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Games
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
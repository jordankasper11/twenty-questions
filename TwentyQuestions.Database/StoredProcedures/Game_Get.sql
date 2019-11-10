CREATE PROCEDURE [dbo].[Game_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@UserId			UNIQUEIDENTIFIER,
	@OrderBy		NVARCHAR(64) = 'ModifiedDate DESC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate,
				CASE WHEN CreatedBy = @UserId THEN [Subject] ELSE NULL END AS [Subject],
				OpponentId, MaxQuestions, Completed, Questions,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'ModifiedDate ASC' THEN ModifiedDate END ASC,
					CASE WHEN @OrderBy = 'ModifiedDate DESC' THEN ModifiedDate END DESC
				) AS RowNumber
	INTO		#Games
	FROM		Games
	WHERE		[Status] = 1 AND
				(@Id IS NULL OR Id = @Id) AND
				(@FilterIds = 0 OR Id IN (SELECT Id FROM @Ids)) AND
				(@UserId IS NULL OR CreatedBy = @UserId OR OpponentId = @UserId)

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Games
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
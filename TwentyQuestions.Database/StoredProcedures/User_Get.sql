CREATE PROCEDURE [dbo].[User_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@Status			INT = NULL,
	@UserId			UNIQUEIDENTIFIER,
	@OrderBy		NVARCHAR(64) = 'Username ASC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		Id, [Status], CreatedBy, CreatedDate, ModifiedBy, ModifiedDate,
				Username, Email, AvatarFileExtension,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'Username ASC' THEN Username END ASC,
					CASE WHEN @OrderBy = 'Username DESC' THEN Username END DESC
				) AS RowNumber
	INTO		#Users
	FROM		Users
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND [Status] = 1) OR (@Status IS NOT NULL AND [Status] & @Status > 0)) AND
				(@Id IS NULL OR Id = @Id) AND
				(@FilterIds = 0 OR Id IN (SELECT Id FROM @Ids))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Users
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO
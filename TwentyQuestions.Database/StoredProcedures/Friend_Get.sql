CREATE PROCEDURE [dbo].[Friend_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@UserId			UNIQUEIDENTIFIER,
	@OrderBy		NVARCHAR(64) = 'Username ASC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	DECLARE		@Friends TABLE (
		Id			UNIQUEIDENTIFIER,
		FriendId	UNIQUEIDENTIFIER,
		Username	NVARCHAR(32)
	)

	INSERT INTO @Friends (Id, FriendId, Username)
		SELECT	F.Id,
				CASE WHEN F.CreatedBy = @UserId THEN F.FriendId ELSE F.CreatedBy END,
				CASE WHEN F.CreatedBy = @UserId THEN U2.Username ELSE U1.Username END
		FROM	Friends AS F
				INNER JOIN Users AS U1 ON U1.Id = F.CreatedBy
				INNER JOIN Users AS U2 ON U2.Id = F.FriendId
		WHERE	F.CreatedBy = @UserId OR
				F.FriendId = @UserId

	SELECT		F.Id, F.[Status], F.CreatedBy, F.CreatedDate, F.ModifiedBy, F.ModifiedDate,
				T.FriendId, T.Username,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'Username ASC' THEN T.Username END ASC,
					CASE WHEN @OrderBy = 'Username DESC' THEN T.Username END DESC
				) AS RowNumber
	INTO		#Friends
	FROM		@Friends AS T
				INNER JOIN Friends AS F ON F.Id = T.Id
	WHERE		F.[Status] = 1 AND
				(@Id IS NULL OR F.Id = @Id) AND
				(@FilterIds = 0 OR F.Id IN (SELECT Id FROM @Ids))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Friends
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO
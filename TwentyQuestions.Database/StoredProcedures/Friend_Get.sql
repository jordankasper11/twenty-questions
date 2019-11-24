CREATE PROCEDURE [dbo].[Friend_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@Status			INT = NULL,
	@UserId			UNIQUEIDENTIFIER,
	@FriendId		UNIQUEIDENTIFIER = NULL,
	@OrderBy		NVARCHAR(64) = 'Username ASC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	DECLARE @Friends TABLE (
		Id					UNIQUEIDENTIFIER,
		FriendId			UNIQUEIDENTIFIER,
		Username			NVARCHAR(32),
		AvatarFileName		VARCHAR(256)
	)

	INSERT INTO @Friends (Id, FriendId, Username, AvatarFileName)
		SELECT	F.Id,
				CASE WHEN F.CreatedBy = @UserId THEN F.FriendId ELSE F.CreatedBy END,
				CASE WHEN F.CreatedBy = @UserId THEN U2.Username ELSE U1.Username END,
				CASE WHEN F.CreatedBy = @UserId THEN U2.AvatarFileName ELSE U1.AvatarFileName END
		FROM	Friends AS F
				INNER JOIN Users AS U1 ON U1.Id = F.CreatedBy
				INNER JOIN Users AS U2 ON U2.Id = F.FriendId
		WHERE	F.CreatedBy = @UserId OR
				F.FriendId = @UserId

	SELECT		F.Id, F.[Status], F.CreatedBy, F.CreatedDate, F.ModifiedBy, F.ModifiedDate,
				T.FriendId, T.Username, T.AvatarFileName,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'Username ASC' THEN T.Username END ASC,
					CASE WHEN @OrderBy = 'Username DESC' THEN T.Username END DESC
				) AS RowNumber
	INTO		#Friends
	FROM		@Friends AS T
				INNER JOIN Friends AS F ON F.Id = T.Id
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND F.[Status] = 1) OR (@Status IS NOT NULL AND F.[Status] & @Status > 0)) AND
				(@Id IS NULL OR F.Id = @Id) AND
				(@FilterIds = 0 OR F.Id IN (SELECT Id FROM @Ids)) AND
				(@FriendId IS NULL OR T.FriendId = @FriendId)

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Friends
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
GO
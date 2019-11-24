CREATE PROCEDURE [dbo].[Game_Get] (
	@Id				UNIQUEIDENTIFIER = NULL,
	@Ids			EntityIdsType READONLY,
	@Status			INT = NULL,
	@UserId			UNIQUEIDENTIFIER,
	@Completed		BIT = NULL,
	@OrderBy		NVARCHAR(64) = 'ModifiedDate DESC',
	@PageNumber		INT = 1,
	@PageSize		INT = 2147483647,
	@TotalRecords	INT = NULL OUTPUT
)
AS
BEGIN
	DECLARE	@FilterIds BIT = CASE WHEN EXISTS(SELECT Id FROM @Ids) THEN 1 ELSE 0 END

	SELECT		G.Id, G.[Status], G.CreatedBy, G.CreatedDate, G.ModifiedBy, G.ModifiedDate, G.OpponentId,
				CASE WHEN G.CreatedBy = @UserId THEN G.[Subject] ELSE NULL END AS [Subject],
				CASE WHEN G.CreatedBy = @UserId THEN U2.Id ELSE U1.Id END AS FriendId,
				CASE WHEN G.CreatedBy = @UserId THEN U2.Username ELSE U1.Username END AS FriendUsername,
				CASE WHEN G.CreatedBy = @UserId THEN U2.AvatarFileName ELSE U1.AvatarFileName END AS FriendAvatarFileName,
				G.MaxQuestions, G.Completed, G.Questions,
				ROW_NUMBER() OVER (ORDER BY
					CASE WHEN @OrderBy = 'ModifiedDate ASC' THEN G.ModifiedDate END ASC,
					CASE WHEN @OrderBy = 'ModifiedDate DESC' THEN G.ModifiedDate END DESC
				) AS RowNumber
	INTO		#Games
	FROM		Games AS G
				INNER JOIN Users AS U1 ON U1.Id = G.CreatedBy
				INNER JOIN Users AS U2 ON U2.Id = G.OpponentId
	WHERE		((@Id IS NOT NULL OR @FilterIds = 1) OR (@Status IS NULL AND G.[Status] = 1) OR (@Status IS NOT NULL AND G.[Status] & @Status > 0)) AND
				(@Id IS NULL OR G.Id = @Id) AND
				(@FilterIds = 0 OR G.Id IN (SELECT Id FROM @Ids)) AND
				(@UserId IS NULL OR G.CreatedBy = @UserId OR G.OpponentId = @UserId) AND
				(@Completed IS NULL OR G.Completed = @Completed) AND
				(@Completed IS NOT NULL OR (G.Completed = 0 OR G.ModifiedDate <= DATEADD(DAY, 7, GETUTCDATE())))

	SET	@TotalRecords = @@ROWCOUNT

	SELECT		*
	FROM		#Games
	ORDER BY	RowNumber ASC
	OFFSET		(@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT	@PageSize ROWS ONLY
END
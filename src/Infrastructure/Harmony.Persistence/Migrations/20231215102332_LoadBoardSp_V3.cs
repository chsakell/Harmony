using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoadBoardSp_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" USE [Harmony]
GO

/****** Object:  StoredProcedure [dbo].[LoadBoard]    Script Date: 12/01/2023 5:27:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[LoadBoard]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[LoadBoard]
END
GO

CREATE PROCEDURE [dbo].[LoadBoard] @BoardId uniqueidentifier, @cardsPerList int
AS
BEGIN
	SELECT * FROM Boards WHERE Id = @BoardId

	DECLARE @BoardType int
    SELECT @BoardType = Type FROM Boards WHERE Id = @BoardId

	SELECT * FROM BoardLists where BoardId = @BoardId AND Status = 0

	declare @boardListIds TABLE(Id uniqueidentifier)
	INSERT into @boardListIds (Id)
	select Id from BoardLists  where BoardId = @BoardId AND Status = 0

	DECLARE @cards Table(Id uniqueidentifier, Title nvarchar(300), Description nvarchar(max), UserId nvarchar(450),
	BoardListId uniqueidentifier, Position smallint, Status int, StartDate datetime2, DueDate datetime2, ReminderDate datetime2,
	SerialNumber int, IssueTypeId uniqueidentifier, SprintId uniqueidentifier, DateCreated datetime2, DateUpdated datetime2, 
	DueDateReminderType int null, StoryPoints smallint null, DateCompleted datetime2 null);

	DECLARE @boardListId uniqueidentifier

	DECLARE cursor_boardLists CURSOR
	FOR SELECT 
			Id
		FROM 
			BoardLists where BoardId = @BoardId AND Status = 0;


	OPEN cursor_boardLists;

	FETCH NEXT FROM cursor_boardLists INTO @boardListId;

	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @BoardType = 0
				BEGIN
					INSERT INTO @cards 
					Select * from Cards
					where BoardListId = @boardListId AND Status = 0 
					order by Position
					OFFSET 0 ROWS 
					FETCH FIRST @cardsPerList ROWS ONLY;
					FETCH NEXT FROM cursor_boardLists INTO 
						@boardListId;
				END
				ELSE
				BEGIN
					INSERT INTO @cards 
					Select c.* from Cards c
					JOIN Sprints s on s.Id = c.SprintId
					where BoardListId = @boardListId AND c.Status = 0 AND s.Status = 1
					order by c.Position
					OFFSET 0 ROWS 
					FETCH FIRST @cardsPerList ROWS ONLY;
					FETCH NEXT FROM cursor_boardLists INTO 
						@boardListId;
				END
		END;

	CLOSE cursor_boardLists;

	DEALLOCATE cursor_boardLists;

	select * from @cards Order by BoardListId, Position

	select * from Labels where BoardId = @BoardId

	select cl.* 
	from CardLabels cl
	join Labels l on l.Id = cl.LabelId
	where l.BoardId = @BoardId

	select * from Attachments where CardId in (Select id from @cards) order by DateCreated

	select * from UserCards where CardId in (Select id from @cards) 

	select * from CheckLists where CardId in (select id from @cards) order by CardId, position

	select * from CheckListItems 
	where CheckListId in (select Id from CheckLists where CardId in (select id from @cards))
	order by CheckListId, Position

	select * from IssueTypes where BoardId = @BoardId order by DateCreated

	select * from Sprints where BoardId = @BoardId

	select CardId, COUNT(*) as TotalComments
	from Comments
	where CardId in (select id from @cards)
	group by CardId
END
GO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[LoadBoard]");
        }
    }
}

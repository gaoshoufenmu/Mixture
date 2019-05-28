USE [test]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Proc_Table_Insert
	-- Add the parameters for the stored procedure here
	@id INT OUTPUT,
	@uid INT,
	@uname VARCHAR(100),
	@time DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @tblname VARCHAR(30), @objectId INT
    SET @tblname = 'SlaveLog_' + cast(@uid%256 as varchar(5))
    SELECT @objectId = [object_id] FROM sys.tables WHERE [name] = @tblname
    IF @objectId is null
    BEGIN
		EXEC Proc_Table_Create @tblname
	END
	
	DECLARE @sql NVARCHAR(4000)
	SET @sql = 'INSERT INTO ' + @tblname + '(
				s_uid,
				s_uname,
				s_time)'
	EXEC SP_EXECUTESQL @sql, N'
								@uidp INT,
								@unamep VARCHAR(100),
								@timep DATETIME'
							, @uidp = @uid
							, @unamep = @uname
							, @timep = @time						
	SET @id = @@identity
END
GO
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
CREATE PROCEDURE Proc_Table_Update
@id INT,
@uid INT,
@uname VARCHAR(100),
@time DATETIME

AS
BEGIN
	SET NOCOUNT ON

	DECLARE @objectId INT, @tblname VARCHAR(30)
	SET @tblname = 'SlaveLog_' + cast(@uid%256 as varchar(5))
	SELECT @objectId = [object_id] FROM sys.tables WHERE [name] = @tblname
	IF @objectId IS NULL
		RETURN

	DECLARE @sql VARCHAR(3000)
	SET @sql = 'UPDATE TABLE ' + @tblname + ' SET(
													s_uid = @uidp,
													s_uname = @unamep,
													s_time = @timep)
											  WHERE s_id = @idp'
	EXEC SP_EXECUTESQL @sql, N'@uidp INT
							  ,@unamep VARCHAR(100)
							  ,@time DATETIME'
						   ,@uidp = @uid
						   ,@unamep = @uname
						   ,@timep = @time
						   ,@idp = @id
END
GO
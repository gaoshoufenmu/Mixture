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
CREATE PROCEDURE Proc_Table_Delete
@id INT,
@uid INT

AS
BEGIN
	SET NOCOUNT ON

	DECLARE @objectId INT, @tblname VARCHAR(30)
	SET @tblname = 'SlaveLog_' + cast(@uid%256 as varchar(5))
	SELECT @objectId = [object_id] FROM sys.tables WHERE [name] = @tblname
	IF @objectId IS NULL
		RETURN

	DECLARE @sql VARCHAR(1000)
	SET @sql = 'DELETE FROM ' + @tblname + ' WHERE s_id = @idp'
	EXEC SP_EXECUTESQL @sql, N'@idp INT', @idp = @id
	-- SET @sql = 'DELETE FROM ' + @tblname + ' WHERE s_uid = @uidp'
	-- EXEC SP_EXECUTESQL @sql, N'@uidp INT', @uidp = @uid

END
GO
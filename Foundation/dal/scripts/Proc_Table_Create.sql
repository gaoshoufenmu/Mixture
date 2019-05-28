--
--
--
USE [test]
GO
CREATE PROCEDURE Proc_Table_Create
	-- Add the parameters for the stored procedure here
	@tablename varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(3000)

    SET @sql = 'IF EXISTS (SELECT 1 FROM sysindexes WHERE id = object_id(''' + @tablename + ''') AND name = ''Idx_uid'' AND indid > 0 AND indid < 255)
				DROP index ' + @tablename + '.Idx_uid' +
			   '
			   IF EXISTS (SELECT 1 FROM sysobjects WHERE id = object_id(''' + @tablename + ''') AND type = ''U'')
			    DROP TABLE ' + @tablename +

	'
	CREATE TABLE '
    + @tablename +				-- note that brackets can not be applied to fields in this sql stmt
    ' (
	s_id int IDENTITY(1,1) NOT NULL,
	s_uid int NOT NULL,
	s_uname varchar(100) NOT NULL,
	s_time datetime NOT NULL,
 CONSTRAINT PK_' + UPPER(@tablename) + ' PRIMARY KEY CLUSTERED 
(
	s_id ASC
)
)
	create index Idx_uid on ' +@tablename + ' (s_uid ASC)
'
	EXEC SP_EXECUTESQL @sql
END
GO
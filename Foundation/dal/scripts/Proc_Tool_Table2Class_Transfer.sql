USE [test]
GO

SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------------------
--
-- Transfer from table fields to CSharp class
-- @procname: specify procedure
-- @intypes: specify input params type
-- @outtypes: specify output params type
--
-------------------------------------------------------------------------------------------
CREATE PROCEDURE Proc_Tool_Table2Class_Transfer
@tablename VARCHAR(100)
AS
BEGIN
    DECLARE @colname VARCHAR(50), @typename VARCHAR(50), @description VARCHAR(250)
	DECLARE @objid INT, @csharptype VARCHAR(20)

	-- check whether the table exists
	SELECT @objid = [object_id] FROM sys.tables WHERE [name] = @tablename
	IF @objid is NULL
	BEGIN
		PRINT 'Can not find the table or view "' + @tablename + '"'
		RETURN
	END

	-- get all fields informations from this table
	DECLARE fieldscur CURSOR FOR
		SELECT c.[name] as colname, t.[name] as typename, Cast(ep.[value] as VARCHAR(250)) as description 
		FROM sys.columns c LEFT JOIN sys.types t
			ON c.[system_type_id] = t.[system_type_id] AND c.[user_type_id] = t.[user_type_id] 
			LEFT JOIN sys.extended_properties ep ON c.object_id = ep.major_id AND c.column_id = ep.minor_id
		WHERE c.[object_id] = @objid ORDER BY c.column_id
	OPEN fieldscur
	FETCH NEXT FROM fieldscur
		INTO @colname, @typename, @description

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @csharptype = CASE @typename
		WHEN 'int' THEN 'int'
		WHEN 'tinyint' THEN 'byte'
		WHEN 'smallint' THEN 'Int16'		
		WHEN 'bigint' THEN 'Int64'
		WHEN 'datetime' THEN 'DateTime'
		WHEN 'smalldatetime' THEN 'DateTime'
		WHEN 'money' THEN 'decimal'
		WHEN 'smallmoney' THEN 'decimal'
		WHEN 'bit' THEN 'bool'
		WHEN 'float' THEN 'Single'
		WHEN 'real' THEN 'double'
		WHEN 'image' THEN 'byte[]'
		ELSE 'string'
		END

		IF @csharptype <> 'string'
			PRINT '          obj.' + @colname + ' = (' + @csharptype + ')reader["' + @colname + '"];'
		ELSE
			PRINT '          obj.' + @colname + ' = reader["' + @colname + '"].ToString();'
		
		FETCH NEXT FROM fieldscur
		INTO @colname, @typename, @description
	END

	CLOSE fieldscur
	DEALLOCATE fieldscur
END
GO
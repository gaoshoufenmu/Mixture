USE [test]
GO

SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------------------
--
-- Print a CSharp class mapping to a special table
-- @procname: specify procedure
-- @intypes: specify input params type
-- @outtypes: specify output params type
--
-------------------------------------------------------------------------------------------

CREATE PROCEDURE Proc_Tools_Create_DALMethod
@procname varchar(100),
@intype varchar(100),
@outtype varchar(100),
@isscalar bit

AS

BEGIN
	DECLARE @paramname VARCHAR(50), @typename VARCHAR(50), @length INT, @isoutparam BIT
	DECLARE @objid INT, @csharptype VARCHAR(20)

	-- check procedure whether exists
	SELECT @objid = [object_id] FROM sys.procedures WHERE [name] = @procname
	IF @objid IS NULL
	BEGIN
		PRINT 'Can not find the procedure [' + @procname + ']'
		RETURN
	END

	-- get all params details of this procedure
	DECLARE params CURSOR FOR
		SELECT p.[name] , t.[name] as typename, p.max_length as max_length, p.is_output as isoutput 
		FROM sys.parameters p LEFT JOIN sys.types t 
			ON p.[system_type_id] = t.[system_type_id] AND p.[user_type_id] = t.[user_type_id]
		WHERE p.[object_id] = @objid
		ORDER BY p.[parameter_id]

	DECLARE @instmt VARCHAR(1000)
	DECLARE @outstmt VARCHAR(1000)
	DECLARE @valstmt VARCHAR(1000)
	SET @instmt = ''
	SET @outstmt = ''
	SET @valstmt = ''

	FETCH NEXT FROM params
		INTO @paramname, @typename, @length, @isoutparam

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @csharptype=CASE @typename
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

		-- first param; without comma prefixed when jointing
		IF @instmt = ''
		BEGIN
			IF @isoutparam = 0
				SET @instmt = @csharptype + ' ' +LOWER(REPLACE(@paramname, '@', ''))
			ELSE
			BEGIN
				SET @outstmt = @outstmt + ' ' +LOWER(REPLACE(@paramname, '@', '')) + ' = (' +@csharptype +')dbCmd.Parameters["' +@paramname + '"].Value;'
				SET @instmt = 'out ' + @csharptype + ' ' + LOWER(REPLACE(@paramname, '@', ''))
				IF @intype <> ''
					SET @outstmt = @csharptype + ' ' + @outstmt
			END
		END
		ELSE
		BEGIN
			IF @isoutparam = 0
				SET @instmt = @instmt + ', ' + @csharptype + ' ' + LOWER(REPLACE(@paramname, '@', ''))
			ELSE
			BEGIN
				SET @outstmt = @outstmt + ' ' + LOWER(REPLACE(@paramname, '@', '')) + ' = (' + @csharptype + ')dbCmd.Parameters["' + @paramname + '"].Value;'
				SET @instmt = @instmt + ', out ' + @csharptype + ' ' + LOWER(REPLACE(@paramname, '@', ''))
				IF @intype <> ''
					SET @outstmt = @csharptype + ' ' + @outstmt
			END
		END

		IF @valstmt = ''
		BEGIN
			IF @isoutparam = 0
				SET @valstmt = LOWER(REPLACE(@paramname, '@', ''))
			ELSE
				SET @valstmt = 'out ' + LOWER(REPLACE(@paramname, '@', ''))
		END
		ELSE
		BEGIN
			IF @isoutparam = 0
				SET @valstmt = @valstmt + ', ' + LOWER(REPLACE(@paramname, '@', ''))
			ELSE
				SET @valstmt = @valstmt + ', out ' + LOWER(REPLACE(@paramname, '@', ''))
		END

		FETCH NEXT FROM params
			 INTO @paramname, @typename, @length, @isoutparam
	END
	CLOSE params

	OPEN params
	IF @intype <> ''
	BEGIN
		SET @instmt = @intype + ' obj'
		SET @valstmt = 'obj'
	END

	PRINT ' public ' + @outtype + ' ' + REPLACE(@procname, 'proc_', '') + '(' +@instmt + ')'
	PRINT '{'
	PRINT '     var dbCmd = db.GetStoredProcCommand("' + @procname + '");'

	FETCH NEXT FROM params
		INTO @paramname, @typename, @length, @isoutparam
	WHILE @@FETCH_STATUS =0
	BEGIN
		SET @csharptype=CASE @typename
		WHEN 'int' THEN 'Int32'
		WHEN 'tinyint' THEN 'Byte'
		WHEN 'smallint' THEN 'Int16'		
		WHEN 'bigint' THEN 'Int64'
		WHEN 'datetime' THEN 'DateTime'
		WHEN 'smalldatetime' THEN 'DateTime'
		WHEN 'money' THEN 'Currency'
		WHEN 'smallmoney' THEN 'Currency'
		WHEN 'bit' THEN 'Boolean'
		WHEN 'float' THEN 'Single'
		WHEN 'real' THEN 'Double'
		WHEN 'image' THEN 'Binary'
		ELSE 'String'
		END

		IF @isoutparam = 0
		BEGIN
			IF @intype = ''
				PRINT '     db.AddInParameter(dbCmd, "' + @paramname + '", DbType.' + @csharptype + ', ' +LOWER(REPLACE(@paramname, '@', '')) + ');'
			ELSE
				PRINT '     db.AddInParameter(dbCmd, "' + @paramname + '", DbType.' + @csharptype + ', obj.' + REPLACE(@paramname, '@', '') + ');'
		END
		ELSE
			PRINT '     db.AddOutParamenter(dbCmd, "' + @paramname + '", DbType.' + @csharptype + ', ' + CAST(@length AS VARCHAR(10)) + ');'

		FETCH NEXT FROM params
			INTO @paramname, @typename, @length, @isoutparam
	END
	CLOSE params
	DEALLOCATE params

	PRINT ''
	PRINT '     try'
	PRINT '     {'
	
	DECLARE @returnstr VARCHAR(50), @basetype VARCHAR(50)
	SET @basetype = REPLACE(REPLACE(@outtype, 'list<', ''), '>', '')

	

	IF @outtype = 'int'
	BEGIN
		IF @isscalar = 0
			PRINT '     int _ret = db.ExecuteNonQuery(dbCmd);'	-- perform non query, such as UPDATE, DELETE and ADD operations
		ELSE
			PRINT '     int _ret = Convert.ToInt32(db.ExecuteScalar(dbCmd));' -- perform sql query, and return first column of first row of result data set

		IF @outstmt <> ''
		BEGIN
			PRINT '     ' + @outstmt
		END
		SET @returnstr = '_ret'
	END
	ELSE IF @outtype = 'string'
	BEGIN
		IF @isscalar = 0
			PRINT '     db.ExecuteNonQuery(dbCmd);'
		ELSE
			PRINT '     string _ret = Convert.ToString(db.ExecuteScalar(dbCmd));'
		IF @outstmt <> ''
		BEGIN
			PRINT '     ' + @outstmt
		END
		SET @returnstr = '_ret'
	END
	ELSE IF @outtype = 'bool'
	BEGIN
		IF @isscalar = 0
			PRINT '     db.ExecuteNonQuery(dbCmd);'
		ELSE
			PRINT '     bool _ret = Convert.ToBoolean(db.ExecuteScalar(dbCmd));'
		IF @outstmt <> ''
		BEGIN
			PRINT '     ' + @outstmt
		END
		SET @returnstr = '_ret'
	END
	ELSE IF @outtype = 'Dataset'
	BEGIN
		PRINT '     Dataset ds = db.ExecuteDataset(dbCmd);'
		IF @outstmt <> ''
		BEGIN
			PRINT '     ' + @outstmt
		END
		SET @returnstr = '_ret'
	END
	ELSE IF @outtype = 'void'
	BEGIN
		PRINT '     db.ExecuteNonQuery(dbCmd);'
		IF @outstmt <> ''
		BEGIN
			PRINT '     ' + @outstmt
		END
		SET @returnstr = '_ret'
	END
	ELSE IF SUBSTRING(@outtype,1,5)='list<'
	BEGIN										-- ExcuteReader: perform sql query, and return DataReader
		PRINT '			'+@outtype+' list = new '+@outtype+'();'
		PRINT '			using(IDataReader reader = db.ExecuteReader(dbCommandWrapper))
				{
					while(reader.Read())
					{
						'+@basetype+' obj = new '+@basetype+'();'
					EXEC Proc_Tool_Table2Class_Transfer @basetype
		PRINT '					list.Add(obj);'
		PRINT '				}
					reader.NextResult();'
		IF @outstmt<>''
		BEGIN	
			PRINT '			'+@outstmt
		END
		PRINT'			}
			'
		SET @returnstr = 'list' 
	END
	ELSE
	BEGIN
		PRINT '			'+@outtype+' obj = null;'
		PRINT '			using(IDataReader reader = db.ExecuteReader(dbCommandWrapper))
				{
					if(reader.Read())
					{
						obj = new '+@outtype+'();'
					EXEC Proc_Tool_Table2Class_Transfer @outtype
		PRINT '				}'
		IF @outstmt<>''
		begin	
			PRINT '			'+@outstmt
		end
		PRINT'			}
			'
		SET @returnstr = 'obj' 
	END

	IF @returnstr <> ''
	BEGIN
		PRINT ' '
		PRINT '      return ' + @returnstr + ';'
	END

	PRINT '    }
	       catch(Exception e)
		   {
			   throw new Exception(e.Message);
		   }'
	PRINT ' }'
END
USE [test]
GO

SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------------------
--
-- Print a CSharp class mapping to a special table
--
--
-------------------------------------------------------------------------------------------

CREATE PROCEDURE Proc_Tool_Create_MappingClass
@tablename varchar(100)

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

	IF RIGHT(@tablename, 3) = 'ies'
		SET @tablename = LEFT(@tablename, LEN(@tablename) - 3) + 'y'
	ELSE IF RIGHT(@tablename, 1) = 's' AND RIGHT(@tablename, 2) <> 'ss'
		SET @tablename = LEFT(@tablename, LEN(@tablename) - 1)

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

-------------------------------------------------------------------------------------------
	PRINT 'public class ' +@tablename + ' : IDisposable'
	PRINT '{'
	PRINT '		#region IDisposable 
		/// <summary>
		/// Finalizer
		/// </summary>'
	PRINT '		~'+@tablename+'()'+'
		{
			Dispose( false );
		} '
	PRINT ''
	PRINT '		/// <summary>
		/// Stop Finalizing
		/// </summary>
		public void Dispose()		
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;
		}
		#endregion'
	PRINT ''

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

		IF @description IS NULL
			SET @description = @colname
		ELSE
			SET @description = @description + '[' + @colname + ']'

		PRINT '		#region '+@description
		PRINT '		private ' + @csharptype + ' _' + LOWER(@colname) + ';'
		PRINT '     public ' + @csharptype + ' ' + @colname + ' => ' + ' _' + LOWER(@colname) + ';'
		PRINT '     #endregion'
		PRINT ''

		FETCH NEXT FROM fieldscur
			INTO @colname, @typename, @description
	END

	CLOSE fieldscur
	DEALLOCATE fieldscur
PRINT '}'
END
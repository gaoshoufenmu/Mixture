USE [test]
GO

SET ANSI_NULLS ON 
GO
--Specifies ISO compliant behavior of the Equals (=) and Not Equal To (<>) comparison operators when they are used with null values in SQL Server 2016.
--When SET ANSI_NULLS is ON, a SELECT statement that uses WHERE column_name = NULL returns zero rows even if there are null values in column_name. 
--A SELECT statement that uses WHERE column_name <> NULL returns zero rows even if there are nonnull values in column_name.

--When SET ANSI_NULLS is OFF, the Equals (=) and Not Equal To (<>) comparison operators do not follow the ISO standard. 
--A SELECT statement that uses WHERE column_name = NULL returns the rows that have null values in column_name. 
--A SELECT statement that uses WHERE column_name <> NULL returns the rows that have nonnull values in the column. 
--Also, a SELECT statement that uses WHERE column_name <> XYZ_value returns all rows that are not XYZ_value and that are not NULL.(see https://msdn.microsoft.com/en-us/library/ms188048.aspx)

SET QUOTED_IDENTIFIER ON
GO
--Causes SQL Server to follow the ISO rules regarding quotation mark delimiting identifiers and literal strings. 
--Identifiers delimited by double quotation marks can be either Transact-SQL reserved keywords or can contain characters not generally allowed by the Transact-SQL syntax rules for identifiers.
--When SET QUOTED_IDENTIFIER is ON, identifiers can be delimited by double quotation marks, and literals must be delimited by single quotation marks. 
--When SET QUOTED_IDENTIFIER is OFF, identifiers cannot be quoted and must follow all Transact-SQL rules for identifiers.

--When SET QUOTED_IDENTIFIER is ON (default), all strings delimited by double quotation marks are interpreted as object identifiers. 
--Therefore, quoted identifiers do not have to follow the Transact-SQL rules for identifiers. 
--They can be reserved keywords and can include characters not generally allowed in Transact-SQL identifiers. Double quotation marks cannot be used to delimit literal string expressions; 
--single quotation marks must be used to enclose literal strings. 
--If a single quotation mark (') is part of the literal string, it can be represented by two single quotation marks ("). 
--SET QUOTED_IDENTIFIER must be ON when reserved keywords are used for object names in the database.
--When SET QUOTED_IDENTIFIER is OFF, literal strings in expressions can be delimited by single or double quotation marks. 
--If a literal string is delimited by double quotation marks, the string can contain embedded single quotation marks, such as apostrophes.(see https://msdn.microsoft.com/en-us/library/ms174393.aspx)

-------------------------------------------------------------------------------------------
--
-- Print sql statements that can be executed to create a procedure or all CRUD procedures
--
--
-------------------------------------------------------------------------------------------
CREATE PROCEDURE Proc_Tool_Create_Produce
@tablename varchar(50),		-- which table the CRUD operations are aimed to
@opname varchar(20),		-- operation name, such as 'insert', 'update' and others; empty string denotes all CRUD operations
@authorname varchar(30),	-- will be presented in comments at the head
@description varchar(100)	-- will be presented in comments at the head

AS
BEGIN

	SET NOCOUNT ON

	DECLARE @objectID INT
	SELECT @objectID = [object_id] FROM sys.tables WHERE [name]=@tablename	-- check whether table exists
	IF @objectID IS NULL
		RETURN

	DECLARE @colname varchar(100), @typename varchar(50), @bytelen int, @colid int	-- declarations
	DECLARE @colopname varchar(2000), @colopvalue varchar(2000)
	DECLARE @colprocparam varchar(100), @keycol varchar(50), @iskeycol bit
	DECLARE @startid int, @endid int, @sql VARCHAR(1000)

	-- get all fields informations of this table
	DECLARE fieldscur CURSOR FOR	
		-- contains column name, column type, column byte length, if is key column and column id
		-- maximum length (in bytes) of the column, -1 = Column data type is varchar(max), nvarchar(max), varbinary(max), or xml
		SELECT c.[name] as colname, t.[name] as typename, c.max_length as bytelen, c.is_identity as iskeycol, c.column_id as colid
		FROM sys.columns c LEFT JOIN sys.types t			
		ON c.[system_type_id] = t.[system_type_id]			-- ID of the system type of the column
			AND c.[user_type_id] = t.[user_type_id]			-- ID of the type of the column as defined by the user
		WHERE c.[object_id] = @objectID						-- ID of the object to which this column belongs
		ORDER BY c.column_id								-- ID of the column

	IF @opname = 'insert' or @opname = ''	-- print sql statements of INSERT procedure
	BEGIN
		OPEN fieldscur
		SET @colopname = ''
		SET @colopvalue = ''
		PRINT '/*----'
		PRINT 'Function: '+@tablename
		PRINT 'Date : '+convert(varchar(20),getdate(),120)
		PRINT 'Author: '+@authorname
		PRINT 'Description: ' + @description
		PRINT '----*/'
		PRINT ''
		PRINT 'CREATE PROCEDURE Proc_' +@tablename + '_Insert'
		-- fetch a record into varies
		FETCH NEXT FROM fieldscur
			INTO @colname, @typename, @bytelen, @iskeycol, @colid

		WHILE @@FETCH_STATUS = 0
		BEGIN

			-- JOINT A COMPLETED COLUMN DECLARATION. aka. @c_id int OR @c_name varchar(20)
			SET @colprocparam = '@' + @colname + ' ' + @typename						
			IF @iskeycol = 0   -- current column is not key
			BEGIN
				-- column is a string, append its length
				IF @typename = 'varchar' or @typename = 'char'				-- current column is single-byte string
				BEGIN 
					IF @bytelen > 0     -- finit length
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen as varchar(10)) + ')'
					ELSE				-- infinit length
						SET @colprocparam = @colprocparam + '(max)'
				END
				ELSE IF @typename = 'nvarchar' or @typename = 'nchar'		-- current column is double-byte string
				BEGIN
					IF @bytelen > 0		-- finit length
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen/2 as varchar(10)) + ')'
					ELSE
						SET @colprocparam = @colprocparam + '(max)'
				END

				-- accumulate column names into a sql statement used in INSERT operation. aka. INSERT INTO [tablename] (...)
				IF @colopname = ''		-- firstly joint sql statement, comma is not needed
					SET @colopname = ' ' + @colname
				ELSE
					SET @colopname = @colopname + char(10) +',' + @colname

				-- accumulate column values into a sql statement used in INSERT operation. aka. INSERT INTO [tablename] (...) VALUES(...)
				IF @colopvalue = ''
					SET @colopvalue = ' @' + @colname		-- aka. param of this procedure
				ELSE
					SET @colopvalue = @colopvalue + char(10) + ',@' + @colname
			END
			ELSE		-- current column is key
			BEGIN
				SET @colprocparam = @colprocparam + ' OUTPUT'
				SET @keycol = @colname
			END

			FETCH NEXT FROM fieldscur
				INTO @colname, @typename, @bytelen, @iskeycol, @colid
			
			-- print declarations of this procedure params
			IF @@FETCH_STATUS = 0	-- not last record, then comma is needed
				PRINT ' ' + @colprocparam + ','
			ELSE
				PRINT ' ' + @colprocparam
		END

		PRINT 'AS'
		PRINT ''
		PRINT 'INSERT INTO ' + @tablename + ' ('

		PRINT @colopname
		PRINT ') VALUES('
		PRINT @colopvalue
		PRINT ')'

		PRINT 'SET @' + @keycol + '=@@identity'
		PRINT 'GO'
		CLOSE fieldscur
	END
	-- OUTPUTING SQL STATEMENT OF INSERT PROCEDURE ENDS HERE

	-- PRINT SQL STATEMENT OF UPDATE PROCEDURE
	IF @opname = 'update' or @opname = ''
	BEGIN
		OPEN fieldscur
		SET @colopname = ''
		SET @colopvalue = ''
		PRINT '/*----'
		PRINT 'Function: '+@tablename
		PRINT 'Date : '+convert(varchar(20),getdate(),120)
		PRINT 'Author: '+@authorname
		PRINT 'Description: ' + @description
		PRINT '----*/'
		PRINT ''
		PRINT 'CREATE PROCEDURE Proc_' +@tablename + '_Update'

		-- fetch a record into varies
		FETCH NEXT FROM fieldscur
			INTO @colname, @typename, @bytelen, @iskeycol, @colid

		WHILE @@FETCH_STATUS = 0
		BEGIN
			
			-- CONSTRUCT SQL STATEMENT THAT REPRESENT THE PRRAMS OF UPDATE PROCEDURE
			SET @colprocparam = '@' +@colname + ' ' + @typename
			IF @iskeycol = 0
			BEGIN
				IF @typename = 'varchar' or @typename = 'char'
				BEGIN
					IF @bytelen > 0
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen as varchar(10)) + ')'
					ELSE
						SET @colprocparam = @colprocparam + '(max)'
				END
				ELSE IF @typename = 'nvarchar' or @typename = 'nchar'
				BEGIN
					IF @bytelen > 0
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen/2 as varchar(10)) + ')'
					ELSE
						SET @colprocparam = @colprocparam + '(max)'
				END

				-- JOINT UPDATE CLAUSE AKA. UPDATE [tablename] SET ...
				IF @colopname = ''
					SET @colopname = @colname + '=@' + @colname
				ELSE
					SET @colopname = @colopname + char(10) + ',' + @colname + '=@' + @colname
			END
			ELSE
				SET @keycol = @colname

			FETCH NEXT FROM fieldscur
				INTO @colname, @typename, @bytelen, @iskeycol, @colid

			IF @@FETCH_STATUS = 0
				PRINT @colprocparam + ','
			ELSE
				PRINT @colprocparam
		END

		PRINT 'AS'
		PRINT ''
		PRINT 'UPDATE ' + @tablename + ' SET'
		PRINT @colprocparam
		PRINT 'WHERE ' + @keycol + '=@' +@keycol
		PRINT 'GO'
		CLOSE fieldscur
	END
	-- OUTPUTING STATEMENTS OF UPDATE PROCEDURE ENDS HERE

	-- PRINT STATEMENTS OF DELETE PROCEDURE
	IF @opname = 'delete' or @opname = ''
	BEGIN
		OPEN fieldscur
		-- fetch a record into varies
		FETCH NEXT FROM fieldscur
			INTO @colname, @typename, @bytelen, @iskeycol, @colid

		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @iskeycol = 1
			BEGIN
				PRINT '/*----'
				PRINT 'Function: '+@tablename
				PRINT 'Date : '+convert(varchar(20),getdate(),120)
				PRINT 'Author: '+@authorname
				PRINT 'Description: ' + @description
				PRINT '----*/'
				PRINT ''
				PRINT 'CREATE PROCEDURE Proc_'+@tableName+'_Delete_By'+@colname
				PRINT '	@'+@colname+' '+@typename
				PRINT 'AS'
				PRINT ''
				PRINT '	DELETE FROM '+@tablename+' WHERE '+@colname+'=@'+@colname
				PRINT 'GO'
			END

			FETCH NEXT FROM fieldscur
				INTO @colname, @typename, @bytelen, @iskeycol, @colid
		END

		CLOSE fieldscur
	END
	-- DELETE PROCEDURE CREATION ENDS HERE

	-- SELECT PROCEDURE
	IF @opname = 'select' or @opname = ''
	BEGIN
		OPEN fieldscur
		-- fetch a record into varies
		FETCH NEXT FROM fieldscur
			INTO @colname, @typename, @bytelen, @iskeycol, @colid 
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF EXISTS(SELECT [object_id] FROM sys.index_columns WHERE object_id = @objectID AND column_id = @colid)	-- check current column is key
			BEGIN
				PRINT '/*----'
				PRINT 'Function: '+@tablename
				PRINT 'Date : '+convert(varchar(20),getdate(),120)
				PRINT 'Author: '+@authorname
				PRINT 'Description: ' + @description
				PRINT '----*/'
				PRINT ''
				PRINT 'CREATE PROCEDURE Proc_'+@tablename+'_Select_By'+@colname

				SET @colprocparam = ' @' +@colname + ' ' + @typename
				IF @typename = 'varchar' or @typename = 'char'
				BEGIN
					IF @bytelen > 0
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen as varchar(10)) + ')'
					ELSE
						SET @colprocparam = @colprocparam + '(max)'
				END
				ELSE IF @typename = 'nvarchar' or @typename = 'nchar'
				BEGIN
					IF @bytelen > 0
						SET @colprocparam = @colprocparam + '(' + cast(@bytelen/2 as varchar(10)) + ')'
					ELSE
						SET @colprocparam = @colprocparam + '(max)'
				END

				PRINT @colprocparam
				PRINT 'AS'
				PRINT ''
				PRINT 'SELECT * FROM ' +@tablename+ ' WHERE ' + @colname + '= @' + @colname
				PRINT 'GO'
			END
			
			FETCH NEXT FROM fieldscur
				INTO @colname, @typename, @bytelen, @iskeycol, @colid 
		END
		CLOSE fieldscur
	END

	DEALLOCATE fieldscur

	-- paging select a list records
	IF @opname = 'page' or @opname = ''
	BEGIN
		PRINT '/*----'
		PRINT 'Function: '+@tablename
		PRINT 'Date : '+convert(varchar(20),getdate(),120)
		PRINT 'Author: '+@authorname
		PRINT 'Description: ' + @description
		PRINT '----*/'
		PRINT ''
		PRINT 'Create Procedure Proc_' +@tablename+ '_Select'
		PRINT ' @columns varchar(300),'
		PRINT '	@where varchar(1000),'
		PRINT '	@order varchar(100),'
		PRINT '	@page int,'
		PRINT '	@pagesize int,'
		PRINT '	@rowcount int output'
		PRINT 'AS'
		PRINT 'BEGIN'
		PRINT ' EXEC Proc_Table_Select @columns, @where, @order, @page, @pagesize, ''' + @tablename + ''''
		PRINT 'END'
		PRINT 'GO'
	END
END
GO
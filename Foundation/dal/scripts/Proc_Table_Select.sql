USE [test]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------------------------------------------------------------------------
--
-- paging select records from a table
--
-------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE Proc_Table_Select
	@columns varchar(3000),		-- '*', '[fields, delimited by comma]'
	@where varchar(1000),		-- '' or 'WHERE [predicate]'
	@order varchar(100),		-- '' or 'ORDER [field(s)]'
	@page int,					--
	@pagesize int,
	@tablename varchar(100),
	@rowcount int output
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @sql NVARCHAR(3000)
	DECLARE @start int, @end int

	SET @start = (@page - 1) * @pagesize + 1
	SET @end = @page * @pagesize

	IF @rowcount IS NULL
	BEGIN
		SET @sql = 'SELECT @rowcountp = count(1) FROM @tablenamep'
		EXEC SP_EXECUTESQL @sql, N'@tablenamep varchar(100), @rowcountp int output', @tablename, @rowcountp output
		SET @rowcount = @rowcountp
	END

	SET @sql = N'SELECT * FROM 
				(SELECT @columnsp, ROW_NUMBER() OVER (ORDER BY @orderp) as rowindex FROM @tablenamep @wherep) t 
				WHERE t.rowindex BETWEEN @startp AND @endp'

	EXEC SP_EXECUTESQL @sql, N'@columnsp varchar(3000),
							   @orderp varchar(100),
							   @tablenamep varchar(100),
							   @wherep varchar(1000),
							   @startp int,
							   @endp int',
							   @columns,
							   @order,
							   @tablename,
							   @where,
							   @start,
							   @end
END
GO
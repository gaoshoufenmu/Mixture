--Retrieve a specify page rows data
CREATE PROC Proc_Page_Select
@tbname     sysname,               --tablename
@FieldKey   nvarchar(1000),      --key field(s), delimited by comma when necessary
@PageCurrent int=1,               --current page
@PageSize   int=10,                --
@FieldShow nvarchar(1000)='',      --fields delimited by comma to be shown
@FieldOrder nvarchar(1000)='',      --fields used to specify order, extra DESC/ASC can be appended
@Where    nvarchar(1000)='',     --filter
@PageCount int OUTPUT             --
AS
SET NOCOUNT ON
--validation check
IF OBJECT_ID(@tbname) IS NULL
BEGIN
    RAISERROR(N'table: "%s" not exist',1,16,@tbname)
    RETURN
END
IF OBJECTPROPERTY(OBJECT_ID(@tbname),N'IsTable')=0
    AND OBJECTPROPERTY(OBJECT_ID(@tbname),N'IsView')=0
    AND OBJECTPROPERTY(OBJECT_ID(@tbname),N'IsTableFunction')=0
BEGIN
    RAISERROR(N'"%s" is not table or view or table function',1,16,@tbname)
    RETURN
END

--field check
IF ISNULL(@FieldKey,N'')=''
BEGIN
    RAISERROR(N'page selection needs a key field',1,16)
    RETURN
END

--check other parameters
IF ISNULL(@PageCurrent,0)<1 SET @PageCurrent=1
IF ISNULL(@PageSize,0)<1 SET @PageSize=10
IF ISNULL(@FieldShow,N'')=N'' SET @FieldShow=N'*'
IF ISNULL(@FieldOrder,N'')=N''
    SET @FieldOrder=N''
ELSE
    SET @FieldOrder=N'ORDER BY '+LTRIM(@FieldOrder)		--Return a character expression after it removes leading blanks
IF ISNULL(@Where,N'')=N''
    SET @Where=N''
ELSE
    SET @Where=N'WHERE ('+@Where+N')'

-- calculate total page count
IF @PageCount IS NULL
BEGIN
    DECLARE @sql nvarchar(4000)
    SET @sql=N'SELECT @PageCount=COUNT(*)'
        +N' FROM '+@tbname
        +N' '+@Where
    EXEC sp_executesql @sql,N'@PageCount int OUTPUT',@PageCount OUTPUT
    SET @PageCount=(@PageCount+@PageSize-1)/@PageSize
END

--
DECLARE @TopN varchar(20),@TopN1 varchar(20)
SELECT @TopN=@PageSize,
    @TopN1=(@PageCurrent-1)*@PageSize

--
IF @PageCurrent=1
    EXEC(N'SELECT TOP '+@TopN
        +N' '+@FieldShow
        +N' FROM '+@tbname
        +N' '+@Where
        +N' '+@FieldOrder)
ELSE
BEGIN
    --alias
    IF @FieldShow=N'*'
        SET @FieldShow=N'a.*'

    DECLARE @Where1 nvarchar(4000),@Where2 nvarchar(4000),
        @s nvarchar(1000),@Field sysname
    SELECT @Where1=N'',@Where2=N'',@s=@FieldKey
    WHILE CHARINDEX(N',',@s)>0
        SELECT @Field=LEFT(@s,CHARINDEX(N',',@s)-1),	-- get a key field
            @s=STUFF(@s,1,CHARINDEX(N',',@s),N''),		-- delete the former key string and comma from @s
            @Where1=@Where1+N' AND a.'+@Field+N'=b.'+@Field,
            @Where2=@Where2+N' AND b.'+@Field+N' IS NULL',
            @Where=REPLACE(@Where,@Field,N'a.'+@Field),	-- add alias
            @FieldOrder=REPLACE(@FieldOrder,@Field,N'a.'+@Field),	-- add alias
            @FieldShow=REPLACE(@FieldShow,@Field,N'a.'+@Field)		-- add alias

    SELECT @Where=REPLACE(@Where,@s,N'a.'+@s),			-- process the last key field
        @FieldOrder=REPLACE(@FieldOrder,@s,N'a.'+@s),	-- add alias
        @FieldShow=REPLACE(@FieldShow,@s,N'a.'+@s),		-- add alias
        @Where1=STUFF(@Where1+N' AND a.'+@s+N'=b.'+@s,1,5,N''), -- delete the leading ' AND ' string   
        @Where2=CASE
            WHEN @Where='' THEN N'WHERE ('
            ELSE @Where+N' AND ('
            END
			+N'b.'+@s+N' IS NULL'+@Where2+N')'

    -- do execution
	-- to select a page rows, eg. [k*n+1, n], then select [1, k*n] first, marked as b, select Top n from Table a left join b on a.id = b.id where b.id = null
    EXEC(N'SELECT TOP '+@TopN
        +N' '+@FieldShow
        +N' FROM '+@tbname
        +N' a LEFT JOIN(SELECT TOP '+@TopN1
        +N' '+@FieldKey
        +N' FROM '+@tbname
        +N' a '+@Where
        +N' '+@FieldOrder
        +N')b ON '+@Where1
        +N' '+@Where2
        +N' '+@FieldOrder)

	-- SELECT TOP 2 s_uname, s_time FROM test_table a 
	-- LEFT JOIN(SELECT TOP 2 s_id FROM test_table a where a.s_uid > 1 ORDER BY a.s_id) b 
	-- ON a.s_id = b.s_id WHERE a.s_id > 1 AND b.s_id IS NULL ORDER BY a.s_id
END
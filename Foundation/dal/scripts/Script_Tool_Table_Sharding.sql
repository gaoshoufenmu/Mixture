use [DatabaseName]
go

DECLARE @iterator int, @tablename varchar(30), @nametail varchar(5), @sql nvarchar(2000)
set @iterator = 0
declare @rowcount int
	   
while @iterator < 256
begin
	if @iterator < 10
	begin
		set @nametail = '00' + cast(@iterator as varchar(3))
	end
	else if @iterator <100
		set @nametail = '0' + cast(@iterator as varchar(3))
	else
		set @nametail = cast(@iterator as varchar(3))
		
	set @tablename = 'Table_Slave_' + @nametail
	
	select @rowcount = count(1) from SearchHistory where sh_u_uid%256 = @iterator
	if @rowcount > 0
	begin
		EXEC SlaveTable_Create @tablename
		set @sql = 'insert into @tablenamep (
		   s_uid,
		   s_uname,
		   s_time
		) select 
		   s_uid,
		   s_uname,
		   s_time
		 from SearchHistory where s_uid%256 = @iteratorp
		'
		EXEC SP_EXECUTESQL @sql, N'@tablenamep varchar(30), @iteratorp int', @tablenamep = @tablename, @iteratorp = @iterator
	end
	
	set @iterator = @iterator + 1
end
go
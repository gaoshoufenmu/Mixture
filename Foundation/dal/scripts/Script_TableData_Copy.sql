-- Create a test table and add some test data into it
exec Proc_SlaveTable_Create 'test_table'

insert into test_table 
select 1, '1', '2016-01-01 00:00:00' union all
select 2, '2', '2016-01-02 00:00:00' union all
select 3, '3', '2016-01-03 00:00:00' union all
select 4, '4', '2016-01-04 00:00:00'


/*
查看表中数据
select * from @tb 
*/


----------------------------------------------------------------------------------------------------
-- Traverse each row of this test table
-- 声明循环用的“指针”
declare @min varchar(5)
--赋初值
select  @min=min(s_id) from test_table 
--开始循环
while @min is not null
begin
  select * from test_table where s_id = @min
  select @min=min(s_id) from test_table where s_id>@min  --更新“指针”内容，使之移到下一记录
end

--
declare @s_id char( 11 )
set rowcount 0
IF OBJECT_ID('tempdb.dbo.#mytemp') IS NOT NULL DROP TABLE dbo.#mytemp
select * into #mytemp from test_table
set rowcount 1
select @s_id = s_id from #mytemp
while @@rowcount <> 0
begin
    set rowcount 0
    select * from #mytemp where s_id = @s_id
    delete #mytemp where s_id = @s_id

    set rowcount 1
    select @s_id = s_id from #mytemp
end
set rowcount 0

-- In some cases, no unique identifier may exist. If that is the case, you can modify the temp table method to use a newly created key column. For example:
set rowcount 0
IF OBJECT_ID('tempdb.dbo.#mytemp1') IS NOT NULL DROP TABLE dbo.#mytemp1
select NULL mykey, * into #mytemp1 from test_table		-- mykey is the newly key field
set rowcount 1	-- stop processing query after 1 row returned
update #mytemp1 set mykey = 1

while @@rowcount > 0
begin
    set rowcount 0
    select * from #mytemp1 where mykey = 1
    delete #mytemp1 where mykey = 1
    set rowcount 1
    update #mytemp1 set mykey = 1
end
set rowcount 0  --To set this option off so that all rows are returned, specify SET ROWCOUNT 0.


--http://daprlabs.com/blog/blog/2014/04/03/microsoft-orleans-why-its-cool-useful/
-- 
BEGIN TRY   
    BEGIN TRANSACTION 
    DECLARE @Id INT, @LastID INT   
    SELECT @Id = MIN([s_id])-1 FROM test_table
    SELECT @LastId = MAX([s_id])-1 FROM test_table
         
    WHILE  @Id <= @LastID
        BEGIN     
            SELECT TOP 1 @Id=[s_id] FROM test_table WHERE [s_id] > @Id ORDER BY [s_id] 
            select * from test_table where s_id = @Id
			--PRINT @Id                  
        END 
    COMMIT TRANSACTION   
END TRY 
BEGIN CATCH
	ROLLBACK TRANSACTION     RAISERROR ('Error, Please try again.',16,1)
END CATCH





------------------------------------------------------------------------------------------------------
-- Copy table data using a temp table
--Container to Insert Id which are to be iterated
Declare @temp1 Table
(
  tempId int
)
--Container to Insert records in the inner select for final output
Declare @FinalTable  Table
(
  s_id int,
  s_uid int,
  s_uname varchar(100),
  s_time datetime
)
Insert into @temp1 
Select Distinct s_id From test_table

-- Keep track of @temp1 record processing
Declare @Id int
While((Select Count(*) From @temp1)>0)
Begin
   Set @Id=(Select Top 1 tempId From @temp1)

   Insert Into @FinalTable 
   Select * From test_table Where s_id=@Id and s_id %2 = 0

   Delete @temp1 Where tempId=@Id
End
Select * From @FinalTable


--------------------------------------------------------------------------------------------------------
-- Block copy
BEGIN
  --IF OBJECT_ID('tempdb.dbo.#resultTable') IS NOT NULL DROP TABLE dbo.#resultTable
	Declare @FinalTable  Table
	(
	  s_id int,
	  s_uid int,
	  s_uname varchar(100),
	  ss_time datetime
	)
	  
  INSERT @FinalTable
      SELECT s_id, s_uid, s_uname, DATEADD(year,1,s_time)
      FROM test_table
      WHERE s_id > 2

  SELECT *
  FROM @FinalTable
END     

use test
exec Proc_Table_Create 'test_table'
go

insert into test_table 
select 1, '1', '2016-01-01 00:00:00' union all
select 2, '2', '2016-01-02 00:00:00' union all
select 3, '3', '2016-01-03 00:00:00' union all
select 4, '4', '2016-01-04 00:00:00'

go
--ANSI-standard semicolon statement terminators are often omitted in T-SQL queries and many developers are unaware that this is syntax is deprecated.
--Omitting statement terminators is a dangerous practice because, even if the batch compiles, you may get unexpected results.

--The SQL Server documentation indicates that not terminating T-SQL statements with a semicolon is a deprecated feature. 
--This means that the long-term goal is to enforce use of the semicolon in a future version of the product. 
--That’s one more reason to get into the habit of terminating all of your statements, even where it’s currently not required.

--BEGIN TRY
--	BEGIN TRAN
--	SELECT 1/0 AS CauseAnException;
--	COMMIT
--END TRY
--BEGIN CATCH
--	SELECT ERROR_MESSAGE();
--	THROW
--END CATCH

--;WITH Numbers AS
--(
--    SELECT n = 1
--    UNION ALL
--    SELECT n + 1
--    FROM Numbers
--    WHERE n+1 <= 10
--)
--SELECT n
--FROM Numbers

--There are two situations in which you must use the semicolon.
 
--The first situation is where you use a Common Table Expression (CTE),
--and the CTE is not the first statement in the batch.
 
--The second is where you issue a Service Broker statement
--and the Service Broker statement is not the first statement in the batch.

declare @t table(id int)
insert into @t
select 1 union
select 3 union
select 4
 
;with cr as  -- Here semicolon is necessary
(
    select * from @t
)
select * from cr
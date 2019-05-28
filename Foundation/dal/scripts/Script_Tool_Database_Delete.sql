-- delete a database
USE master
GO

IF EXISTS(SELECT * FROM sys.databases where name = 'test')
	DROP DATABASE test
GO
:: for advanced functions, please refer to "Sqlcmd"

:: deploy a database and create some tables and store procedures
:: create database
osql -S localhost -U sjj -P 123.qianzhan -d "master" -n -i E:\Master\Comprehension\CSharp\Sql\Scripts\Script_Tool_Database_Delete.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "master" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Script_Tool_Database_Create.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt

:: create CRUD store procedures
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Table_Create.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Table_Update.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Table_Delete.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Table_Insert.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Table_Select.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt

:: create auxiliary store procedures
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Tool_Table2Class_Transfer.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Tool_Create_DALMethod.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Tool_Create_MappingClass.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt
osql -S localhost -U sjj -P 123.qianzhan -d "test" -e -i E:\Master\Comprehension\CSharp\Sql\Scripts\Proc_Tool_Create_Procedure.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt

:: deploy a test table
osql -S localhost -U sjj -P 123.qianzhan -d "test" -e -i E:\Master\Comprehension\CSharp\Sql\Scripts\Script_Database_Depoly.sql>>E:\Master\Comprehension\CSharp\Sql\Scripts\log.txt


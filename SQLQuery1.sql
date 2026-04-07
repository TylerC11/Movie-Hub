

RESTORE DATABASE IMDB FROM DISK = 'C:\Users\Mitch Barron\source\repos\PROG2500\IMDB_Project.bak'
WITH MOVE 'IMDB_Project' TO 'C:\Users\Mitch Barron\Source\repos\PROG2500\IMDB_Project.mdf',
MOVE 'IMDB_Project_log' TO 'C:\Users\Mitch Barron\Source\repos\PROG2500\IMDB_Project_log.ldf',
RECOVERY, REPLACE, STATS = 10;
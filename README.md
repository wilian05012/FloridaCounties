# FloridaCounties
Getting Florida counties maps in to SQL Server Database.

## Description
A solution containing two projects: FloridaCounties is a simple C# console application targetting .NET Core 3.1 and a XUnit Test project with little value. 
The console project uses EntityFrameworkCore with NetTopologySuite to populate a table containing mainly the name of each county with its geographic limits in the form of a polygon.

The data is obtained from the Official Geographic Data Portal of The State of Florida.

## Instructions:
1. Clone the project in Visual Studio.
2. Restore the nuget packages packages and build the project.
3. Verifiy that the solution starting project is FloridaCounties.
4. Open the Package Manager Console and run Update-Database. 
    - *(This will create the database in your (localdb)\MSSQLLocal instance, verify it by opening the SQL Server Objct Explorer)*
5. Run the project. 
    - *(This will query the portal for the data, clear the content of the table tblCounties and populat it with the data retrieved)*


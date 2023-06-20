# DbUp Overview

## MSSQL Connection Configuration.
The service receives the MSSQL connection string as a command line argument. The following fields are required:

- Server=[server]
- Database=[database]
- User ID=[username]
- Password=[password]

An example of the command line argument passed in would be:

```
"Server=[server]; Database=[database]; User ID=[username]; Password=[password"
```

## MSSQL scripts.
The Scripts folder contains a folder for each year, which contains the MSSQL scripts for that year. The naming convention for a MSSQL scripts is [year]_[time]_ScriptName.sql. The scripts are executed in ascending order, so it's important to make sure that the year and time of the script name reflects the intended order of execution.

Once a script has run, the DbUp services writes an entiry to the SchemaVersions table on the database. This is used to track whether a script has been run. Once a script name is added to SchemaVersions, the DbUp service will not run that script again unless you remove the scripts entry from SchemaVersions.

You can add a new script by placing it in the relevant year folder in Scripts. The DbUp service has the following flag set in the .cspoj file in order to import it automatically:

```
  <ItemGroup>
    <EmbeddedResource Include="**\*.sql" />
  </ItemGroup>
```

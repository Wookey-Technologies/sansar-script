# Safe Data Store

Persistent script storage is stored in tables, and each table has their own unique global key. Because keys are global and permanent, it is paramount that the key you pick for your table is random enough that it cannot be accidentally picked by someone else in a script anywhere in Sansar, at any time now and until the end of time.

This script is meant to provide an easy way to create unique keys for your tables, and allow other scripts to create tables dynamically safely. 9 Secret numbers must be picked that will form a database namespace for all your tables. A database here is just a prefix with all the numbers for each table you create. The database name is used by other scripts to find the right namespace within the scene, but it can be any name, and be a different name, in each scene you share the database with, only the numbers act as the database namespace.

Other scripts must be compatible with this script in order to utilize this safe storage mechanism, they will need to ask you for a database name. You place this script in the scene only once per database. To share a database, just give this script to someone else along with all the numbers you used for your own database.

It is not recommended to use this script in products you sell, because anyone can see the parameters, get your database key, and potentially make edits to your database with their own scripts.

The code can be found here:
https://github.com/darwinrecreant/sansar-script/tree/master/Users/evoav/SafeDataStore

Its best to have a global way to share table keys safely, so share this script with other scripters and let them implement the same solution so that tables can be shared between scripts safely.

## For Scripters

You can extend this class to add functionality, but do not change the main api. It was kept simple intentionally so that all other scripts now and in the future could work with it.

To use the database you must find the database reflective like so:

```csharp
using Sansar.Simulation;
using System;
using Sansar.Script;

namespace Examples {
  public interface IDatabase
  {
    Func<string, DataStore> GetCreateTable(string database);
    DataStore GetSchemaAccess(string database, string schemaPassword);
    bool IsDatabase(string name);
  }
  class ExampleDatabase : SceneObjectScript {
    public string DatabaseName;
    Func<string, DataStore> createTable = null;

    override public void Init() {
      float waitAmount = 0.05f;
      while (waitAmount < 4) {
        foreach (IDatabase db in ScenePrivate.FindReflective<IDatabase>("Persistence.Database")) {
          createTable = db.GetCreateTable(DatabaseName);
          if (createTable != null) break;
        }
        Wait(waitAmount);
        waitAmount += waitAmount;
      }

      if (createTable == null) {
        Log.Write(LogLevel.Warning, ObjectPrivate.Name, "Database not found: " + DatabaseName);
        return;
      }

      DataStore users = createTable("users");
      DataStore highScore = createTable("highscore");
      // ...
    }
  }
}
```
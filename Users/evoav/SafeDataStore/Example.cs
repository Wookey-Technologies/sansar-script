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
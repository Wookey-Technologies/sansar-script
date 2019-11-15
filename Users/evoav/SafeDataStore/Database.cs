using Sansar.Simulation;
using Sansar.Script;
using System;

namespace Persistence {

  interface IDatabase
  {
    Func<string, DataStore> GetCreateTable(string database);
    bool IsDatabase(string name);
  }

  [Tooltip(@"Persistent script storage is stored in tables, and each table has their own unique global key. Because keys are global and permanent, it is paramount that the key you pick for your table is random enough that it cannot be accidentally picked by someone else in a script anywhere in Sansar, at any time now and until the end of time.

This script is meant to provide an easy way to create unique keys for your tables, and allow other scripts to create tables dynamically safely. 9 Secret numbers must be picked that will form a database namespace for all your tables. A database here is just a prefix with all the numbers for each table you create. The database name is used by other scripts to find the right namespace within the scene, but it can be any name, and be a different name, in each scene you share the database with, only the numbers act as the database namespace.

Other scripts must be compatible with this script in order to utilize this safe storage mechanism, they will need to ask you for a database name. You place this script in the scene only once per database. To share a database, just give this script to someone else along with all the numbers you used for your own database.

It is not recommended to use this script in products you sell, because anyone can see the parameters, get your database key, and potentially make edits to your database with their own scripts.

The code can be found here:
https://github.com/darwinrecreant/sansar-script/tree/master/Users/evoav/SafeDataStore

Its best to have a global way to share table keys safely, so share this script with other scripters and let them implement the same solution so that tables can be shared between scripts safely.")]
  [DisplayName("Database")]
  [RegisterReflective]
  class Database : SceneObjectScript, IDatabase
  {
    [Tooltip("This is the identifier you use in other scripts to target the right table namesapce. This is a required field.")]
    [DisplayName("Database Name")]
    [EditorVisible(true)]
    readonly string DatabaseName;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #1")]
    [EditorVisible(true)]
    readonly int Salt1;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #2")]
    [EditorVisible(true)]
    readonly int Salt2;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #3")]
    [EditorVisible(true)]
    readonly int Salt3;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #4")]
    [EditorVisible(true)]
    readonly int Salt4;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #5")]
    [EditorVisible(true)]
    readonly int Salt5;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #6")]
    [EditorVisible(true)]
    readonly int Salt6;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #7")]
    [EditorVisible(true)]
    readonly int Salt7;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #8")]
    [EditorVisible(true)]
    readonly int Salt8;
    [Tooltip("None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.")]
    [Range(0, 100)]
    [DisplayName("Secret Number #9")]
    [EditorVisible(true)]
    readonly int Salt9;

    bool IsValid
    {
      get
      {
        return Salt1 != 0 && Salt1 != 100
          && Salt2 != 0 && Salt2 != 100
          && Salt3 != 0 && Salt3 != 100
          && Salt4 != 0 && Salt4 != 100
          && Salt5 != 0 && Salt5 != 100
          && Salt6 != 0 && Salt6 != 100
          && Salt7 != 0 && Salt7 != 100
          && Salt8 != 0 && Salt8 != 100
          && Salt9 != 0 && Salt9 != 100
          && !(Salt1 == Salt2 && Salt1 == Salt3 && Salt1 == Salt4 && Salt1 == Salt5 && Salt1 == Salt6 && Salt1 == Salt7 && Salt1 == Salt8 && Salt1 == Salt9);
      }
    }

    string salt = "";
    override public void Init()
    {
      if (DatabaseName == "")
      {
        Log.Write(LogLevel.Warning, ObjectPrivate.Name, "Database must have a name.");
        Unregister();
        return;
      }
      if (!IsValid)
      {
        Log.Write(LogLevel.Warning, ObjectPrivate.Name, "None of the sceret numbers are allowed to be 0 or 100, and they are not allowed to all be the same number.");
        Unregister();
        return;
      }
      salt = Salt1.ToString("00") + Salt2.ToString("00") + Salt3.ToString("00") + Salt4.ToString("00") + Salt5.ToString("00") + Salt6.ToString("00") + Salt7.ToString("00") + Salt8.ToString("00") + Salt9.ToString("00");

    }

    public bool IsDatabase(string name)
    {
      return DatabaseName == name;
    }

    public Func<string, DataStore> GetCreateTable(string database)
    {
      if (database != DatabaseName) return null;
      return (string tableName) => ScenePrivate.CreateDataStore(salt + tableName);
    }
  }
}
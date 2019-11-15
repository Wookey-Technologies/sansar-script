using Sansar.Simulation;
using Sansar.Script;

namespace Persistence {
  interface ISafeDataStore
  {
    DataStore CreateDataStore(string tableName);
    bool IsDatabase(string name);
  }

  [RegisterReflective]
  class SafeDataStore : SceneObjectScript, ISafeDataStore
  {
    public readonly string DatabaseName;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt1;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt2;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt3;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt4;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt5;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt6;
    [Range(0, 100)]
    [DisplayName("Secret Number")]
    public readonly int Salt7;

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
          && !(Salt1 == Salt2 && Salt1 == Salt3 && Salt1 == Salt4 && Salt1 == Salt5 && Salt1 == Salt6 && Salt1 == Salt7);
      }
    }

    string salt = "";
    override public void Init()
    {
      if (!IsValid)
      {
        Log.Write(LogLevel.Warning, ObjectPrivate.Name, "None of the sceret numbers are allowed to be 0 or 100");
        Unregister();
        return;
      }
      salt = Salt1.ToString("00") + Salt2.ToString("00") + Salt3.ToString("00") + Salt4.ToString("00") + Salt5.ToString("00") + Salt6.ToString("00") + Salt7.ToString("00");

    }

    public bool IsDatabase(string name)
    {
      return DatabaseName == name;
    }

    public DataStore CreateDataStore(string database, string tableName)
    {
      if (database != DatabaseName) return null;
      return ScenePrivate.CreateDataStore(salt + tableName);
    }
  }
}
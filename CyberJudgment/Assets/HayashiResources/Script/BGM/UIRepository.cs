using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

public class UIRepository
{
    private SQLiteConnection _connection;

    public UIRepository(string databasePath)
    {
        _connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _connection.CreateTable<UIButtonLink>();
    }

    public List<UIButtonLink> GetAllButtonLinks()
    {
        return _connection.Table<UIButtonLink>().ToList();
    }

    public UIButtonLink GetButtonLinkByName(string buttonName)
    {
        return _connection.Table<UIButtonLink>().Where(x => x.ButtonName == buttonName).FirstOrDefault();
    }

    public void AddButtonLink(UIButtonLink buttonLink)
    {
        _connection.Insert(buttonLink);
    }
}

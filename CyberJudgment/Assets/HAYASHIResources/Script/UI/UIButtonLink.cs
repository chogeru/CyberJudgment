using SQLite4Unity3d;

public class UIButtonLink
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string ButtonName { get; set; }
    public string LinkedUIObjectName { get; set; }
}

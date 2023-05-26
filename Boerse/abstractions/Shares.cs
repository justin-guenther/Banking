namespace Boerse.abstractions;

public class Shares
{
    public string Name { get; set; } = null!;
    public string NameShort { get; set; } = null!;
    public int Amount { get; set; }
    public int Price { get; set; }
}
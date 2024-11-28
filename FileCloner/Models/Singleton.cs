namespace FileCloner.Models;

public class Singleton
{
    private static readonly Lazy<Singleton> s_instance = new(() => new Singleton());
    private string? _userName;

    // Private constructor to prevent instantiation
    private Singleton() { }

    // Property to get the singleton instance
    public static Singleton Instance => s_instance.Value;

    // Property for UserName
    public string UserName
    {
        get => _userName ?? "Server"; // Return an empty string if null
        set => _userName = value;
    }
}

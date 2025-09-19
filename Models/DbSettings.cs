namespace Models;

public sealed class DbSettings
{
    public string? MigrationConnectionString { get; set; }
    public string? ConnectionString { get; set; }
}

using FluentMigrator.Runner.VersionTableInfo;

namespace Migrations;

[VersionTableMetaData]
public class VersionTable : IVersionTableMetaData
{
    public string SchemaName => "public";
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string AppliedOnColumnName => "applied_on";
    public string DescriptionColumnName => "description";

    // Новые свойства, которые требуют версии FluentMigrator 6.x:
    public string? UniqueIndexName => "ux_version_info_version"; // можно и null, но так понятнее
    public bool CreateWithPrimaryKey => false;                   // создаём без PK (по умолчанию ок)

    public object? ApplicationContext { get; set; }
    public bool OwnsSchema => false;
}

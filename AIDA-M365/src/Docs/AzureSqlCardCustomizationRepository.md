# Azure SQL Card Customization Repository

Blazor WebAssembly should not connect directly to Azure SQL. Host this repository in a server/API project and expose it to the client through `POST api/card-customizations`, which is what `CardCustomizationApiRepository` calls.

```csharp
using System.Data;
using AIDA.M365.Models;
using Dapper;

public sealed class DapperCardCustomizationRepository : ICardCustomizationRepository
{
    private readonly IDbConnection _connection;

    public DapperCardCustomizationRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task SaveAsync(
        CardCustomization customization,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        MERGE dbo.CardCustomizations AS target
        USING (SELECT @GraphEventId AS GraphEventId) AS source
        ON target.GraphEventId = source.GraphEventId
        WHEN MATCHED THEN UPDATE SET
            SectionKey = @SectionKey,
            StartUtc = @StartUtc,
            EndUtc = @EndUtc,
            ColorHex = @ColorHex,
            Priority = @Priority,
            SmartSummary = @SmartSummary,
            ActionItemsJson = @ActionItemsJson,
            UpdatedUtc = @UpdatedUtc
        WHEN NOT MATCHED THEN INSERT
            (MetadataId, GraphEventId, SectionKey, StartUtc, EndUtc, ColorHex, Priority, SmartSummary, ActionItemsJson, UpdatedUtc)
        VALUES
            (@MetadataId, @GraphEventId, @SectionKey, @StartUtc, @EndUtc, @ColorHex, @Priority, @SmartSummary, @ActionItemsJson, @UpdatedUtc);
        """;

        var parameters = new
        {
            customization.MetadataId,
            customization.GraphEventId,
            customization.SectionKey,
            customization.StartUtc,
            customization.EndUtc,
            customization.ColorHex,
            customization.Priority,
            customization.SmartSummary,
            ActionItemsJson = JsonSerializer.Serialize(customization.ActionItems),
            customization.UpdatedUtc
        };

        await _connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }
}
```

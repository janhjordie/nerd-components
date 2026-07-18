namespace TheNerdCollective.Integrations.Dar.IntegrationTests.Migration;

internal sealed record MigrationProbeRow(
    string Operation,
    bool DatafordelerOk,
    bool DatafordelerBlocked,
    string DatafordelerDetail,
    bool WithDawaOk,
    string WithDawaDetail,
    MigrationDataSource InferredSource,
    bool FallbackRequired);

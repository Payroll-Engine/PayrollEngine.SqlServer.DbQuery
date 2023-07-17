
namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestVersionSettings
    {
        internal string TableName { get; init; }
        internal string MajorVersionColumnName { get; init; }
        internal string MinorVersionColumnName { get; init; }
        internal string SubVersionColumnName { get; init; }
        internal string MinVersion { get; init; }
    }
}

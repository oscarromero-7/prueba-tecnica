using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace PruebaTecnica.Infrastructure;

public static class DbUpdateExceptionExtensions
{
    private const int SqliteConstraintUnique = 2067;

    public static bool IsUniqueConstraintViolation(this DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique };
}

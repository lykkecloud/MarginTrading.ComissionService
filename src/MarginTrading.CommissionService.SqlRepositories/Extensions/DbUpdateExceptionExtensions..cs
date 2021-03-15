// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.Common.MsSql;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MarginTrading.CommissionService.SqlRepositories.Extensions
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool ValueAlreadyExistsException(this DbUpdateException e)
        {
            return e.InnerException is SqlException sqlException &&
                   (sqlException.Number == MsSqlErrorCodes.PrimaryKeyConstraintViolation ||
                    sqlException.Number == MsSqlErrorCodes.DuplicateIndex);
        }
    }
}
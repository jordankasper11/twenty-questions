using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TwentyQuestions.Data.Extensions
{
    public static class DataRowExtensions
    {
        public static DateTime? GetUtcDateTime(this DataRow dataRow, string columnName)
        {
            var value = dataRow.Field<DateTime?>(columnName);

            if (value != null)
                value = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);

            return value;
        }
    }
}

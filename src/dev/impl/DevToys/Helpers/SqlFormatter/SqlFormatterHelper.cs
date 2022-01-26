#nullable enable

using System;
using DevToys.Helpers.SqlFormatter.Core;
using DevToys.Helpers.SqlFormatter.Languages;

namespace DevToys.Helpers.SqlFormatter
{
    internal static class SqlFormatterHelper
    {
        internal static string Format(string sql, SqlLanguage language, SqlFormatterOptions options)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            Formatter formatter = language switch
            {
                SqlLanguage.Sql => new StandardSqlFormatter(),
                SqlLanguage.Tsql => new TSqlFormatter(),
                SqlLanguage.Spark => new SparkSqlFormatter(),
                SqlLanguage.RedShift => new RedshiftFormatter(),
                SqlLanguage.PostgreSql => new PostgreSqlFormatter(),
                SqlLanguage.PlSql => new PlSqlFormatter(),
                SqlLanguage.N1ql => new N1qlFormatter(),
                SqlLanguage.MySql => new MySqlFormatter(),
                SqlLanguage.MariaDb => new MariaDbFormatter(),
                SqlLanguage.Db2 => new Db2Formatter(),
                _ => throw new NotSupportedException(),
            };

            return formatter.Format(sql, options);
        }
    }
}

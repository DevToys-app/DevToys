using System;
using DevToys.Helpers.SqlFormatter;

namespace DevToys.Models
{
    public class SqlLanguageDisplayPair : IEquatable<SqlLanguageDisplayPair>
    {
        private static SqlFormatterStrings Strings => LanguageManager.Instance.SqlFormatter;

        public static readonly SqlLanguageDisplayPair Db2 = new(Strings.SqlLanguageDb2, SqlLanguage.Db2);

        public static readonly SqlLanguageDisplayPair MariaDb = new(Strings.SqlLanguageMariaDb, SqlLanguage.MariaDb);

        public static readonly SqlLanguageDisplayPair MySql = new(Strings.SqlLanguageMySql, SqlLanguage.MySql);

        public static readonly SqlLanguageDisplayPair N1ql = new(Strings.SqlLanguageN1ql, SqlLanguage.N1ql);

        public static readonly SqlLanguageDisplayPair PlSql = new(Strings.SqlLanguagePlSql, SqlLanguage.PlSql);

        public static readonly SqlLanguageDisplayPair PostgreSql = new(Strings.SqlLanguagePostgreSql, SqlLanguage.PostgreSql);

        public static readonly SqlLanguageDisplayPair RedShift = new(Strings.SqlLanguageRedShift, SqlLanguage.RedShift);

        public static readonly SqlLanguageDisplayPair Spark = new(Strings.SqlLanguageSpark, SqlLanguage.Spark);

        public static readonly SqlLanguageDisplayPair Sql = new(Strings.SqlLanguageSql, SqlLanguage.Sql);

        public static readonly SqlLanguageDisplayPair Tsql = new(Strings.SqlLanguageTsql, SqlLanguage.Tsql);

        public string DisplayName { get; }

        public SqlLanguage Value { get; }

        private SqlLanguageDisplayPair(string displayName, SqlLanguage value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(SqlLanguageDisplayPair other)
        {
            return other.Value == Value;
        }
    }
}

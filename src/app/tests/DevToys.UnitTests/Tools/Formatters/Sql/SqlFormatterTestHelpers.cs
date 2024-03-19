using System.Text.RegularExpressions;
using DevToys.Tools.Helpers.SqlFormatter.Core;
using DevToys.Tools.Helpers.SqlFormatter;
using Indentation = DevToys.Tools.Models.Indentation;

namespace DevToys.UnitTests.Tools.Formatters.Sql;
internal static class SqlFormatterTestHelpers
{
    /// <summary>
    /// Core tests for all SQL formatters
    /// </summary>
    internal static void BehavesLikeSqlFormatter(Formatter formatter)
    {
        SupportsComments(formatter);
        SupportsConfigOptions(formatter);
        SupportsOperators(formatter, "=", "+", "-", "*", "/", "<>", ">", "<", ">=", "<=");

        // does nothing with empty input
        string input = string.Empty;
        string expectedResult = string.Empty;
        string output = formatter.Format(input);
        output.Should().Be(expectedResult);

        // formats lonely semicolon
        input = ";";
        expectedResult = ";";
        AssertFormat(formatter, input, expectedResult);

        // formats simple SELECT query
        input = "SELECT count(*),Column1 FROM Table1;";
        expectedResult =
@"SELECT
  count(*),
  Column1
FROM
  Table1;";
        AssertFormat(formatter, input, expectedResult);

        // formats complex SELECT
        input = "SELECT DISTINCT name, ROUND(age/7) field1, 18 + 20 AS field2, 'some string' FROM foo;";
        expectedResult =
@"SELECT
  DISTINCT name,
  ROUND(age / 7) field1,
  18 + 20 AS field2,
  'some string'
FROM
  foo;";
        AssertFormat(formatter, input, expectedResult);

        // formats SELECT with complex WHERE
        input =
@"SELECT * FROM foo WHERE Column1 = 'testing'
AND ( (Column2 = Column3 OR Column4 >= NOW()) );";
        expectedResult =
@"SELECT
  *
FROM
  foo
WHERE
  Column1 = 'testing'
  AND (
    (
      Column2 = Column3
      OR Column4 >= NOW()
    )
  );";
        AssertFormat(formatter, input, expectedResult);

        // formats SELECT with top level reserved words
        input =
@"SELECT * FROM foo WHERE name = 'John' GROUP BY some_column
HAVING column > 10 ORDER BY other_column LIMIT 5;";
        expectedResult =
@"SELECT
  *
FROM
  foo
WHERE
  name = 'John'
GROUP BY
  some_column
HAVING
  column > 10
ORDER BY
  other_column
LIMIT
  5;";
        AssertFormat(formatter, input, expectedResult);

        // formats LIMIT with two comma-separated values on single line
        input = @"LIMIT 5, 10;";
        expectedResult =
@"LIMIT
  5, 10;";
        AssertFormat(formatter, input, expectedResult);

        // formats LIMIT of single value followed by another SELECT using commas
        input = @"LIMIT 5; SELECT foo, bar;";
        expectedResult =
@"LIMIT
  5;
SELECT
  foo,
  bar;";
        AssertFormat(formatter, input, expectedResult);

        // formats LIMIT of single value and OFFSET
        input = @"LIMIT 5 OFFSET 8;";
        expectedResult =
@"LIMIT
  5 OFFSET 8;";
        AssertFormat(formatter, input, expectedResult);

        // recognizes LIMIT in lowercase
        input = @"limit 5, 10;";
        expectedResult =
@"limit
  5, 10;";
        AssertFormat(formatter, input, expectedResult);

        // preserves case of keywords
        input = @"select distinct * frOM foo WHERe a > 1 and b = 3";
        expectedResult =
@"select
  distinct *
frOM
  foo
WHERe
  a > 1
  and b = 3";
        AssertFormat(formatter, input, expectedResult);

        // formats SELECT query with SELECT query inside it
        input = @"SELECT *, SUM(*) AS sum FROM (SELECT * FROM Posts LIMIT 30) WHERE a > b";
        expectedResult =
@"SELECT
  *,
  SUM(*) AS sum
FROM
  (
    SELECT
      *
    FROM
      Posts
    LIMIT
      30
  )
WHERE
  a > b";
        AssertFormat(formatter, input, expectedResult);

        // formats simple INSERT query
        input = @"INSERT INTO Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');";
        expectedResult =
@"INSERT INTO
  Customers (ID, MoneyBalance, Address, City)
VALUES
  (12, -123.4, 'Skagen 2111', 'Stv');";
        AssertFormat(formatter, input, expectedResult);

        // formats open paren after comma
        input = @"WITH TestIds AS (VALUES (4),(5), (6),(7),(9),(10),(11)) SELECT * FROM TestIds;";
        expectedResult =
@"WITH TestIds AS (
  VALUES
    (4),
    (5),
    (6),
    (7),
    (9),
    (10),
    (11)
)
SELECT
  *
FROM
  TestIds;";
        AssertFormat(formatter, input, expectedResult);

        // keeps short parenthesized list with nested parenthesis on single line
        input = @"SELECT (a + b * (c - NOW()));";
        expectedResult =
@"SELECT
  (a + b * (c - NOW()));";
        AssertFormat(formatter, input, expectedResult);

        // breaks long parenthesized lists to multiple lines
        input =
@"INSERT INTO some_table (id_product, id_shop, id_currency, id_country, id_registration) (
SELECT IF(dq.id_discounter_shopping = 2, dq.value, dq.value / 100),
IF (dq.id_discounter_shopping = 2, 'amount', 'percentage') FROM foo);";
        expectedResult =
@"INSERT INTO
  some_table (
    id_product,
    id_shop,
    id_currency,
    id_country,
    id_registration
  ) (
    SELECT
      IF(
        dq.id_discounter_shopping = 2,
        dq.value,
        dq.value / 100
      ),
      IF (
        dq.id_discounter_shopping = 2,
        'amount',
        'percentage'
      )
    FROM
      foo
  );";
        AssertFormat(formatter, input, expectedResult);

        // formats simple UPDATE query
        input = @"UPDATE Customers SET ContactName='Alfred Schmidt', City='Hamburg' WHERE CustomerName='Alfreds Futterkiste';";
        expectedResult =
@"UPDATE
  Customers
SET
  ContactName = 'Alfred Schmidt',
  City = 'Hamburg'
WHERE
  CustomerName = 'Alfreds Futterkiste';";
        AssertFormat(formatter, input, expectedResult);

        // formats simple DELETE query
        input = @"DELETE FROM Customers WHERE CustomerName='Alfred' AND Phone=5002132;";
        expectedResult =
@"DELETE FROM
  Customers
WHERE
  CustomerName = 'Alfred'
  AND Phone = 5002132;";
        AssertFormat(formatter, input, expectedResult);

        // formats simple DROP query
        input = @"DROP TABLE IF EXISTS admin_role;";
        expectedResult = @"DROP TABLE IF EXISTS admin_role;";
        AssertFormat(formatter, input, expectedResult);

        // formats incomplete query
        input = @"SELECT count(";
        expectedResult =
@"SELECT
  count(";
        AssertFormat(formatter, input, expectedResult);

        // formats UPDATE query with AS part
        input = @"UPDATE customers SET total_orders = order_summary.total  FROM ( SELECT * FROM bank) AS order_summary";
        expectedResult =
@"UPDATE
  customers
SET
  total_orders = order_summary.total
FROM
  (
    SELECT
      *
    FROM
      bank
  ) AS order_summary";
        AssertFormat(formatter, input, expectedResult);

        // formats top-level and newline multi-word reserved words with inconsistent spacing
        input = "SELECT * FROM foo LEFT \t   \r\n JOIN bar ORDER \r\n BY blah";
        expectedResult = "SELECT\r\n  *\r\nFROM\r\n  foo\r\n  LEFT \t   \r\n JOIN bar\r\nORDER \r\n BY\r\n  blah";
        AssertFormat(formatter, input, expectedResult);

        // formats long double parenthized queries to multiple lines
        input = @"((foo = '0123456789-0123456789-0123456789-0123456789'))";
        expectedResult =
@"(
  (
    foo = '0123456789-0123456789-0123456789-0123456789'
  )
)";
        AssertFormat(formatter, input, expectedResult);

        // formats short double parenthized queries to one line
        input = @"((foo = 'bar'))";
        expectedResult = @"((foo = 'bar'))";
        AssertFormat(formatter, input, expectedResult);

        // formats logical operators
        AssertFormat(formatter, @"foo ALL bar", @"foo ALL bar");
        AssertFormat(formatter, @"foo = ANY (1, 2, 3)", @"foo = ANY (1, 2, 3)");
        AssertFormat(formatter, @"EXISTS bar", @"EXISTS bar");
        AssertFormat(formatter, @"foo IN (1, 2, 3)", @"foo IN (1, 2, 3)");
        AssertFormat(formatter, @"foo LIKE 'hello%'", @"foo LIKE 'hello%'");
        AssertFormat(formatter, @"foo IS NULL", @"foo IS NULL");
        AssertFormat(formatter, @"UNIQUE foo", @"UNIQUE foo");

        // formats AND/OR operators
        AssertFormat(formatter, "foo AND bar", "foo\r\nAND bar");
        AssertFormat(formatter, "foo OR bar", "foo\r\nOR bar");

        // keeps separation between multiple statements
        AssertFormat(formatter, "foo;bar;", "foo;\r\nbar;");
        AssertFormat(formatter, "foo\r\n;bar;", "foo;\r\nbar;");
        AssertFormat(formatter, "foo\n\n\n;bar;\n\n", "foo;\r\nbar;");
        input =
@"SELECT count(*),Column1 FROM Table1;
SELECT count(*),Column1 FROM Table2;";
        expectedResult =
@"SELECT
  count(*),
  Column1
FROM
  Table1;
SELECT
  count(*),
  Column1
FROM
  Table2;";
        AssertFormat(formatter, input, expectedResult);

        // formats unicode correctly
        input = @"SELECT 结合使用, тест FROM table;";
        expectedResult =
@"SELECT
  结合使用,
  тест
FROM
  table;";
        AssertFormat(formatter, input, expectedResult);

        // correctly indents create statement after select
        input =
@"SELECT * FROM test;
CREATE TABLE TEST(id NUMBER NOT NULL, col1 VARCHAR2(20), col2 VARCHAR2(20));";
        expectedResult =
@"SELECT
  *
FROM
  test;
CREATE TABLE TEST(
  id NUMBER NOT NULL,
  col1 VARCHAR2(20),
  col2 VARCHAR2(20)
);";
        AssertFormat(formatter, input, expectedResult);

        // correctly handles floats as single tokens
        input = @"SELECT 1e-9 AS a, 1.5e-10 AS b, 3.5E12 AS c, 3.5e12 AS d;";
        expectedResult =
@"SELECT
  1e-9 AS a,
  1.5e-10 AS b,
  3.5E12 AS c,
  3.5e12 AS d;";
        AssertFormat(formatter, input, expectedResult);

        // does not split UNION ALL in half
        input =
@"SELECT * FROM tbl1
UNION ALL
SELECT * FROM tbl2;";
        expectedResult =
@"SELECT
  *
FROM
  tbl1
UNION ALL
SELECT
  *
FROM
  tbl2;";
        AssertFormat(formatter, input, expectedResult);

        // correctly formats hardcoded values in from statement
        input =
@"SELECT Id FROM (values(1),(2),(3), (4)) as  b  (id)";
        expectedResult =
@"SELECT
  Id
FROM
  (
    values
      (1),
      (2),
      (3),
      (4)
  ) as b (id)";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Shared tests for MySQL and MariaDB
    /// </summary>
    internal static void BehavesLikeMariaDbFormatter(Formatter formatter)
    {
        BehavesLikeSqlFormatter(formatter);
        SupportsCase(formatter);
        SupportsCreateTable(formatter);
        SupportsAlterTable(formatter);
        SupportsStrings(formatter, "\"\"", "''", "``");
        SupportsBetween(formatter);
        SupportsComments(formatter);
        SupportsConfigOptions(formatter);
        SupportsOperators(
            formatter,
            "%",
            "&",
            "|",
            "^",
            "~",
            "!=",
            "!",
            "<=>",
            "<<",
            ">>",
            "&&",
            "||",
            ":=");
        SupportsJoin(
            formatter,
            without: new[] { "FULL" },
            additionally: new[]
            {
                "STRAIGHT_JOIN",
                "NATURAL LEFT JOIN",
                "NATURAL LEFT OUTER JOIN",
                "NATURAL RIGHT JOIN",
                "NATURAL RIGHT OUTER JOIN"
            });

        // supports # comments
        string input = "SELECT a # comment\r\nFROM b # comment";
        string expectedResult =
@"SELECT
  a # comment
FROM
  b # comment";
        AssertFormat(formatter, input, expectedResult);

        // supports @variables
        input = "SELECT @foo, @bar";
        expectedResult =
@"SELECT
  @foo,
  @bar";
        AssertFormat(formatter, input, expectedResult);

        // supports setting variables: @var :=
        input = "SET @foo := (SELECT * FROM tbl);";
        expectedResult =
@"SET
  @foo := (
    SELECT
      *
    FROM
      tbl
  );";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests for standard -- and /* *\/ comments
    /// </summary>
    internal static void SupportsComments(Formatter formatter)
    {
        // formats SELECT query with different comments
        string input = @"
SELECT
/*
 * This is a block comment
 */
* FROM
-- This is another comment
MyTable -- One final comment
WHERE 1 = 2;";

        string expectedResult =
@"SELECT
  /*
   * This is a block comment
   */
  *
FROM
  -- This is another comment
  MyTable -- One final comment
WHERE
  1 = 2;";
        AssertFormat(formatter, input, expectedResult);

        // maintains block comment indentation
        input =
@"SELECT
  /*
   * This is a block comment
   */
  *
FROM
  MyTable
WHERE
  1 = 2";

        expectedResult = input;
        AssertFormat(formatter, input, expectedResult);

        // formats tricky line comments
        input = "SELECT a--comment, here\r\nFROM b--comment";

        expectedResult =
@"SELECT
  a --comment, here
FROM
  b --comment";
        AssertFormat(formatter, input, expectedResult);

        // formats line comments followed by semicolon
        input = @"
SELECT a FROM b
--comment
;";

        expectedResult =
@"SELECT
  a
FROM
  b --comment
;";
        AssertFormat(formatter, input, expectedResult);

        // formats line comments followed by comma
        input = @"
SELECT a --comment
, b";

        expectedResult =
@"SELECT
  a --comment
,
  b";
        AssertFormat(formatter, input, expectedResult);

        // formats line comments followed by close-paren
        input = "SELECT ( a --comment\r\n )";

        expectedResult =
@"SELECT
  (a --comment
)";
        AssertFormat(formatter, input, expectedResult);

        // formats line comments followed by open-paren
        input = "SELECT a --comment\r\n()";

        expectedResult =
@"SELECT
  a --comment
  ()";
        AssertFormat(formatter, input, expectedResult);

        // recognizes line-comments with Windows line-endings (converts them to UNIX)
        input = "SELECT * FROM\r\n-- line comment 1\r\nMyTable -- line comment 2\r\n";
        expectedResult = "SELECT\r\n  *\r\nFROM\r\n  -- line comment 1\r\n  MyTable -- line comment 2";
        AssertFormat(formatter, input, expectedResult);

        // formats query that ends with open comment
        input = @"
SELECT count(*)
/*Comment";

        expectedResult =
@"SELECT
  count(*)
  /*Comment";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests for all the config options
    /// </summary>
    internal static void SupportsConfigOptions(Formatter formatter)
    {
        // supports indent option
        string input = @"SELECT count(*),Column1 FROM Table1;";

        string expectedResult =
@"SELECT
    count(*),
    Column1
FROM
    Table1;";

        string output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.FourSpaces, Uppercase: false));
        output.Should().Be(expectedResult);

        // supports linesBetweenQueries option
        input = @"SELECT * FROM foo; SELECT * FROM bar;";

        expectedResult =
@"SELECT
  *
FROM
  foo;

SELECT
  *
FROM
  bar;";

        output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.TwoSpaces, Uppercase: false, LinesBetweenQueries: 2));
        output.Should().Be(expectedResult);

        // supports uppercase option
        input = @"select distinct * frOM foo left join bar WHERe cola > 1 and colb = 3";

        expectedResult =
@"SELECT
  DISTINCT *
FROM
  foo
  LEFT JOIN bar
WHERE
  cola > 1
  AND colb = 3";

        output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.TwoSpaces, Uppercase: true));
        output.Should().Be(expectedResult);

        // supports indent option one tab
        input = @"SELECT count(*),Column1 FROM Table1;";

        expectedResult =
string.Format(@"SELECT
{0}count(*),
{1}Column1
FROM
{2}Table1;", "\t", "\t", "\t");

        output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.OneTab, Uppercase: false));
        output.Should().Be(expectedResult);

        // supports leading comma option
        input = """
select f.title,
f.replacement_cost,
f.rental_rate,
r.return_date AS total_rentals FROM film f
GROUP by
f.title,
f.replacement_cost,
f.rental_rate
""";

        expectedResult =
"""
SELECT
    f.title,
    f.replacement_cost,
    f.rental_rate,
    r.return_date AS total_rentals
FROM
    film f
GROUP BY
    f.title,
    f.replacement_cost,
    f.rental_rate
""";

        output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.FourSpaces, Uppercase: true, UseLeadingComma: false));
        output.Should().Be(expectedResult);

        expectedResult =
"""
SELECT
    f.title
    , f.replacement_cost
    , f.rental_rate
    , r.return_date AS total_rentals
FROM
    film f
GROUP BY
    f.title
    , f.replacement_cost
    , f.rental_rate
""";

        output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.FourSpaces, Uppercase: true, UseLeadingComma: true));
        output.Should().Be(expectedResult);
    }

    /// <summary>
    /// Tests support for various operators
    /// </summary>
    internal static void SupportsOperators(Formatter formatter, params string[] operators)
    {
        foreach (string op in operators)
        {
            string input = $"foo{op}bar";
            string expectedResult = $"foo {op} bar";
            AssertFormat(formatter, input, expectedResult);
        }
    }

    /// <summary>
    /// Tests support for CASE [WHEN...] END syntax
    /// </summary>
    internal static void SupportsCase(Formatter formatter)
    {
        // formats CASE ... WHEN with a blank expression
        string input = "CASE WHEN option = 'foo' THEN 1 WHEN option = 'bar' THEN 2 WHEN option = 'baz' THEN 3 ELSE 4 END;";

        string expectedResult =
@"CASE
  WHEN option = 'foo' THEN 1
  WHEN option = 'bar' THEN 2
  WHEN option = 'baz' THEN 3
  ELSE 4
END;";
        AssertFormat(formatter, input, expectedResult);

        // formats CASE ... WHEN with an expression
        input = "CASE toString(getNumber()) WHEN 'one' THEN 1 WHEN 'two' THEN 2 WHEN 'three' THEN 3 ELSE 4 END;";

        expectedResult =
@"CASE
  toString(getNumber())
  WHEN 'one' THEN 1
  WHEN 'two' THEN 2
  WHEN 'three' THEN 3
  ELSE 4
END;";
        AssertFormat(formatter, input, expectedResult);

        // formats CASE ... WHEN inside SELECT
        input = "SELECT foo, bar, CASE baz WHEN 'one' THEN 1 WHEN 'two' THEN 2 ELSE 3 END FROM table";

        expectedResult =
@"SELECT
  foo,
  bar,
  CASE
    baz
    WHEN 'one' THEN 1
    WHEN 'two' THEN 2
    ELSE 3
  END
FROM
  table";
        AssertFormat(formatter, input, expectedResult);

        // recognizes lowercase CASE ... END
        input = "case when option = 'foo' then 1 else 2 end;";

        expectedResult =
@"case
  when option = 'foo' then 1
  else 2
end;";
        AssertFormat(formatter, input, expectedResult);

        // ignores words CASE and END inside other strings
        input = "SELECT CASEDATE, ENDDATE FROM table1;";

        expectedResult =
@"SELECT
  CASEDATE,
  ENDDATE
FROM
  table1;";
        AssertFormat(formatter, input, expectedResult);

        // properly converts to uppercase in case statements
        input = "case toString(getNumber()) when 'one' then 1 when 'two' then 2 when 'three' then 3 else 4 end;";

        expectedResult =
@"CASE
  toString(getNumber())
  WHEN 'one' THEN 1
  WHEN 'two' THEN 2
  WHEN 'three' THEN 3
  ELSE 4
END;";
        string output = formatter.Format(input, new SqlFormatterOptions(Indentation: Indentation.TwoSpaces, Uppercase: true));
        output.Should().Be(expectedResult);
    }

    /// <summary>
    /// Tests support for CREATE TABLE syntax
    /// </summary>
    internal static void SupportsCreateTable(Formatter formatter)
    {
        // formats short CREATE TABLE
        string input = "CREATE TABLE items (a INT PRIMARY KEY, b TEXT);";

        string expectedResult =
@"CREATE TABLE items (a INT PRIMARY KEY, b TEXT);";
        AssertFormat(formatter, input, expectedResult);

        // formats long CREATE TABLE
        input = "CREATE TABLE items (a INT PRIMARY KEY, b TEXT, c INT NOT NULL, doggie INT NOT NULL);";

        expectedResult =
@"CREATE TABLE items (
  a INT PRIMARY KEY,
  b TEXT,
  c INT NOT NULL,
  doggie INT NOT NULL
);";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests support for ALTER TABLE syntax
    /// </summary>
    internal static void SupportsAlterTable(Formatter formatter)
    {
        // formats ALTER TABLE ... ALTER COLUMN query
        string input = "ALTER TABLE supplier ALTER COLUMN supplier_name VARCHAR(100) NOT NULL;";

        string expectedResult =
@"ALTER TABLE
  supplier
ALTER COLUMN
  supplier_name VARCHAR(100) NOT NULL;";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests support for ALTER TABLE ... MODIFY syntax
    /// </summary>
    internal static void SupportsAlterTableModify(Formatter formatter)
    {
        // formats ALTER TABLE ... MODIFY statement
        string input = "ALTER TABLE supplier MODIFY supplier_name char(100) NOT NULL;";

        string expectedResult =
@"ALTER TABLE
  supplier
MODIFY
  supplier_name char(100) NOT NULL;";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests support for various string syntax
    /// </summary>
    internal static void SupportsStrings(Formatter formatter, params string[] stringTypes)
    {
        string input;
        string expectedResult;
        if (stringTypes.Contains("\"\""))
        {
            // supports double-quoted strings
            input = "\"foo JOIN bar\"";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "\"foo \\\" JOIN bar\"";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("''"))
        {
            // supports single-quoted strings
            input = "'foo JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "'foo \\' JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("``"))
        {
            // supports backtick-quoted strings
            input = "`foo JOIN bar`";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "`foo `` JOIN bar`";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("U&\"\""))
        {
            // supports unicode double-quoted strings
            input = "U&\"foo JOIN bar\"";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "U&\"foo \\\" JOIN bar\"";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("U&''"))
        {
            // supports single-quoted strings
            input = "U&'foo JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "U&'foo \\' JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("$$"))
        {
            // supports dollar-quoted strings
            input = "$xxx$foo $$ LEFT JOIN $yyy$ bar$xxx$";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "$$foo JOIN bar$$";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "$$foo $ JOIN bar$$";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "$$foo \r\n bar$$";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("[]"))
        {
            // supports [bracket-quoted identifiers]
            input = "[foo JOIN bar]";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "[foo ]] JOIN bar]";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        if (stringTypes.Contains("N''"))
        {
            // supports T-SQL unicode strings
            input = "N'foo JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);

            input = "N'foo \\' JOIN bar'";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }
    }

    /// <summary>
    /// Tests support for BETWEEN _ AND _ syntax
    /// </summary>
    internal static void SupportsBetween(Formatter formatter)
    {
        // formats BETWEEN _ AND _ on single line
        string input = "foo BETWEEN bar AND baz";
        string expectedResult = input;
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests support for SET SCHEMA syntax
    /// </summary>
    internal static void SupportsSchema(Formatter formatter)
    {
        // formats simple SET SCHEMA statements
        string input = "SET SCHEMA schema1;";
        string expectedResult =
@"SET SCHEMA
  schema1;";
        AssertFormat(formatter, input, expectedResult);
    }

    /// <summary>
    /// Tests support for various joins
    /// </summary>
    internal static void SupportsJoin(Formatter formatter, string[] without = null, string[] additionally = null)
    {
        Regex unsupportedJoinRegex
            = without != null
            ? new Regex(string.Join("|", without), RegexOptions.Compiled)
            : new Regex(@"^whateve_!%&$");

        bool isSupportedJoin(string join) => !unsupportedJoinRegex.Match(join).Success;

        string[] joins = new[] { "CROSS JOIN", "NATURAL JOIN" };
        foreach (string join in joins)
        {
            if (isSupportedJoin(join))
            {
                string input = $"SELECT * FROM tbl1 {join} tbl2";
                string expectedResult =
$@"SELECT
  *
FROM
  tbl1
  {join} tbl2";
                AssertFormat(formatter, input, expectedResult);
            }
        }

        // <join> ::= [ <join type> ] JOIN
        //
        // <join type> ::= INNER | <outer join type> [ OUTER ]
        //
        // <outer join type> ::= LEFT | RIGHT | FULL

        joins = new[]
        {
            "JOIN",
            "INNER JOIN",
            "LEFT JOIN",
            "LEFT OUTER JOIN",
            "RIGHT JOIN",
            "RIGHT OUTER JOIN",
            "FULL JOIN",
            "FULL OUTER JOIN"
        };

        if (additionally != null)
            joins = joins.Concat(additionally).ToArray();

        foreach (string join in joins)
        {
            if (isSupportedJoin(join))
            {
                string input =
$@"SELECT customer_id.from, COUNT(order_id) AS total FROM customers
{join} orders ON customers.customer_id = orders.customer_id;";
                string expectedResult =
$@"SELECT
  customer_id.from,
  COUNT(order_id) AS total
FROM
  customers
  {join} orders ON customers.customer_id = orders.customer_id;";
                AssertFormat(formatter, input, expectedResult);
            }
        }
    }

    internal static void AssertFormat(Formatter formatter, string input, string expectedOutput)
    {
        string output = formatter.Format(input);
        output.Replace("\r\n", Environment.NewLine).Should().Be(expectedOutput.Replace("\r\n", Environment.NewLine));
    }
}

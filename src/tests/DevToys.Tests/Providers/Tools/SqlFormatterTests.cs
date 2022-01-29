using System;
using System.Linq;
using System.Text.RegularExpressions;
using DevToys.Helpers.SqlFormatter;
using DevToys.Helpers.SqlFormatter.Core;
using DevToys.Helpers.SqlFormatter.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class SqlFormatterTests : MefBaseTest
    {
        [TestMethod]
        public void StandardSqlFormatterTest()
        {
            var formatter = new StandardSqlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCase(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsStrings(formatter, "\"\"", "''");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsJoin(formatter);

            // formats FETCH FIRST like LIMIT
            string input = "SELECT * FETCH FIRST 2 ROWS ONLY;";
            string expectedResult =
@"SELECT
  *
FETCH FIRST
  2 ROWS ONLY;";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void TSqlFormatterTest()
        {
            var formatter = new TSqlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCase(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsStrings(formatter, "\"\"", "''", "N''", "[]");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(formatter, "%", "&", "|", "^", "~", "!=", "!<", "!>", "+=", "-=", "*=", "/=", "%=", "|=", "&=", "^=", "::");
            SupportsJoin(formatter, without: new[] { "NATURAL" });

            // formats INSERT without INTO
            string input = "INSERT Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');";
            string expectedResult =
@"INSERT
  Customers (ID, MoneyBalance, Address, City)
VALUES
  (12, -123.4, 'Skagen 2111', 'Stv');";
            AssertFormat(formatter, input, expectedResult);

            // recognizes @variables
            input = "SELECT @variable, @\"var name\", @[var name];";
            expectedResult = "SELECT\r\n  @variable,\r\n  @\"var name\",\r\n  @[var name];";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with CROSS JOIN
            input = "SELECT a, b FROM t CROSS JOIN t2 on t.id = t2.id_t";
            expectedResult =
@"SELECT
  a,
  b
FROM
  t
  CROSS JOIN t2 on t.id = t2.id_t";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void SparkSqlFormatterTest()
        {
            var formatter = new SparkSqlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCase(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsStrings(formatter, "\"\"", "''", "``");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(formatter, "!=", "%", "|", "&", "^", "~", "!", "<=>", "%", "&&", "||", "==");
            SupportsJoin(
                formatter,
                additionally: new[]
                {
                    "ANTI JOIN",
                    "SEMI JOIN",
                    "LEFT ANTI JOIN",
                    "LEFT SEMI JOIN",
                    "RIGHT OUTER JOIN",
                    "RIGHT SEMI JOIN",
                    "NATURAL ANTI JOIN",
                    "NATURAL FULL OUTER JOIN",
                    "NATURAL INNER JOIN",
                    "NATURAL LEFT ANTI JOIN",
                    "NATURAL LEFT OUTER JOIN",
                    "NATURAL LEFT SEMI JOIN",
                    "NATURAL OUTER JOIN",
                    "NATURAL RIGHT OUTER JOIN",
                    "NATURAL RIGHT SEMI JOIN",
                    "NATURAL SEMI JOIN"
                });

            // formats WINDOW specification as top level
            string input = "SELECT *, LAG(value) OVER wnd AS next_value FROM tbl WINDOW wnd as (PARTITION BY id ORDER BY time);";
            string expectedResult =
@"SELECT
  *,
  LAG(value) OVER wnd AS next_value
FROM
  tbl
WINDOW
  wnd as (
    PARTITION BY
      id
    ORDER BY
      time
  );";
            AssertFormat(formatter, input, expectedResult);

            // formats window function and end as inline
            input = "SELECT window(time, \"1 hour\").start AS window_start, window(time, \"1 hour\").end AS window_end FROM tbl;";
            expectedResult = "SELECT\r\n  window(time, \"1 hour\").start AS window_start,\r\n  window(time, \"1 hour\").end AS window_end\r\nFROM\r\n  tbl;";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void RedshiftFormatterTest()
        {
            var formatter = new RedshiftFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsAlterTableModify(formatter);
            SupportsStrings(formatter, "\"\"", "''", "``");
            SupportsSchema(formatter);
            SupportsOperators(formatter, "%", "^", "|/", "||/", "<<", ">>", "&", "|", "~", "!", "!=", "||");
            SupportsJoin(formatter);

            // formats LIMIT
            string input = "SELECT col1 FROM tbl ORDER BY col2 DESC LIMIT 10;";
            string expectedResult =
@"SELECT
  col1
FROM
  tbl
ORDER BY
  col2 DESC
LIMIT
  10;";
            AssertFormat(formatter, input, expectedResult);

            // formats only -- as a line comment
            input =
@"SELECT col FROM
-- This is a comment
MyTable;";
            expectedResult =
@"SELECT
  col
FROM
  -- This is a comment
  MyTable;";
            AssertFormat(formatter, input, expectedResult);

            // recognizes @ as part of identifiers
            input = @"SELECT @col1 FROM tbl";
            expectedResult =
@"SELECT
  @col1
FROM
  tbl";
            AssertFormat(formatter, input, expectedResult);

            // formats DISTKEY and SORTKEY after CREATE TABLE
            input = @"CREATE TABLE items (a INT PRIMARY KEY, b TEXT, c INT NOT NULL, d INT NOT NULL) DISTKEY(created_at) SORTKEY(created_at);";
            expectedResult =
@"CREATE TABLE items (a INT PRIMARY KEY, b TEXT, c INT NOT NULL, d INT NOT NULL)
DISTKEY
(created_at)
SORTKEY
(created_at);";
            AssertFormat(formatter, input, expectedResult);

            // formats COPY
            input =
@"COPY schema.table
FROM 's3://bucket/file.csv'
IAM_ROLE 'arn:aws:iam::123456789:role/rolename'
FORMAT AS CSV DELIMITER ',' QUOTE ''
REGION AS 'us-east-1'";
            expectedResult =
@"COPY
  schema.table
FROM
  's3://bucket/file.csv'
IAM_ROLE
  'arn:aws:iam::123456789:role/rolename'
FORMAT
  AS CSV
DELIMITER
  ',' QUOTE ''
REGION
  AS 'us-east-1'";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void PostgreSqlFormatterTest()
        {
            var formatter = new PostgreSqlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCase(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsStrings(formatter, "\"\"", "''", "U&\"\"", "U&''", "$$");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(
                formatter,
                "%",
                "^",
                "!",
                "!!",
                "@",
                "!=",
                "&",
                "|",
                "~",
                "#",
                "<<",
                ">>",
                "||/",
                "|/",
                "::",
                "->>",
                "->",
                "~~*",
                "~~",
                "!~~*",
                "!~~",
                "~*",
                "!~*",
                "!~");
            SupportsJoin(formatter);

            // supports $n placeholders
            string input = "SELECT $1, $2 FROM tbl";
            string expectedResult =
@"SELECT
  $1,
  $2
FROM
  tbl";
            AssertFormat(formatter, input, expectedResult);

            // supports :name placeholders
            input = "foo = :bar";
            expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void PlSqlFormatterTest()
        {
            var formatter = new PlSqlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCase(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsAlterTableModify(formatter);
            SupportsStrings(formatter, "\"\"", "''", "``");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(formatter, "||", "**", "!=", ":=");
            SupportsJoin(formatter);

            // formats FETCH FIRST like LIMIT
            string input = "SELECT col1 FROM tbl ORDER BY col2 DESC FETCH FIRST 20 ROWS ONLY;";
            string expectedResult =
@"SELECT
  col1
FROM
  tbl
ORDER BY
  col2 DESC
FETCH FIRST
  20 ROWS ONLY;";
            AssertFormat(formatter, input, expectedResult);

            // formats only -- as a line comment
            input = "SELECT col FROM\r\n-- This is a comment\r\nMyTable;\r\n";
            expectedResult =
@"SELECT
  col
FROM
  -- This is a comment
  MyTable;";
            AssertFormat(formatter, input, expectedResult);

            // recognizes _, $, #, . and @ as part of identifiers
            input = "SELECT my_col$1#, col.2@ FROM tbl\r\n";
            expectedResult =
@"SELECT
  my_col$1#,
  col.2@
FROM
  tbl";
            AssertFormat(formatter, input, expectedResult);

            // formats INSERT without INTO
            input = "INSERT Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');";
            expectedResult =
@"INSERT
  Customers (ID, MoneyBalance, Address, City)
VALUES
  (12, -123.4, 'Skagen 2111', 'Stv');";
            AssertFormat(formatter, input, expectedResult);

            // recognizes ?[0-9]* placeholders
            input = "SELECT ?1, ?25, ?;";
            expectedResult =
@"SELECT
  ?1,
  ?25,
  ?;";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with CROSS APPLY
            input = "SELECT a, b FROM t CROSS APPLY fn(t.id)";
            expectedResult =
@"SELECT
  a,
  b
FROM
  t
  CROSS APPLY fn(t.id)";
            AssertFormat(formatter, input, expectedResult);

            // formats simple SELECT
            input = "SELECT N, M FROM t";
            expectedResult =
@"SELECT
  N,
  M
FROM
  t";
            AssertFormat(formatter, input, expectedResult);

            // formats simple SELECT with national characters
            input = "SELECT N'value'";
            expectedResult =
@"SELECT
  N'value'";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with OUTER APPLY
            input = "SELECT a, b FROM t OUTER APPLY fn(t.id)";
            expectedResult =
@"SELECT
  a,
  b
FROM
  t
  OUTER APPLY fn(t.id)";
            AssertFormat(formatter, input, expectedResult);

            // formats Oracle recursive sub queries
            input =
@"WITH t1(id, parent_id) AS (
  -- Anchor member.
  SELECT
    id,
    parent_id
  FROM
    tab1
  WHERE
    parent_id IS NULL
  MINUS
    -- Recursive member.
  SELECT
    t2.id,
    t2.parent_id
  FROM
    tab1 t2,
    t1
  WHERE
    t2.parent_id = t1.id
) SEARCH BREADTH FIRST BY id SET order1,
another AS (SELECT * FROM dual)
SELECT id, parent_id FROM t1 ORDER BY order1;";
            expectedResult =
@"WITH t1(id, parent_id) AS (
  -- Anchor member.
  SELECT
    id,
    parent_id
  FROM
    tab1
  WHERE
    parent_id IS NULL
  MINUS
  -- Recursive member.
  SELECT
    t2.id,
    t2.parent_id
  FROM
    tab1 t2,
    t1
  WHERE
    t2.parent_id = t1.id
) SEARCH BREADTH FIRST BY id SET order1,
another AS (
  SELECT
    *
  FROM
    dual
)
SELECT
  id,
  parent_id
FROM
  t1
ORDER BY
  order1;";
            AssertFormat(formatter, input, expectedResult);

            // formats Oracle recursive sub queries regardless of capitalization
            input =
@"WITH t1(id, parent_id) AS (
  -- Anchor member.
  SELECT
    id,
    parent_id
  FROM
    tab1
  WHERE
    parent_id IS NULL
  MINUS
    -- Recursive member.
  SELECT
    t2.id,
    t2.parent_id
  FROM
    tab1 t2,
    t1
  WHERE
    t2.parent_id = t1.id
) SEARCH BREADTH FIRST by id set order1,
another AS (SELECT * FROM dual)
SELECT id, parent_id FROM t1 ORDER BY order1;";
            expectedResult =
@"WITH t1(id, parent_id) AS (
  -- Anchor member.
  SELECT
    id,
    parent_id
  FROM
    tab1
  WHERE
    parent_id IS NULL
  MINUS
  -- Recursive member.
  SELECT
    t2.id,
    t2.parent_id
  FROM
    tab1 t2,
    t1
  WHERE
    t2.parent_id = t1.id
) SEARCH BREADTH FIRST by id set order1,
another AS (
  SELECT
    *
  FROM
    dual
)
SELECT
  id,
  parent_id
FROM
  t1
ORDER BY
  order1;";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void N1qlFormatterTest()
        {
            var formatter = new N1qlFormatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsStrings(formatter, "\"\"", "''", "``");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(formatter, "%", "==", "!=");
            SupportsJoin(formatter, without: new[] { "FULL", "CROSS", "NATURAL" });

            // formats SELECT query with element selection expression
            string input = "SELECT order_lines[0].productId FROM orders;";
            string expectedResult =
@"SELECT
  order_lines[0].productId
FROM
  orders;";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with primary key querying
            input = "SELECT fname, email FROM tutorial USE KEYS ['dave', 'ian'];";
            expectedResult =
@"SELECT
  fname,
  email
FROM
  tutorial
USE KEYS
  ['dave', 'ian'];";
            AssertFormat(formatter, input, expectedResult);

            // formats INSERT with {} object literal
            input = "INSERT INTO heroes (KEY, VALUE) VALUES ('123', {'id':1,'type':'Tarzan'});";
            expectedResult =
@"INSERT INTO
  heroes (KEY, VALUE)
VALUES
  ('123', {'id': 1, 'type': 'Tarzan'});";
            AssertFormat(formatter, input, expectedResult);

            // formats INSERT with large object and array literals
            input = "INSERT INTO heroes (KEY, VALUE) VALUES ('123', {'id': 1, 'type': 'Tarzan', 'array': [123456789, 123456789, 123456789, 123456789, 123456789], 'hello': 'world'});";
            expectedResult =
@"INSERT INTO
  heroes (KEY, VALUE)
VALUES
  (
    '123',
    {
      'id': 1,
      'type': 'Tarzan',
      'array': [
        123456789,
        123456789,
        123456789,
        123456789,
        123456789
      ],
      'hello': 'world'
    }
  );";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with UNNEST top level reserver word
            input = "SELECT * FROM tutorial UNNEST tutorial.children c;";
            expectedResult =
@"SELECT
  *
FROM
  tutorial
UNNEST
  tutorial.children c;";
            AssertFormat(formatter, input, expectedResult);

            // formats SELECT query with NEST and USE KEYS
            input =
@"SELECT * FROM usr
USE KEYS 'Elinor_33313792' NEST orders_with_users orders
ON KEYS ARRAY s.order_id FOR s IN usr.shipped_order_history END;";
            expectedResult =
@"SELECT
  *
FROM
  usr
USE KEYS
  'Elinor_33313792'
NEST
  orders_with_users orders ON KEYS ARRAY s.order_id FOR s IN usr.shipped_order_history END;";
            AssertFormat(formatter, input, expectedResult);

            // formats explained DELETE query with USE KEYS and RETURNING
            input = "EXPLAIN DELETE FROM tutorial t USE KEYS 'baldwin' RETURNING t";
            expectedResult =
@"EXPLAIN DELETE FROM
  tutorial t
USE KEYS
  'baldwin' RETURNING t";
            AssertFormat(formatter, input, expectedResult);

            // formats UPDATE query with USE KEYS and RETURNING
            input = "UPDATE tutorial USE KEYS 'baldwin' SET type = 'actor' RETURNING tutorial.type";
            expectedResult =
@"UPDATE
  tutorial
USE KEYS
  'baldwin'
SET
  type = 'actor' RETURNING tutorial.type";
            AssertFormat(formatter, input, expectedResult);

            // recognizes $variables
            input = "SELECT $variable, $\'var name\', $\"var name\", $`var name`;";
            expectedResult = "SELECT\r\n  $variable,\r\n  $'var name',\r\n  $\"var name\",\r\n  $`var name`;";
            AssertFormat(formatter, input, expectedResult);
        }

        [TestMethod]
        public void MySqlFormatterTest()
        {
            var formatter = new MySqlFormatter();
            BehavesLikeMariaDbFormatter(formatter);
            SupportsOperators(formatter, "->", "->>");
        }

        [TestMethod]
        public void MariaDbFormatterTest()
        {
            var formatter = new MariaDbFormatter();
            BehavesLikeMariaDbFormatter(formatter);
        }

        [TestMethod]
        public void Db2FormatterTest()
        {
            var formatter = new Db2Formatter();
            BehavesLikeSqlFormatter(formatter);
            SupportsCreateTable(formatter);
            SupportsAlterTable(formatter);
            SupportsStrings(formatter, "\"\"", "''", "``");
            SupportsBetween(formatter);
            SupportsSchema(formatter);
            SupportsOperators(formatter, "%", "**", "!=", "!>", "!>", "||");

            // formats FETCH FIRST like LIMIT
            string input = "SELECT col1 FROM tbl ORDER BY col2 DESC FETCH FIRST 20 ROWS ONLY;";
            string expectedResult =
@"SELECT
  col1
FROM
  tbl
ORDER BY
  col2 DESC
FETCH FIRST
  20 ROWS ONLY;";
            AssertFormat(formatter, input, expectedResult);

            // formats only -- as a line comment
            input =
@"SELECT col FROM
-- This is a comment
MyTable;";
            expectedResult =
@"SELECT
  col
FROM
  -- This is a comment
  MyTable;";
            AssertFormat(formatter, input, expectedResult);

            // recognizes @ and # as part of identifiers
            input = "SELECT col#1, @col2 FROM tbl";
            expectedResult =
@"SELECT
  col#1,
  @col2
FROM
  tbl";
            AssertFormat(formatter, input, expectedResult);

            // recognizes :variables
            input = "SELECT :variable;";
            expectedResult =
@"SELECT
  :variable;";
            AssertFormat(formatter, input, expectedResult);
        }

        /// <summary>
        /// Core tests for all SQL formatters
        /// </summary>
        private void BehavesLikeSqlFormatter(Formatter formatter)
        {
            SupportsComments(formatter);
            SupportsConfigOptions(formatter);
            SupportsOperators(formatter, "=", "+", "-", "*", "/", "<>", ">", "<", ">=", "<=");

            // does nothing with empty input
            string input = string.Empty;
            string expectedResult = string.Empty;
            string output = formatter.Format(input);
            Assert.AreEqual(expectedResult, output);

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
        }

        /// <summary>
        /// Shared tests for MySQL and MariaDB
        /// </summary>
        private void BehavesLikeMariaDbFormatter(Formatter formatter)
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
        private void SupportsComments(Formatter formatter)
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
        private void SupportsConfigOptions(Formatter formatter)
        {
            // supports indent option
            string input = @"SELECT count(*),Column1 FROM Table1;";

            string expectedResult =
@"SELECT
    count(*),
    Column1
FROM
    Table1;";

            string output = formatter.Format(input, new SqlFormatterOptions(indentationSize: 4, uppercase: false));
            Assert.AreEqual(expectedResult, output);

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

            output = formatter.Format(input, new SqlFormatterOptions(indentationSize: 2, uppercase: false, linesBetweenQueries: 2));
            Assert.AreEqual(expectedResult, output);

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

            output = formatter.Format(input, new SqlFormatterOptions(indentationSize: 2, uppercase: true));
            Assert.AreEqual(expectedResult, output);
        }

        /// <summary>
        /// Tests support for various operators
        /// </summary>
        private void SupportsOperators(Formatter formatter, params string[] operators)
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
        private void SupportsCase(Formatter formatter)
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
            string output = formatter.Format(input, new SqlFormatterOptions(indentationSize: 2, uppercase: true));
            Assert.AreEqual(expectedResult, output);
        }

        /// <summary>
        /// Tests support for CREATE TABLE syntax
        /// </summary>
        private void SupportsCreateTable(Formatter formatter)
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
        private void SupportsAlterTable(Formatter formatter)
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
        private void SupportsAlterTableModify(Formatter formatter)
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
        private void SupportsStrings(Formatter formatter, params string[] stringTypes)
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
        private void SupportsBetween(Formatter formatter)
        {
            // formats BETWEEN _ AND _ on single line
            string input = "foo BETWEEN bar AND baz";
            string expectedResult = input;
            AssertFormat(formatter, input, expectedResult);
        }

        /// <summary>
        /// Tests support for SET SCHEMA syntax
        /// </summary>
        private void SupportsSchema(Formatter formatter)
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
        private void SupportsJoin(Formatter formatter, string[] without = null, string[] additionally = null)
        {
            Regex unsupportedJoinRegex
                = without != null
                ? new Regex(string.Join("|", without), RegexOptions.Compiled)
                : new Regex(@"^whateve_!%&$");

            Func<string, bool> isSupportedJoin
                = (string join) => !unsupportedJoinRegex.Match(join).Success;

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
            {
                joins = joins.Concat(additionally).ToArray();
            }

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

        private void AssertFormat(Formatter formatter, string input, string expectedOutput)
        {
            string output = formatter.Format(input);
            Assert.AreEqual(expectedOutput, output);
        }
    }
}

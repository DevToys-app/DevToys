using DevToys.Tools.Helpers.SqlFormatter.Languages;

namespace DevToys.UnitTests.Tools.Formatters.Sql;

[Collection(nameof(TestParallelizationDisabled))]
public class SqlFormatterTests : MefBasedTest
{
    [Fact]
    public void StandardSqlFormatterTest()
    {
        var formatter = new StandardSqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCase(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsJoin(formatter);

        // formats FETCH FIRST like LIMIT
        string input = "SELECT * FETCH FIRST 2 ROWS ONLY;";
        string expectedResult =
@"SELECT
  *
FETCH FIRST
  2 ROWS ONLY;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void TSqlFormatterTest()
    {
        var formatter = new TSqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCase(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "N''", "[]");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "%", "&", "|", "^", "~", "!=", "!<", "!>", "+=", "-=", "*=", "/=", "%=", "|=", "&=", "^=", "::");
        SqlFormatterTestHelpers.SupportsJoin(formatter, without: new[] { "NATURAL" });

        // formats INSERT without INTO
        string input = "INSERT Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');";
        string expectedResult =
@"INSERT
  Customers (ID, MoneyBalance, Address, City)
VALUES
  (12, -123.4, 'Skagen 2111', 'Stv');";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes @variables
        input = "SELECT @variable;";
        expectedResult = "SELECT\r\n  @variable;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats SELECT query with CROSS JOIN
        input = "SELECT a, b FROM t CROSS JOIN t2 on t.id = t2.id_t";
        expectedResult =
@"SELECT
  a,
  b
FROM
  t
  CROSS JOIN t2 on t.id = t2.id_t";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void SparkSqlFormatterTest()
    {
        var formatter = new SparkSqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCase(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "``");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "!=", "%", "|", "&", "^", "~", "!", "<=>", "%", "&&", "||", "==");
        SqlFormatterTestHelpers.SupportsJoin(
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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats window function and end as inline
        input = "SELECT window(time, \"1 hour\").start AS window_start, window(time, \"1 hour\").end AS window_end FROM tbl;";
        expectedResult = "SELECT\r\n  window(time, \"1 hour\").start AS window_start,\r\n  window(time, \"1 hour\").end AS window_end\r\nFROM\r\n  tbl;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void RedshiftFormatterTest()
    {
        var formatter = new RedshiftFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTableModify(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "``");
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "%", "^", "|/", "||/", "<<", ">>", "&", "|", "~", "!", "!=", "||");
        SqlFormatterTestHelpers.SupportsJoin(formatter);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes @ as part of identifiers
        input = @"SELECT @col1 FROM tbl";
        expectedResult =
@"SELECT
  @col1
FROM
  tbl";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats DISTKEY and SORTKEY after CREATE TABLE
        input = @"CREATE TABLE items (a INT PRIMARY KEY, b TEXT, c INT NOT NULL, d INT NOT NULL) DISTKEY(created_at) SORTKEY(created_at);";
        expectedResult =
@"CREATE TABLE items (a INT PRIMARY KEY, b TEXT, c INT NOT NULL, d INT NOT NULL)
DISTKEY
(created_at)
SORTKEY
(created_at);";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void PostgreSqlFormatterTest()
    {
        var formatter = new PostgreSqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCase(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "U&\"\"", "U&''", "$$");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(
            formatter,
            "%",
            "^",
            "!",
            "!!",
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
        SqlFormatterTestHelpers.SupportsJoin(formatter);

        // supports $n placeholders
        string input = "SELECT $1, $2 FROM tbl";
        string expectedResult =
@"SELECT
  $1,
  $2
FROM
  tbl";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // supports :name placeholders
        input = "foo = :bar";
        expectedResult = input;
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void PlSqlFormatterTest()
    {
        var formatter = new PlSqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCase(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTableModify(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "``");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "||", "**", "!=", ":=");
        SqlFormatterTestHelpers.SupportsJoin(formatter);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats only -- as a line comment
        input = "SELECT col FROM\r\n-- This is a comment\r\nMyTable;\r\n";
        expectedResult =
@"SELECT
  col
FROM
  -- This is a comment
  MyTable;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes _, $, #, . and @ as part of identifiers
        input = "SELECT my_col$1#, col.2@ FROM tbl\r\n";
        expectedResult =
@"SELECT
  my_col$1#,
  col.2@
FROM
  tbl";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats INSERT without INTO
        input = "INSERT Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');";
        expectedResult =
@"INSERT
  Customers (ID, MoneyBalance, Address, City)
VALUES
  (12, -123.4, 'Skagen 2111', 'Stv');";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes ?[0-9]* placeholders
        input = "SELECT ?1, ?25, ?;";
        expectedResult =
@"SELECT
  ?1,
  ?25,
  ?;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats SELECT query with CROSS APPLY
        input = "SELECT a, b FROM t CROSS APPLY fn(t.id)";
        expectedResult =
@"SELECT
  a,
  b
FROM
  t
  CROSS APPLY fn(t.id)";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats simple SELECT
        input = "SELECT N, M FROM t";
        expectedResult =
@"SELECT
  N,
  M
FROM
  t";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats simple SELECT with national characters
        input = "SELECT N'value'";
        expectedResult =
@"SELECT
  N'value'";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats SELECT query with OUTER APPLY
        input = "SELECT a, b FROM t OUTER APPLY fn(t.id)";
        expectedResult =
@"SELECT
  a,
  b
FROM
  t
  OUTER APPLY fn(t.id)";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void N1qlFormatterTest()
    {
        var formatter = new N1qlFormatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "``");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "%", "==", "!=");
        SqlFormatterTestHelpers.SupportsJoin(formatter, without: new[] { "FULL", "CROSS", "NATURAL" });

        // formats SELECT query with element selection expression
        string input = "SELECT order_lines[0].productId FROM orders;";
        string expectedResult =
@"SELECT
  order_lines[0].productId
FROM
  orders;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats INSERT with {} object literal
        input = "INSERT INTO heroes (KEY, VALUE) VALUES ('123', {'id':1,'type':'Tarzan'});";
        expectedResult =
@"INSERT INTO
  heroes (KEY, VALUE)
VALUES
  ('123', {'id': 1, 'type': 'Tarzan'});";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats SELECT query with UNNEST top level reserver word
        input = "SELECT * FROM tutorial UNNEST tutorial.children c;";
        expectedResult =
@"SELECT
  *
FROM
  tutorial
UNNEST
  tutorial.children c;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats explained DELETE query with USE KEYS and RETURNING
        input = "EXPLAIN DELETE FROM tutorial t USE KEYS 'baldwin' RETURNING t";
        expectedResult =
@"EXPLAIN DELETE FROM
  tutorial t
USE KEYS
  'baldwin' RETURNING t";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // formats UPDATE query with USE KEYS and RETURNING
        input = "UPDATE tutorial USE KEYS 'baldwin' SET type = 'actor' RETURNING tutorial.type";
        expectedResult =
@"UPDATE
  tutorial
USE KEYS
  'baldwin'
SET
  type = 'actor' RETURNING tutorial.type";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes $variables
        input = "SELECT $variable;";
        expectedResult = "SELECT\r\n  $variable;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }

    [Fact]
    public void MySqlFormatterTest()
    {
        var formatter = new MySqlFormatter();
        SqlFormatterTestHelpers.BehavesLikeMariaDbFormatter(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "->", "->>");
    }

    [Fact]
    public void MariaDbFormatterTest()
    {
        var formatter = new MariaDbFormatter();
        SqlFormatterTestHelpers.BehavesLikeMariaDbFormatter(formatter);
    }

    [Fact]
    public void Db2FormatterTest()
    {
        var formatter = new Db2Formatter();
        SqlFormatterTestHelpers.BehavesLikeSqlFormatter(formatter);
        SqlFormatterTestHelpers.SupportsCreateTable(formatter);
        SqlFormatterTestHelpers.SupportsAlterTable(formatter);
        SqlFormatterTestHelpers.SupportsStrings(formatter, "\"\"", "''", "``");
        SqlFormatterTestHelpers.SupportsBetween(formatter);
        SqlFormatterTestHelpers.SupportsSchema(formatter);
        SqlFormatterTestHelpers.SupportsOperators(formatter, "%", "**", "!=", "!>", "!>", "||");

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

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
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes @ and # as part of identifiers
        input = "SELECT col#1, @col2 FROM tbl";
        expectedResult =
@"SELECT
  col#1,
  @col2
FROM
  tbl";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);

        // recognizes :variables
        input = "SELECT :variable;";
        expectedResult =
@"SELECT
  :variable;";
        SqlFormatterTestHelpers.AssertFormat(formatter, input, expectedResult);
    }
}

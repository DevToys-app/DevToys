internal class CSharpVersionUpdateRule
{
    private readonly VersionUpdateRule _updateRule;
    public CSharpVersionUpdateRule(string attributeName, string updateRule)
    {
        AttributeName = attributeName;
        _updateRule = new VersionUpdateRule(updateRule);
    }
    public string AttributeName { get; private set; }
    public string Update(VersionString v) { return _updateRule.Update(v); }
}

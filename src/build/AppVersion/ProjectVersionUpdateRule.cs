internal class ProjectVersionUpdateRule
{
    private readonly VersionUpdateRule _updateRule;
    public ProjectVersionUpdateRule(string parameterName, string updateRule)
    {
        ParameterName = parameterName;
        _updateRule = new VersionUpdateRule(updateRule);
    }
    public string ParameterName { get; private set; }
    public string Update(VersionString v) { return _updateRule.Update(v); }
}

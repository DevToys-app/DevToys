using DevToys.Tools.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonTableHelperTests
{
    [Theory]
    [InlineData("{\"a\":{\"b\":{\"c\":1}}}", "{\"a_b_c\":1}")]
    [InlineData("{\"a\":{\"b\":{\"c\":[]}}}", "{}")]
    [InlineData("{\"a\":{\"b\":1,\"c\":{\"d\":2},\"e\":[3,4]},\"f\":[5,6]}", "{\"a_b\":1,\"a_c_d\":2}")]
    public void Flatten(string inputJson, string expectedJson)
    {
        var obj = JsonConvert.DeserializeObject(inputJson) as JObject;
        JObject flattened = JsonTableHelper.FlattenJsonObject(obj);
        string serialized = JsonConvert.SerializeObject(flattened);
        serialized.Should().Be(expectedJson);
    }
}

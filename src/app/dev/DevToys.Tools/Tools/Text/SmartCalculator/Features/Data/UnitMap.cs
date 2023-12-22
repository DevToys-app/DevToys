using System.Runtime.Serialization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;
using Newtonsoft.Json;
using UnitsNet.Units;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

[DataContract]
internal sealed class UnitMap
{
    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<LengthUnit>))]
    [DataMember(Name = "Length")]
    public Dictionary<string, LengthUnit> Length { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<InformationUnit>))]
    [DataMember(Name = "Information")]
    public Dictionary<string, InformationUnit> Information { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<AreaUnit>))]
    [DataMember(Name = "Area")]
    public Dictionary<string, AreaUnit> Area { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<SpeedUnit>))]
    [DataMember(Name = "Speed")]
    public Dictionary<string, SpeedUnit> Speed { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<VolumeUnit>))]
    [DataMember(Name = "Volume")]
    public Dictionary<string, VolumeUnit> Volume { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<MassUnit>))]
    [DataMember(Name = "Mass")]
    public Dictionary<string, MassUnit> Mass { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<AngleUnit>))]
    [DataMember(Name = "Angle")]
    public Dictionary<string, AngleUnit> Angle { get; set; } = null!;

    [JsonConverter(typeof(DictionaryWithSpecialEnumValueConverter<TemperatureUnit>))]
    [DataMember(Name = "Temperature")]
    public Dictionary<string, TemperatureUnit> Temperature { get; set; } = null!;

    public static UnitMap Load(string json)
    {
        UnitMap? result = JsonConvert.DeserializeObject<UnitMap>(json);
        Guard.IsNotNull(result);
        return result;
    }
}

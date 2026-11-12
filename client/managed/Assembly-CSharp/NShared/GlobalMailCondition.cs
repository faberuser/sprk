namespace NShared;

[JsonSerializable(Javascript = true)]
public class GlobalMailCondition
{
	public string CountryCode { get; set; }

	public string Language { get; set; }

	public string DevicePlatform { get; set; }

	public string CreatedTimeBegin { get; set; }

	public string CreatedTimeEnd { get; set; }

	public int GroupIndex { get; set; }
}

namespace NShared;

[JsonSerializable(Javascript = true)]
public class DeviceBuildInfo
{
	public string Fingerprint { get; set; }

	public string Model { get; set; }

	public string Brand { get; set; }

	public string Device { get; set; }

	public string Product { get; set; }

	public string Type { get; set; }

	public string Serial { get; set; }

	public string Board { get; set; }

	public DeviceBuildInfo()
	{
		Fingerprint = string.Empty;
		Model = string.Empty;
		Brand = string.Empty;
		Device = string.Empty;
		Product = string.Empty;
		Type = string.Empty;
		Serial = string.Empty;
		Board = string.Empty;
	}
}

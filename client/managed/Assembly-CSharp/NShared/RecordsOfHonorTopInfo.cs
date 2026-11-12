namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecordsOfHonorTopInfo
{
	public RecordsOfHonorContentType ContentType { get; set; }

	public string CountryCode { get; set; }

	public string Name { get; set; }

	public int AvatarHeroIndex { get; set; }
}

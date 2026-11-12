namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecordsOfHonorSeasonInfo
{
	public RecordsOfHonorContentType ContentType { get; set; }

	public int[] Season { get; set; }
}

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetGachaItemResultInfo
{
	public ItemResultInfo ItemResult { get; set; }

	public PetInfo PetResult { get; set; }
}

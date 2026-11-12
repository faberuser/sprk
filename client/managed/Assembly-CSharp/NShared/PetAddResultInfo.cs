namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetAddResultInfo
{
	public ItemResultInfo[] PetSoulResults { get; set; }

	public PetInfo PetResult { get; set; }
}

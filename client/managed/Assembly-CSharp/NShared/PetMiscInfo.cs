namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetMiscInfo : IAccountAttrKey<PetMiscType>
{
	PetMiscType IAccountAttrKey<PetMiscType>.Key => MiscKey;

	public PetMiscType MiscKey { get; set; }

	public string MiscValue { get; set; }

	public string UpdatedTime { get; set; }
}

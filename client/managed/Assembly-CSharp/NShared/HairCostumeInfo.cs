namespace NShared;

[JsonSerializable(Javascript = true)]
public class HairCostumeInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => HairCostumeIndex;

	public int HairCostumeIndex { get; set; }

	public string CreatedTime { get; set; }
}

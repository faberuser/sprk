namespace NShared;

[JsonSerializable(Javascript = true)]
public class CostumeInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => CostumeIndex;

	public int CostumeIndex { get; set; }

	public int[] ColorCostumeIndices { get; set; }

	public string CreatedTime { get; set; }
}

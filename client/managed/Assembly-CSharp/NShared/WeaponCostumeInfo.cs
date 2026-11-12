namespace NShared;

[JsonSerializable(Javascript = true)]
public class WeaponCostumeInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => WeaponCostumeIndex;

	public int WeaponCostumeIndex { get; set; }

	public string CreatedTime { get; set; }
}

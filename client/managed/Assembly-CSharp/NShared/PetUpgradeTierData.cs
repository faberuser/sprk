using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetUpgradeTierData : IDataKeySelector<int>
{
	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public string ResultPetGroupCode { get; set; }

	[Key(2)]
	public int ItemCount { get; set; }

	[Key(3)]
	public int ReqGold { get; set; }

	int IDataKeySelector<int>.Key => Grade;
}

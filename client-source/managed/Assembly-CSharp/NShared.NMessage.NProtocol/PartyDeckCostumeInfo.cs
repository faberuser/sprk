namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyDeckCostumeInfo
{
	public int HeroIndex { get; set; }

	public CachedHeroCostumeInfo CostumeInfo { get; set; }
}

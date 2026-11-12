namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyDeckHeroInfo
{
	public int HeroIndex { get; set; }

	public CachedHeroInfo HeroInfo { get; set; }

	public bool IsAdd
	{
		get
		{
			if (HeroInfo != null)
			{
				return HeroInfo.HeroIndex > 0;
			}
			return false;
		}
	}

	public bool IsRemove => !IsAdd;

	public int DeckIndex { get; set; }
}

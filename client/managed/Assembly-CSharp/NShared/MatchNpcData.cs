namespace NShared;

[JsonSerializable]
public class MatchNpcData : IDataKeySelector<int>
{
	public int Index { get; set; }

	public string Name { get; set; }

	public int HeroIndex1 { get; set; }

	public string HeroInfo1 { get; set; }

	public int HeroIndex2 { get; set; }

	public string HeroInfo2 { get; set; }

	public int HeroIndex3 { get; set; }

	public string HeroInfo3 { get; set; }

	public int HeroIndex4 { get; set; }

	public string HeroInfo4 { get; set; }

	public int TierIndex { get; set; }

	public int MinMatchScore { get; set; }

	public int MaxMatchScore { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int[] GetHeroIndices()
	{
		return new int[4] { HeroIndex1, HeroIndex2, HeroIndex3, HeroIndex4 };
	}

	public void SetHeroInfo(int slotIndex, int heroIndex, string heroInfo)
	{
		switch (slotIndex)
		{
		case 1:
			HeroIndex1 = heroIndex;
			HeroInfo1 = heroInfo;
			break;
		case 2:
			HeroIndex2 = heroIndex;
			HeroInfo2 = heroInfo;
			break;
		case 3:
			HeroIndex3 = heroIndex;
			HeroInfo3 = heroInfo;
			break;
		case 4:
			HeroIndex4 = heroIndex;
			HeroInfo4 = heroInfo;
			break;
		}
	}

	public override string ToString()
	{
		return "Index=" + Index + " Name=" + Name + " (" + HeroIndex1 + "," + HeroIndex2 + "," + HeroIndex3 + "," + HeroIndex4 + ")";
	}
}

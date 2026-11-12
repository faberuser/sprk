using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GetNewHeroSceneBundleData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int[] BundleHeroIndices { get; set; }

	[Key(2)]
	public string PackCode { get; set; }

	[Key(3)]
	public string[] TreeName { get; set; }

	[Key(4)]
	public int[] SequenceHeroIndices { get; set; }

	[Key(5)]
	public GetNewHeroInfoPosition[] HeroInfoPosition { get; set; }

	[Key(6)]
	public string Name { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int[] GetSequenceIndices()
	{
		if (SequenceHeroIndices != null && SequenceHeroIndices.Length == BundleHeroIndices.Length)
		{
			return SequenceHeroIndices;
		}
		return BundleHeroIndices;
	}

	public bool GetShowInfoOnHeroLeft(int index)
	{
		if (HeroInfoPosition == null)
		{
			return false;
		}
		if (index >= HeroInfoPosition.Length)
		{
			return false;
		}
		return HeroInfoPosition[index] == GetNewHeroInfoPosition.Left;
	}

	public bool IsIncludeHeroIndex(int heroIndex)
	{
		if (BundleHeroIndices == null)
		{
			return false;
		}
		for (int i = 0; i < BundleHeroIndices.Length; i++)
		{
			if (BundleHeroIndices[i] == heroIndex)
			{
				return true;
			}
		}
		return false;
	}
}

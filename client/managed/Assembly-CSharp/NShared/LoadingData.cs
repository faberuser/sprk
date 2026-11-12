using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LoadingData : IDataKeySelector<int>
{
	public enum ECategory
	{
		HeroContent,
		HeroIntro,
		GamePlayTip
	}

	public enum EGroupType
	{
		None,
		Default,
		StartGame,
		StartBattle,
		EndBattle,
		Battle
	}

	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public ECategory Category { get; set; }

	[Key(2)]
	public string[] Argument { get; set; }

	[Key(3)]
	public string HeroCode { get; set; }

	[Key(4)]
	public float[] HeroPosOffset { get; set; }

	[Key(5)]
	public int HeroWidth { get; set; }

	[Key(6)]
	public int HeroHeight { get; set; }

	[Key(7)]
	public EGroupType GroupType { get; set; }

	[Key(8)]
	public string[] GroupArg { get; set; }

	[Key(9)]
	public int ShowRate { get; set; }

	public override string ToString()
	{
		return "[" + Index + "] = " + Category;
	}

	public string GetArg(int index)
	{
		if (Argument != null && index < Argument.Length)
		{
			return Argument[index];
		}
		return string.Empty;
	}

	public int GetArgInt(int index)
	{
		string arg = GetArg(index);
		int result = 0;
		int.TryParse(arg, out result);
		return result;
	}

	public string GetGroupArg(int index)
	{
		if (GroupArg != null && index < GroupArg.Length)
		{
			return GroupArg[index];
		}
		return string.Empty;
	}

	public int GetGroupArgInt(int index)
	{
		string groupArg = GetGroupArg(index);
		int result = 0;
		int.TryParse(groupArg, out result);
		return result;
	}
}

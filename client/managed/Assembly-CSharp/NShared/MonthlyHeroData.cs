using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MonthlyHeroData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string OpenDate { get; set; }

	[Key(2)]
	public CalendarCutSceneType Type { get; set; }

	[Key(3)]
	public int HeroIndex { get; set; }

	[Key(4)]
	public string CutScenePath { get; set; }

	[Key(5)]
	public string IllustPath { get; set; }

	[Key(6)]
	public int RewardIndex { get; set; }

	[Key(7)]
	public string Title { get; set; }

	[Key(8)]
	public string Desc { get; set; }

	public DateTime GetOpenDateTime()
	{
		return Convert.ToDateTime(OpenDate);
	}
}

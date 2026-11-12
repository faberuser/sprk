using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroLimitBreakData : IDataKeySelector<HeroLimitBreakDataKey>
{
	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int LimitBreakLevel { get; set; }

	[Key(2)]
	public int ReqLocalExp { get; set; }

	[Key(3)]
	public string MaterialItemCode1 { get; set; }

	[Key(4)]
	public int MaterialItemCount1 { get; set; }

	[Key(5)]
	public string MaterialItemCode2 { get; set; }

	[Key(6)]
	public int MaterialItemCount2 { get; set; }

	[Key(7)]
	public string MaterialItemCode3 { get; set; }

	[Key(8)]
	public int MaterialItemCount3 { get; set; }

	[Key(9)]
	public int GetTranscendSkillPoint { get; set; }

	HeroLimitBreakDataKey IDataKeySelector<HeroLimitBreakDataKey>.Key => new HeroLimitBreakDataKey(HeroIndex, LimitBreakLevel);

	public List<Tuple<string, int>> GetMaterialItems()
	{
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		if (!String_Extension_Method.IsNullOrEmpty(MaterialItemCode1))
		{
			list.Add(new Tuple<string, int>(MaterialItemCode1, MaterialItemCount1));
		}
		if (!String_Extension_Method.IsNullOrEmpty(MaterialItemCode2))
		{
			list.Add(new Tuple<string, int>(MaterialItemCode2, MaterialItemCount2));
		}
		if (!String_Extension_Method.IsNullOrEmpty(MaterialItemCode3))
		{
			list.Add(new Tuple<string, int>(MaterialItemCode3, MaterialItemCount3));
		}
		return list;
	}
}

using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SelectRewardData : IDataKeySelector<ChapterDungeonKey2>
{
	public class SelectRewardItemInfo
	{
		public string Code;

		public int Count;

		public int Star;

		public SelectRewardItemInfo(string code, int count, int star)
		{
			Code = code;
			Count = count;
			Star = star;
		}
	}

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int SelectCount { get; set; }

	[Key(3)]
	public string Item1Code { get; set; }

	[Key(4)]
	public int Item1Count { get; set; }

	[Key(5)]
	public int Item1Star { get; set; }

	[Key(6)]
	public string Item2Code { get; set; }

	[Key(7)]
	public int Item2Count { get; set; }

	[Key(8)]
	public int Item2Star { get; set; }

	[Key(9)]
	public string Item3Code { get; set; }

	[Key(10)]
	public int Item3Count { get; set; }

	[Key(11)]
	public int Item3Star { get; set; }

	[Key(12)]
	public string Item4Code { get; set; }

	[Key(13)]
	public int Item4Count { get; set; }

	[Key(14)]
	public int Item4Star { get; set; }

	[Key(15)]
	public string Item5Code { get; set; }

	[Key(16)]
	public int Item5Count { get; set; }

	[Key(17)]
	public int Item5Star { get; set; }

	[Key(18)]
	public string Item6Code { get; set; }

	[Key(19)]
	public int Item6Count { get; set; }

	[Key(20)]
	public int Item6Star { get; set; }

	[Key(21)]
	public string Item7Code { get; set; }

	[Key(22)]
	public int Item7Count { get; set; }

	[Key(23)]
	public int Item7Star { get; set; }

	[Key(24)]
	public string Item8Code { get; set; }

	[Key(25)]
	public int Item8Count { get; set; }

	[Key(26)]
	public int Item8Star { get; set; }

	[Key(27)]
	public string Item9Code { get; set; }

	[Key(28)]
	public int Item9Count { get; set; }

	[Key(29)]
	public int Item9Star { get; set; }

	[Key(30)]
	public string Item10Code { get; set; }

	[Key(31)]
	public int Item10Count { get; set; }

	[Key(32)]
	public int Item10Star { get; set; }

	[Key(33)]
	public string Item11Code { get; set; }

	[Key(34)]
	public int Item11Count { get; set; }

	[Key(35)]
	public int Item11Star { get; set; }

	[Key(36)]
	public string Item12Code { get; set; }

	[Key(37)]
	public int Item12Count { get; set; }

	[Key(38)]
	public int Item12Star { get; set; }

	[Key(39)]
	public string Item13Code { get; set; }

	[Key(40)]
	public int Item13Count { get; set; }

	[Key(41)]
	public int Item13Star { get; set; }

	[Key(42)]
	public string Item14Code { get; set; }

	[Key(43)]
	public int Item14Count { get; set; }

	[Key(44)]
	public int Item14Star { get; set; }

	[Key(45)]
	public string Item15Code { get; set; }

	[Key(46)]
	public int Item15Count { get; set; }

	[Key(47)]
	public int Item15Star { get; set; }

	[Key(48)]
	public string Item16Code { get; set; }

	[Key(49)]
	public int Item16Count { get; set; }

	[Key(50)]
	public int Item16Star { get; set; }

	[Key(51)]
	public string Item17Code { get; set; }

	[Key(52)]
	public int Item17Count { get; set; }

	[Key(53)]
	public int Item17Star { get; set; }

	[Key(54)]
	public string Item18Code { get; set; }

	[Key(55)]
	public int Item18Count { get; set; }

	[Key(56)]
	public int Item18Star { get; set; }

	[Key(57)]
	public string Item19Code { get; set; }

	[Key(58)]
	public int Item19Count { get; set; }

	[Key(59)]
	public int Item19Star { get; set; }

	[Key(60)]
	public string Item20Code { get; set; }

	[Key(61)]
	public int Item20Count { get; set; }

	[Key(62)]
	public int Item20Star { get; set; }

	[Key(63)]
	public string Item21Code { get; set; }

	[Key(64)]
	public int Item21Count { get; set; }

	[Key(65)]
	public int Item21Star { get; set; }

	[Key(66)]
	public string Item22Code { get; set; }

	[Key(67)]
	public int Item22Count { get; set; }

	[Key(68)]
	public int Item22Star { get; set; }

	[Key(69)]
	public string Item23Code { get; set; }

	[Key(70)]
	public int Item23Count { get; set; }

	[Key(71)]
	public int Item23Star { get; set; }

	[Key(72)]
	public string Item24Code { get; set; }

	[Key(73)]
	public int Item24Count { get; set; }

	[Key(74)]
	public int Item24Star { get; set; }

	[Key(75)]
	public string Item25Code { get; set; }

	[Key(76)]
	public int Item25Count { get; set; }

	[Key(77)]
	public int Item25Star { get; set; }

	[Key(78)]
	public string Item26Code { get; set; }

	[Key(79)]
	public int Item26Count { get; set; }

	[Key(80)]
	public int Item26Star { get; set; }

	[Key(81)]
	public string Item27Code { get; set; }

	[Key(82)]
	public int Item27Count { get; set; }

	[Key(83)]
	public int Item27Star { get; set; }

	[Key(84)]
	public string Item28Code { get; set; }

	[Key(85)]
	public int Item28Count { get; set; }

	[Key(86)]
	public int Item28Star { get; set; }

	public List<SelectRewardItemInfo> GetSelectRewardItemList()
	{
		List<SelectRewardItemInfo> list = new List<SelectRewardItemInfo>();
		if (Item1Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item1Code, Item1Count, Item1Star));
		}
		if (Item2Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item2Code, Item2Count, Item2Star));
		}
		if (Item3Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item3Code, Item3Count, Item3Star));
		}
		if (Item4Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item4Code, Item4Count, Item4Star));
		}
		if (Item5Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item5Code, Item5Count, Item5Star));
		}
		if (Item6Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item6Code, Item6Count, Item6Star));
		}
		if (Item7Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item7Code, Item7Count, Item7Star));
		}
		if (Item8Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item8Code, Item7Count, Item8Star));
		}
		if (Item9Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item9Code, Item9Count, Item9Star));
		}
		if (Item10Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item10Code, Item10Count, Item10Star));
		}
		if (Item11Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item11Code, Item11Count, Item11Star));
		}
		if (Item12Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item12Code, Item12Count, Item12Star));
		}
		if (Item13Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item13Code, Item13Count, Item13Star));
		}
		if (Item14Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item14Code, Item14Count, Item14Star));
		}
		if (Item15Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item15Code, Item15Count, Item15Star));
		}
		if (Item16Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item16Code, Item16Count, Item16Star));
		}
		if (Item17Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item17Code, Item17Count, Item17Star));
		}
		if (Item18Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item18Code, Item18Count, Item18Star));
		}
		if (Item19Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item19Code, Item19Count, Item19Star));
		}
		if (Item20Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item20Code, Item20Count, Item20Star));
		}
		if (Item21Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item21Code, Item21Count, Item21Star));
		}
		if (Item22Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item22Code, Item22Count, Item22Star));
		}
		if (Item23Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item23Code, Item23Count, Item23Star));
		}
		if (Item24Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item24Code, Item24Count, Item24Star));
		}
		if (Item25Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item25Code, Item25Count, Item25Star));
		}
		if (Item26Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item26Code, Item26Count, Item26Star));
		}
		if (Item27Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item27Code, Item27Count, Item27Star));
		}
		if (Item28Count > 0)
		{
			list.Add(new SelectRewardItemInfo(Item28Code, Item28Count, Item28Star));
		}
		return list;
	}

	public bool IsSelectable()
	{
		return GetRewardItemListElementCount() > SelectCount;
	}

	public int GetRewardItemListElementCount()
	{
		int num = 0;
		if (Item1Count > 0)
		{
			num++;
		}
		if (Item2Count > 0)
		{
			num++;
		}
		if (Item3Count > 0)
		{
			num++;
		}
		if (Item4Count > 0)
		{
			num++;
		}
		if (Item5Count > 0)
		{
			num++;
		}
		if (Item6Count > 0)
		{
			num++;
		}
		if (Item7Count > 0)
		{
			num++;
		}
		if (Item8Count > 0)
		{
			num++;
		}
		if (Item9Count > 0)
		{
			num++;
		}
		if (Item10Count > 0)
		{
			num++;
		}
		if (Item11Count > 0)
		{
			num++;
		}
		if (Item12Count > 0)
		{
			num++;
		}
		if (Item13Count > 0)
		{
			num++;
		}
		if (Item14Count > 0)
		{
			num++;
		}
		if (Item15Count > 0)
		{
			num++;
		}
		if (Item16Count > 0)
		{
			num++;
		}
		if (Item17Count > 0)
		{
			num++;
		}
		if (Item18Count > 0)
		{
			num++;
		}
		if (Item19Count > 0)
		{
			num++;
		}
		if (Item20Count > 0)
		{
			num++;
		}
		if (Item21Count > 0)
		{
			num++;
		}
		if (Item22Count > 0)
		{
			num++;
		}
		if (Item23Count > 0)
		{
			num++;
		}
		if (Item24Count > 0)
		{
			num++;
		}
		if (Item25Count > 0)
		{
			num++;
		}
		if (Item26Count > 0)
		{
			num++;
		}
		if (Item27Count > 0)
		{
			num++;
		}
		if (Item28Count > 0)
		{
			num++;
		}
		return num;
	}

	public override string ToString()
	{
		return "ChapterIndex=" + ChapterIndex + " DungeonIndex=" + DungeonIndex;
	}
}

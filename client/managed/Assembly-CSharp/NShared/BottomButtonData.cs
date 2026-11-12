using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class BottomButtonData : IDataKeySelector<BottomButtonKey>
{
	[Key(0)]
	public ERightMenuButton Type { get; set; }

	[Key(1)]
	public string SubType { get; set; }

	[Key(2)]
	public int TutorialIndex { get; set; }

	[Key(3)]
	public string LabelKey { get; set; }

	[Key(4)]
	public float BgScale { get; set; }

	[Key(5)]
	public string ContentsOpenCode { get; set; }

	[Key(6)]
	public string[] ForceOpenDungeon { get; set; }

	BottomButtonKey IDataKeySelector<BottomButtonKey>.Key => new BottomButtonKey(Type, SubType);

	public override string ToString()
	{
		return "ButtonType = " + Type;
	}

	public List<Tuple<int, int, string>> GetForceOpenDungeon()
	{
		List<Tuple<int, int, string>> list = new List<Tuple<int, int, string>>();
		for (int i = 0; ForceOpenDungeon.Length > i; i += 3)
		{
			int valueInt = ArgParser.GetValueInt(ForceOpenDungeon, i);
			int valueInt2 = ArgParser.GetValueInt(ForceOpenDungeon, i + 1);
			string valueString = ArgParser.GetValueString(ForceOpenDungeon, i + 2);
			Tuple<int, int, string> item = new Tuple<int, int, string>(valueInt, valueInt2, valueString);
			list.Add(item);
		}
		return list;
	}
}

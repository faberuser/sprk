using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GameOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ParentNum { get; set; }

	[Key(2)]
	public bool ChildRoot { get; set; }

	[Key(3)]
	public bool SaveValue { get; set; }

	[Key(4)]
	public string OptionName { get; set; }

	[Key(5)]
	public GameOptionTab GameOptionTab { get; set; }

	[Key(6)]
	public GameOptionComponent GameOptionComponent { get; set; }

	[Key(7)]
	public string OptionDataType { get; set; }

	[Key(8)]
	public string ClassName { get; set; }

	[Key(9)]
	public string FieldName { get; set; }

	[Key(10)]
	public string PostProcessFunction { get; set; }

	[Key(11)]
	public string UIdefaultValue { get; set; }

	[Key(12)]
	public string ValueLocalKey1 { get; set; }

	[Key(13)]
	public string Value1 { get; set; }

	[Key(14)]
	public int[] DisableGroup1 { get; set; }

	[Key(15)]
	public string ValueLocalKey2 { get; set; }

	[Key(16)]
	public string Value2 { get; set; }

	[Key(17)]
	public int[] DisableGroup2 { get; set; }

	[Key(18)]
	public string ValueLocalKey3 { get; set; }

	[Key(19)]
	public string Value3 { get; set; }

	[Key(20)]
	public int[] DisableGroup3 { get; set; }

	[Key(21)]
	public string ValueLocalKey4 { get; set; }

	[Key(22)]
	public string Value4 { get; set; }

	[Key(23)]
	public int[] DisableGroup4 { get; set; }

	[Key(24)]
	public string ToolTipDesc { get; set; }

	int IDataKeySelector<int>.Key => Index;
}

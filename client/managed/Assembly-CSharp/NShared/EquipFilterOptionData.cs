using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipFilterOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Key { get; set; }

	[Key(2)]
	public string Group { get; set; }

	[Key(3)]
	public string ReferenceValue { get; set; }

	[Key(4)]
	public string[] RedirectionValues { get; set; }

	[Key(5)]
	public bool IsDefault { get; set; }

	[Key(6)]
	public bool ChangeOnOpenInHeroEquip { get; set; }

	[Key(7)]
	public int Step { get; set; }

	[Key(8)]
	public bool Show { get; set; }

	[Key(9)]
	public string LocalKey { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return $"Index={Index.ToString()}, Group = {Group}, Option={Key}";
	}

	public void ConvNullToZero()
	{
		if (String_Extension_Method.IsNullOrEmpty(ReferenceValue))
		{
			ReferenceValue = "0";
		}
	}
}

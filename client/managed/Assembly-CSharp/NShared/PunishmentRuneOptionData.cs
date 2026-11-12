using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRuneOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public StatType[] StatTypes { get; set; }

	[Key(2)]
	public int OptionValueMin0 { get; set; }

	[Key(3)]
	public int OptionValueMax0 { get; set; }

	int IDataKeySelector<int>.Key => Index;
}

using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRuneItemCountData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => PunishmentRuneCount;

	[Key(0)]
	public int PunishmentRuneCount { get; set; }

	[Key(1)]
	public int RuneFragmentCount { get; set; }

	[Key(2)]
	public int ApostleBloodCount { get; set; }
}

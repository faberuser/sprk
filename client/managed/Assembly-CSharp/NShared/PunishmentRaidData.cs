using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRaidData : IDataKeySelector<RaidDataKey2>
{
	[Key(0)]
	public int RaidIndex { get; set; }

	[Key(1)]
	public int RaidLevel { get; set; }

	[Key(2)]
	public int GroupIndex { get; set; }

	[Key(3)]
	public byte Boss { get; set; }

	[Key(4)]
	public int EntryPositionX { get; set; }

	[Key(5)]
	public int EntryPositionY { get; set; }

	[Key(6)]
	public string EntryEffectCode { get; set; }

	[Key(7)]
	public int[] TriggerIndices { get; set; }

	[Key(8)]
	public string EntryLocalKey { get; set; }

	[Key(9)]
	public string ImagePath { get; set; }

	[Key(10)]
	public string LabelColor { get; set; }

	[Key(11)]
	public int PortraitPositionX { get; set; }

	[Key(12)]
	public int PortraitPositionY { get; set; }

	[Key(13)]
	public EPunishmentPortraitLineType PortraitLineType { get; set; }

	[Key(14)]
	public string[] CanEnterCondition { get; set; }

	[Key(15)]
	public string[] WaveGimmickValues { get; set; }

	[Key(16)]
	public string BossMiniPortraitPath { get; set; }

	[Key(17)]
	public string WarningLocalKey { get; set; }

	[Key(18)]
	public bool UsePunishmentRaidBossStatus { get; set; }

	RaidDataKey2 IDataKeySelector<RaidDataKey2>.Key => new RaidDataKey2(RaidIndex, RaidLevel);

	[IgnoreMember]
	public bool IsBoss => Boss == 1;

	public override string ToString()
	{
		return "PunishmentRaidData Index=" + RaidIndex;
	}
}

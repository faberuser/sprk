using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildAttendanceRewardData : IDataKeySelector<GuildAttendanceRewardDataKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Day { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public int RewardIndex { get; set; }

	GuildAttendanceRewardDataKey IDataKeySelector<GuildAttendanceRewardDataKey>.Key => new GuildAttendanceRewardDataKey(Index, Day);
}

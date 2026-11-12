using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildPenaltyData : IDataKeySelector<int>
{
	[Key(0)]
	public int WithdrawCount { get; set; }

	[Key(1)]
	public int RejoinTimeMin { get; set; }

	[Key(2)]
	public int RestrictTimeMin { get; set; }

	[Key(3)]
	public bool ShowMinitue { get; set; }

	int IDataKeySelector<int>.Key => WithdrawCount;

	public override string ToString()
	{
		return "WithdrawCount[" + WithdrawCount + "] RejoinTimeMin = " + RejoinTimeMin + " RestrictTimeMin = " + RestrictTimeMin;
	}
}

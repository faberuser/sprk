using System.Linq;
using NShared.NJit;

namespace NShared;

public class GuildPenaltyDataContainer : JitDataContainerDefault<int, GuildPenaltyData>
{
	public const string TABLENAME = "GuildPenaltyTable";

	private int _minWithdrawCount;

	private int _maxWithdrawCount;

	public override TablePathDefine PathDefine => new TablePathDefine("GuildPenaltyTable");

	protected override void OnEndLoad_Final()
	{
		_minWithdrawCount = (from v in GetAllData()
			select v.WithdrawCount).DefaultIfEmpty(0).Min();
		_maxWithdrawCount = (from v in GetAllData()
			select v.WithdrawCount).DefaultIfEmpty(0).Max();
	}

	public GuildPenaltyData GetPanaltyData(int withdrawCount)
	{
		if (withdrawCount < _minWithdrawCount)
		{
			withdrawCount = _minWithdrawCount;
		}
		if (withdrawCount > _maxWithdrawCount)
		{
			withdrawCount = _maxWithdrawCount;
		}
		return GetData(withdrawCount);
	}

	public int GetRestrictHour(int withdrawCount)
	{
		GuildPenaltyData panaltyData = GetPanaltyData(withdrawCount);
		if (panaltyData == null)
		{
			return 0;
		}
		if (panaltyData.ShowMinitue)
		{
			return panaltyData.RestrictTimeMin;
		}
		return panaltyData.RestrictTimeMin / 60;
	}

	public int GetReJoinHour(int withdrawCount)
	{
		GuildPenaltyData panaltyData = GetPanaltyData(withdrawCount);
		if (panaltyData == null)
		{
			return 0;
		}
		if (panaltyData.ShowMinitue)
		{
			return panaltyData.RejoinTimeMin;
		}
		return panaltyData.RejoinTimeMin / 60;
	}
}

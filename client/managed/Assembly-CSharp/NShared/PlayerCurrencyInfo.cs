using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerCurrencyInfo : IAccountAttrKey<CurrencyType>
{
	[NonDbSchema]
	private DateTime _cachedNextResetTime;

	CurrencyType IAccountAttrKey<CurrencyType>.Key => CurrencyType;

	public CurrencyType CurrencyType { get; set; }

	public long Amount { get; set; }

	public long DailyAmount { get; set; }

	public string DailyResetTime { get; set; }

	[NonDbSchema]
	public long NextResetTime { get; set; }

	public void SetNextResetTime(DateTime currentServerUtcTime)
	{
		_cachedNextResetTime = ((NextResetTime < 0) ? DateTime.MaxValue : currentServerUtcTime.AddSeconds(NextResetTime));
	}

	public DateTime GetNextResetTime()
	{
		return _cachedNextResetTime;
	}
}

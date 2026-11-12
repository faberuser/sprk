using System;
using NShared;

namespace NGame2.NAccount;

public class StaminaInfo
{
	public int MaxValue;

	public int RechargeCostGold;

	public int RechargeCostGem;

	public DateTime NextRechargeTime;

	public DateTime FullRechargeTime;

	public float RequestedTime;

	private readonly StaminaData _staminaData;

	private readonly Func<long> _curValueFunc;

	private readonly Func<int> _rechargeCountFunc;

	public long CurValue => Vespa_System_Action_Extension_Method.fire(_curValueFunc);

	public int RechargeCount => Vespa_System_Action_Extension_Method.fire(_rechargeCountFunc);

	public StaminaData StaminaData => _staminaData;

	public StaminaInfo(StaminaData staminaData, Func<long> curValueFunc, Func<int> rechargeCountFunc)
	{
		_staminaData = staminaData;
		_curValueFunc = curValueFunc;
		_rechargeCountFunc = rechargeCountFunc;
	}
}

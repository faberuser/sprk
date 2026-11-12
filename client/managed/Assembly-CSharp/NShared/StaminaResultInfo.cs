namespace NShared;

[JsonSerializable(Javascript = true)]
public class StaminaResultInfo
{
	public ExpResultInfo SprkRaiderExp;

	public StaminaType Type { get; set; }

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public string StaminaRechargeTime { get; set; }

	public int NextRechargeRemainTime { get; set; }

	public int FullRechargeRemainTime { get; set; }

	public int RechargeCount { get; set; }

	public bool IsHide { get; set; }
}

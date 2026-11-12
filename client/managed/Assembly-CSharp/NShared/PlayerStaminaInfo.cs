namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerStaminaInfo
{
	public StaminaType StaminaType { get; set; }

	public int Amount { get; set; }

	public string RechargeTime { get; set; }

	public int RechargeCount { get; set; }
}

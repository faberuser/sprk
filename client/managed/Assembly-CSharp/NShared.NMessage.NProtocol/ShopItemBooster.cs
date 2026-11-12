namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ShopItemBooster
{
	public int ShopIndex { get; set; }

	public int Index { get; set; }

	public string ExpireTime { get; set; }

	public PlayerPurchaseTimeDurationInfo PlayerPurchaseTimeDuration { get; set; }

	public PlayerFirstPurchaseInfo PlayerFirstPurchase { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public AttendanceInfo Attendance { get; set; }

	public AttendanceData[] AttendanceDatas { get; set; }

	public int[] AttendanceRewardableDays { get; set; }
}

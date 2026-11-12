namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class WebPayShopPurchase
{
	public PlayerProductPurchaseInfo PlayerProductPurchase { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public AttendanceInfo Attendance { get; set; }

	public AttendanceData[] AttendanceDatas { get; set; }

	public int[] AttendanceRewardableDays { get; set; }
}

using NShared;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class RestoreSoulStoneRewardInfo
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public long AddSoulStoneMileage { get; set; }

	public bool IsGetBonus { get; set; }

	public int[] SelectSoulStoneIndices { get; set; }

	public bool IsConfirmResponse { get; set; }
}

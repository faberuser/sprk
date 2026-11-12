using NShared.UpgradeEquip;

namespace NShared.SoulWeaponLimitBreak;

[Response]
public class Response : GenericResponse<ResultType>, IResponse
{
	public bool Success { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public int[] RemovedEquipItemSlotIndices { get; set; }

	public ItemResultInfo[] RemovedItemResultInfos { get; set; }
}

namespace NShared.UpgradeEquip;

public interface IResponse
{
	EquipItemInfo ResultEquipItem { get; }

	CurrencyResultInfo3 CurrencyResult { get; }

	ItemResultInfo[] ItemResults { get; }

	int[] RemovedEquipItemSlotIndices { get; }
}

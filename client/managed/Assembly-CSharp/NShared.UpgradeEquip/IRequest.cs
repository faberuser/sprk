namespace NShared.UpgradeEquip;

public interface IRequest
{
	int EquipItemSlotIndex { get; set; }

	int[] MaterialSlotIndices { get; set; }
}

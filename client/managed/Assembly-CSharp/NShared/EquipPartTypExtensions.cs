namespace NShared;

internal static class EquipPartTypExtensions
{
	public static int GetEquipSlotIndex(this EquipPartType equipPartType)
	{
		return GameRoot.Instance.EquipSlotDataContainer.GetEquipSlotIndexFor(equipPartType);
	}

	public static int[] GetEquipSlotIndices(this EquipPartType equipPartType)
	{
		return GameRoot.Instance.EquipSlotDataContainer.GetEquipSlotIndicesFor(equipPartType);
	}
}

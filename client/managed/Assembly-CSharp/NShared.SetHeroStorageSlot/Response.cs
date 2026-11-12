namespace NShared.SetHeroStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroInfo[] Heroes { get; set; }

	public EquipItemInfo[] EquipItems { get; set; }

	public int[] UnEquippedSlotIndices { get; set; }

	public int[] EquipStorageSlotIndex { get; set; }
}

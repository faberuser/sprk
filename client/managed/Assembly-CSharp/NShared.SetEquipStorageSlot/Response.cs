namespace NShared.SetEquipStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int NewHeroIndex { get; set; }

	public int[] NewEquippedSlotIndex { get; set; }

	public int[] NewUnEquippedSlotIndex { get; set; }

	public int OldHeroIndex { get; set; }

	public int[] OldUnEquippedSlotIndex { get; set; }

	public int EquipStorageSlotIndex { get; set; }

	public int HeroIndex { get; set; }

	public HeroInfo HeroInfo { get; set; }

	public int[] UnEquippedSlotIndices { get; set; }
}

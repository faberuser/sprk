namespace NGame2.NCutScene;

public class CreatureSlotHolder : ISlotHolder
{
	private ISlotProvider _slotProvider;

	public int SlotIndex { get; set; }

	public CreatureSlotHolder(ISlotProvider provider)
	{
		_slotProvider = provider;
	}
}

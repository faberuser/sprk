namespace NShared;

public interface ISubItemDataContainer
{
	ItemType[] GetSubTypes();

	ItemData CreateItemWithSubItem(ItemBaseData itemBase);
}

using NShared.NJit;

namespace NShared;

public abstract class SubItemDataContainer<V> : JitDataContainerDefault<string, V>, ISubItemDataContainer where V : class, IItemSubData, IDataKeySelector<string>
{
	public abstract ItemType[] GetSubTypes();

	ItemData ISubItemDataContainer.CreateItemWithSubItem(ItemBaseData itemBase)
	{
		V data = GetData(itemBase.Code);
		if (data == null)
		{
			return new ItemDataWithNullSub(itemBase);
		}
		return new ItemData<V>(itemBase, data);
	}
}

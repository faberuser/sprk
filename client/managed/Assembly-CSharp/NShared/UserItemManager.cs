namespace NShared;

public class UserItemManager : AccountAttrManager<int, ItemInfo>
{
	public int GetItemCount(int itemIndex)
	{
		return Get(itemIndex)?.Count ?? 0;
	}

	public int GetItemCount(string itemCode)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		if (dataByCode == null)
		{
			return 0;
		}
		return GetItemCount(dataByCode.Index);
	}
}

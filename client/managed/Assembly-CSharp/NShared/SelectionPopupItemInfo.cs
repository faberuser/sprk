namespace NShared;

public class SelectionPopupItemInfo
{
	public ItemData Data { get; private set; }

	public int Count { get; private set; }

	public SelectionPopupItemInfo(ItemData data, int count)
	{
		Data = data;
		Count = count;
	}
}

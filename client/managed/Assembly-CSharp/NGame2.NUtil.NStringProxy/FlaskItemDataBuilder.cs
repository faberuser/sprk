using NShared;

namespace NGame2.NUtil.NStringProxy;

public class FlaskItemDataBuilder : IBuilder
{
	private ItemData _itemData;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public FlaskItemDataBuilder(ItemData itemData, bool appendLine)
	{
		_itemData = itemData;
		_appendLine = appendLine;
	}

	public string Build()
	{
		ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(_itemData.Index);
		if (data != null)
		{
			return _itemData.Description2.Replace("{1}", data.SubData.FlaskExp.ToString());
		}
		return _itemData.Description2;
	}
}

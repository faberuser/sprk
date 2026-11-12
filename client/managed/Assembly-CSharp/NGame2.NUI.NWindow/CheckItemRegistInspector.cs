using System;
using NShared;

namespace NGame2.NUI.NWindow;

[Serializable]
public class CheckItemRegistInspector
{
	public int ItemIndex;

	public ItemType ItemType;

	public bool CheckItem(ItemData itemData)
	{
		if (itemData == null)
		{
			return false;
		}
		if (itemData.Index == ItemIndex)
		{
			return true;
		}
		if (itemData.Type == ItemType)
		{
			return true;
		}
		return false;
	}
}

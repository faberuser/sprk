using System.Collections.Generic;

namespace NGame2.NUI.NComponent2.NHelper;

public class ItemAnimationContainer
{
	private bool _open3dModelPopup;

	private List<GetItemAnimationData> _itemAnimationList = new List<GetItemAnimationData>();

	public bool IsEmpty => __Extension_Method_ICollection__.IsNullOrEmpty(_itemAnimationList);

	public ItemAnimationContainer(bool open3dModelPopup)
	{
		_open3dModelPopup = open3dModelPopup;
	}

	public void Reset()
	{
		for (int i = 0; i < _itemAnimationList.Count; i++)
		{
			_itemAnimationList[i].Close();
		}
		_itemAnimationList.Clear();
	}

	public void Add(GetItemAnimationData getItemAnimation)
	{
		if (getItemAnimation != null)
		{
			_itemAnimationList.Add(getItemAnimation);
		}
	}

	public void ChainUpdate()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_itemAnimationList))
		{
			GetItemAnimationData getItemAnimationData = _itemAnimationList[0];
			if (getItemAnimationData.State == GetItemAnimationData.EState.Ready)
			{
				getItemAnimationData.Play(_open3dModelPopup);
			}
			else if (getItemAnimationData.CanNext)
			{
				_itemAnimationList.RemoveAt(0);
			}
		}
	}
}

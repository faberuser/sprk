using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow.NPortal;

public class PortalMenuController
{
	private LocalAccountString _portalContentsMenu = new LocalAccountString(string.Empty, typeof(LobbyManagement).FullName + "_PortalContentsMenu_2008041752");

	private List<PortalRenewalCategory> _localPortalContentsMenuCategoryList = new List<PortalRenewalCategory>();

	private List<PortalRenewalData> _portalRenewalDataList = new List<PortalRenewalData>();

	public List<PortalRenewalData> PortalRenewalDataList => _portalRenewalDataList;

	public PortalMenuController()
	{
		InitLocalPortalContentsMenuCategoryList();
		InitPortalMenu();
	}

	public void Close()
	{
		if (_portalRenewalDataList != null)
		{
			_portalRenewalDataList.Clear();
			_portalRenewalDataList = null;
		}
	}

	private void InitLocalPortalContentsMenuCategoryList()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_localPortalContentsMenuCategoryList))
		{
			return;
		}
		_localPortalContentsMenuCategoryList.Clear();
		if (_portalContentsMenu.Value.Equals(string.Empty))
		{
			return;
		}
		string[] array = _portalContentsMenu.Value.Split(',');
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			return;
		}
		int result = 0;
		PortalRenewalCategory portalRenewalCategory = PortalRenewalCategory.NONE;
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out result))
			{
				portalRenewalCategory = (PortalRenewalCategory)result;
				if (portalRenewalCategory > PortalRenewalCategory.NONE)
				{
					_localPortalContentsMenuCategoryList.Add(portalRenewalCategory);
				}
			}
		}
	}

	private void InitPortalMenu()
	{
		InitDefualtSortPortalMenu();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_localPortalContentsMenuCategoryList) && _localPortalContentsMenuCategoryList.Count == _portalRenewalDataList.Count)
		{
			SwapPortalRenewData();
		}
	}

	public void InitDefualtSortPortalMenu()
	{
		_portalRenewalDataList.Clear();
		_portalRenewalDataList = GameRoot.Instance.PortalRenewalDataContainer.GetCategoryDatas().ToList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalDataList))
		{
			DefaultSortPortalMenu(ref _portalRenewalDataList);
		}
	}

	private void DefaultSortPortalMenu(ref List<PortalRenewalData> list)
	{
		list.Sort((PortalRenewalData left, PortalRenewalData right) => right.Pined.CompareTo(left.Pined));
		list.Sort((PortalRenewalData left, PortalRenewalData right) => (left.Pined && right.Pined) ? left.MenuShowIndex.CompareTo(right.MenuShowIndex) : 0);
		list.Sort((PortalRenewalData left, PortalRenewalData right) => left.MenuShowIndex.CompareTo(right.MenuShowIndex));
	}

	private void SwapPortalRenewData()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_localPortalContentsMenuCategoryList))
		{
			return;
		}
		PortalRenewalCategory localCategory = PortalRenewalCategory.NONE;
		int num = 0;
		PortalRenewalData portalRenewalData = null;
		for (int i = 0; i < _localPortalContentsMenuCategoryList.Count; i++)
		{
			if (_portalRenewalDataList.Count <= i)
			{
				continue;
			}
			portalRenewalData = _portalRenewalDataList[i];
			if (portalRenewalData == null)
			{
				continue;
			}
			localCategory = _localPortalContentsMenuCategoryList[i];
			if (localCategory != portalRenewalData.Category)
			{
				num = _portalRenewalDataList.FindIndex((PortalRenewalData e) => e.Category == localCategory);
				if (num >= 0 && num != i)
				{
					Vespa_Generic_IList_Extension_Method.Swap(_portalRenewalDataList, i, num);
				}
			}
		}
	}

	public void SavePortalMenuCategory()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalDataList))
		{
			return;
		}
		_localPortalContentsMenuCategoryList.Clear();
		PortalRenewalData portalRenewalData = null;
		string text = string.Empty;
		for (int i = 0; i < _portalRenewalDataList.Count; i++)
		{
			portalRenewalData = _portalRenewalDataList[i];
			if (portalRenewalData != null)
			{
				_localPortalContentsMenuCategoryList.Add(portalRenewalData.Category);
				text = text + (int)portalRenewalData.Category + ",";
			}
		}
		_portalContentsMenu.Value = text;
	}

	public bool CheckPossibleSwap(int index, int direction)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalDataList))
		{
			return false;
		}
		if (index < 0 || index >= _portalRenewalDataList.Count)
		{
			return false;
		}
		PortalRenewalData portalRenewalData = _portalRenewalDataList[index];
		if (portalRenewalData == null)
		{
			return false;
		}
		if (portalRenewalData.Pined)
		{
			return false;
		}
		int possibleSwapIndex = GetPossibleSwapIndex(index, direction);
		if (possibleSwapIndex < 0 || possibleSwapIndex >= _portalRenewalDataList.Count)
		{
			return false;
		}
		return true;
	}

	public bool SwapPortalRenewalCategory(int index, int direction, out PortalRenewalData toSwapData, out PortalRenewalData fromSwapData, out int swapIndex)
	{
		toSwapData = null;
		fromSwapData = null;
		swapIndex = -1;
		if (!CheckPossibleSwap(index, direction))
		{
			return false;
		}
		swapIndex = GetPossibleSwapIndex(index, direction);
		if (swapIndex < 0 || swapIndex >= _portalRenewalDataList.Count)
		{
			return false;
		}
		Vespa_Generic_IList_Extension_Method.Swap(_portalRenewalDataList, index, swapIndex);
		toSwapData = _portalRenewalDataList[swapIndex];
		fromSwapData = _portalRenewalDataList[index];
		return true;
	}

	public bool CheckRevertPortalContentsMenu()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalDataList))
		{
			return false;
		}
		List<PortalRenewalData> list = GameRoot.Instance.PortalRenewalDataContainer.GetCategoryDatas().ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return false;
		}
		DefaultSortPortalMenu(ref list);
		for (int i = 0; i < _portalRenewalDataList.Count; i++)
		{
			if (list.Count >= i && _portalRenewalDataList[i].Category != list[i].Category)
			{
				return true;
			}
		}
		return false;
	}

	private int GetPossibleSwapIndex(int index, int direction)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalDataList))
		{
			return -1;
		}
		PortalRenewalData portalRenewalData = null;
		for (int i = index + direction; i < _portalRenewalDataList.Count && i >= 0; i += direction)
		{
			portalRenewalData = _portalRenewalDataList[i];
			if (portalRenewalData != null && !portalRenewalData.Pined)
			{
				return i;
			}
		}
		return -1;
	}
}

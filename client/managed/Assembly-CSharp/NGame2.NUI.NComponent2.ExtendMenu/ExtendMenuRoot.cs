using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ExtendMenu;

public class ExtendMenuRoot : MonoBehaviour, ExtendMenu.IListener
{
	public interface IListener
	{
		void SelectExtendMenu(int mainIndex, int subIndex);
	}

	public ExtendMenu ExtendMenu;

	private IListener _iListener;

	private Dictionary<int, bool> _isNewDic = new Dictionary<int, bool>();

	private const int BaseExtendMenuIndex = 0;

	public int SelectMainIndex { get; private set; }

	public int SelectSubIndex { get; private set; }

	public int ExtendMenuCount { get; set; }

	public bool IsNew => _isNewDic.ContainsValue(value: true);

	public int GetBaseExtendMenuIndex()
	{
		return 0;
	}

	private void Awake()
	{
		Apply();
	}

	public void Init(IListener iListener)
	{
		_iListener = iListener;
	}

	public void Apply()
	{
		ExtendMenuCount = 0;
		_isNewDic.Clear();
		ExtendMenu.Open(this);
		SelectFirstMenu();
	}

	private void RemoveAllChild()
	{
		if (ExtendMenu == null)
		{
			return;
		}
		Transform transform = null;
		for (int num = ExtendMenu.transform.childCount - 1; num >= 0; num--)
		{
			transform = ExtendMenu.transform.GetChild(num);
			if (!(transform == null))
			{
				Object.Destroy(transform.gameObject);
			}
		}
	}

	private void SelectFirstMenu()
	{
		if (!(ExtendMenu == null) && ExtendMenu.transform.childCount > 0)
		{
			if (ExtendMenu.transform.GetChild(0).GetComponent<ExtendMenuComponent>() == null)
			{
				SelectFirstExtendMenu();
			}
			DisableInActive();
		}
	}

	private void SelectFirstExtendMenu()
	{
		Transform transform = null;
		ExtendMenuComponent extendMenuComponent = null;
		ExtendMenu extendMenuByIndex = GetExtendMenuByIndex(1);
		if (extendMenuByIndex == null || extendMenuByIndex.transform.childCount < 1)
		{
			return;
		}
		extendMenuByIndex.EnableChild(isEnable: true);
		SelectMainIndex = 1;
		for (int i = 0; i < extendMenuByIndex.transform.childCount; i++)
		{
			transform = extendMenuByIndex.transform.GetChild(i);
			if (transform == null)
			{
				continue;
			}
			extendMenuComponent = transform.GetComponent<ExtendMenuComponent>();
			if (!(extendMenuComponent == null))
			{
				if (extendMenuComponent.Index == 0)
				{
					extendMenuComponent.SelectMenu(isSelect: true);
				}
				if (extendMenuComponent.Index == 1)
				{
					SelectSubIndex = extendMenuComponent.Index;
					extendMenuComponent.SelectMenu(isSelect: true);
				}
			}
		}
	}

	private void DisableInActiveExtendMenu()
	{
		ExtendMenu[] componentsInChildren = ExtendMenu.transform.GetComponentsInChildren<ExtendMenu>(includeInactive: true);
		foreach (ExtendMenu extendMenu in componentsInChildren)
		{
			if (!(extendMenu == null) && !extendMenu.isActiveAndEnabled && !(extendMenu.MainExtendMenuComponent == null))
			{
				extendMenu.MainExtendMenuComponent.SelectMenu(isSelect: false);
			}
		}
	}

	private void DisableInActiveExtendMenuComponent()
	{
		ExtendMenuComponent[] componentsInChildren = ExtendMenu.transform.GetComponentsInChildren<ExtendMenuComponent>(includeInactive: true);
		foreach (ExtendMenuComponent extendMenuComponent in componentsInChildren)
		{
			if (!(extendMenuComponent == null) && !extendMenuComponent.isActiveAndEnabled)
			{
				extendMenuComponent.SelectMenu(isSelect: false);
			}
		}
	}

	private void DisableInActive()
	{
		DisableInActiveExtendMenu();
		if (SelectMainIndex == 0 || SelectSubIndex != 0)
		{
			DisableInActiveExtendMenuComponent();
		}
	}

	public void SetNew(int mainIndex, int subIndex, bool isNew)
	{
		ExtendMenu extendMenuByIndex = GetExtendMenuByIndex(mainIndex);
		if (extendMenuByIndex == null)
		{
			return;
		}
		Transform transform = null;
		ExtendMenuComponent extendMenuComponent = null;
		for (int i = 0; i < extendMenuByIndex.transform.childCount; i++)
		{
			transform = extendMenuByIndex.transform.GetChild(i);
			if (!(transform == null))
			{
				extendMenuComponent = transform.GetComponent<ExtendMenuComponent>();
				if (!(extendMenuComponent == null) && extendMenuComponent.Index == subIndex)
				{
					extendMenuComponent.SetEnableNew(isNew);
					break;
				}
			}
		}
		extendMenuByIndex.SetEnablNew((extendMenuByIndex.transform.childCount > 1) ? CheckExistIsNew(extendMenuByIndex.transform) : isNew);
		if (_isNewDic.ContainsKey(mainIndex))
		{
			_isNewDic[mainIndex] = isNew;
		}
		else
		{
			_isNewDic.Add(mainIndex, isNew);
		}
	}

	public void SetEventOrBooster(int mainIndex, bool isEvent, bool isBooster)
	{
		ExtendMenu extendMenuByIndex = GetExtendMenuByIndex(mainIndex);
		if (!(extendMenuByIndex == null))
		{
			extendMenuByIndex.SetEnableEvent(isEvent);
			extendMenuByIndex.SetEnableBooster(isBooster);
		}
	}

	private bool CheckExistIsNew(Transform parentTranform)
	{
		Transform transform = null;
		ExtendMenuComponent extendMenuComponent = null;
		for (int i = 0; i < parentTranform.childCount; i++)
		{
			transform = parentTranform.GetChild(i);
			if (!(transform == null))
			{
				extendMenuComponent = transform.GetComponent<ExtendMenuComponent>();
				if (!(extendMenuComponent == null) && extendMenuComponent.Index != 0 && extendMenuComponent.IsNew)
				{
					return true;
				}
			}
		}
		return false;
	}

	private ExtendMenu GetExtendMenuByIndex(int index)
	{
		ExtendMenu extendMenu = null;
		ExtendMenu[] componentsInChildren = ExtendMenu.transform.GetComponentsInChildren<ExtendMenu>(includeInactive: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			extendMenu = componentsInChildren[i];
			if (!(extendMenu == null) && extendMenu.Index == index)
			{
				return extendMenu;
			}
		}
		return extendMenu;
	}

	void ExtendMenu.IListener.Click(int mainIndex, ExtendMenuComponent component)
	{
		if (_iListener != null && !(component == null))
		{
			_iListener.SelectExtendMenu(mainIndex, component.Index);
			SelectMainIndex = mainIndex;
			SelectSubIndex = component.Index;
		}
	}
}

using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HonoraryChamberMenuComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClick(HonoraryChamberMenuComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/HonoraryChamber/HonoraryChamberMenuComponent";

	public static PrefabAuto<HonoraryChamberMenuComponent> PrefabAuto = new PrefabAuto<HonoraryChamberMenuComponent>("Prefab2/Component/HonoraryChamber/HonoraryChamberMenuComponent");

	public UIToggle Toggle_Main;

	public UIToggle Toggle_Sub;

	public UIToggle Toggle_Base;

	private IListener _listener;

	public UILabel[] LabelNameArray;

	public UISprite SpriteArrowNormal;

	public UISprite SpriteArrowDown;

	private HonoraryChamberMenuComponent _parent;

	private List<HonoraryChamberMenuComponent> _subMenuList = new List<HonoraryChamberMenuComponent>();

	private bool _isExtend;

	public HonoraryChamberMenuData MenuData { get; private set; }

	public void SetParentMenu(HonoraryChamberMenuComponent parentMenu)
	{
		_parent = parentMenu;
	}

	public void AddSubMenuList(List<HonoraryChamberMenuComponent> subMenuList)
	{
		_subMenuList.AddRange(subMenuList);
	}

	public void Open(IListener listener, HonoraryChamberMenuData menuData)
	{
		if (listener == null || menuData == null)
		{
			VespaDebug.LogError("listener == null || menuData == null");
			return;
		}
		_listener = listener;
		MenuData = menuData;
		Init();
	}

	private void Init()
	{
		SetToggle();
		SetName();
		SetArrow();
	}

	private void SetToggle()
	{
		bool flag = MenuData.ParentIndex == 0;
		Toggle_Base = (flag ? Toggle_Main : Toggle_Sub);
		UIHelper.EnableGameObject(Toggle_Main, flag);
		UIHelper.EnableGameObject(Toggle_Sub, !flag);
	}

	private void SetName()
	{
		string stringDirect = LocalizationManager.GetStringDirect(MenuData.Name);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LabelNameArray); i++)
		{
			UIHelper.SetLabelText(LabelNameArray[i], stringDirect);
		}
	}

	public void SetExtend(bool extend)
	{
		if (_isExtend == extend)
		{
			return;
		}
		_isExtend = extend;
		if (_isExtend)
		{
			_subMenuList.ForEach(delegate(HonoraryChamberMenuComponent e)
			{
				UIHelper.EnableGameObject(e, enable: true);
			});
		}
		else
		{
			_subMenuList.ForEach(delegate(HonoraryChamberMenuComponent e)
			{
				e.DeselectDisale();
			});
		}
		SetArrow();
	}

	private void SetArrow()
	{
		UIHelper.EnableGameObject(SpriteArrowNormal, !_isExtend);
		UIHelper.EnableGameObject(SpriteArrowDown, _isExtend);
	}

	public void OnCreate()
	{
	}

	public void OnRelease()
	{
		_isExtend = false;
		Toggle_Base.Set(state: false);
		_parent = null;
		_subMenuList.Clear();
	}

	public void SetSelect(bool select)
	{
		Toggle_Base.Set(select);
	}

	public bool IsSelected()
	{
		return Toggle_Base.value;
	}

	public void OnValueChanged()
	{
		if (_listener != null && !(Toggle_Base == null))
		{
			if (Toggle_Base.value)
			{
				OnMenuSelect();
			}
			else
			{
				SetExtend(extend: false);
			}
		}
	}

	private void DeselectDisale()
	{
		SetSelect(select: false);
		UIHelper.EnableGameObject(this, enable: false);
	}

	private void OnMenuSelect()
	{
		if (MenuData.ContentsType == RecordsOfHonorContentType.None)
		{
			HonoraryChamberMenuComponent firstSubMenu = GetFirstSubMenu();
			if (!(firstSubMenu == null) && !firstSubMenu.isActiveAndEnabled)
			{
				HonoraryChamberMenuComponent firstMenuRecursive = GetFirstMenuRecursive(this);
				if (!(firstMenuRecursive == this))
				{
					UIHelper.EnableGameObject(firstMenuRecursive, enable: true);
					firstMenuRecursive.SetSelect(select: true);
					firstMenuRecursive.OnValueChanged();
				}
			}
		}
		else
		{
			HonoraryChamberMenuComponent parent = _parent;
			while (parent != null)
			{
				parent.SetExtend(extend: true);
				parent.SetSelect(select: true);
				parent = parent._parent;
			}
			_listener.OnClick(this);
		}
	}

	private static HonoraryChamberMenuComponent GetFirstMenuRecursive(HonoraryChamberMenuComponent menu)
	{
		HonoraryChamberMenuComponent firstSubMenu = menu.GetFirstSubMenu();
		if (firstSubMenu == null)
		{
			return menu;
		}
		return GetFirstMenuRecursive(firstSubMenu);
	}

	private HonoraryChamberMenuComponent GetFirstSubMenu()
	{
		if (__Extension_Method_ICollection__.COUNT(_subMenuList) != 0)
		{
			return _subMenuList[0];
		}
		return null;
	}
}

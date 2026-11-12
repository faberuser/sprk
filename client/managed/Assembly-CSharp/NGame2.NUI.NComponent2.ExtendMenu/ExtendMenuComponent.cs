using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ExtendMenu;

public class ExtendMenuComponent : UIScrollItemBehaviour
{
	public new interface IListener
	{
		void Click(ExtendMenuComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/ExtendMenu/ExtendMenuComponent";

	public static PrefabAuto<ExtendMenuComponent> PrefabAuto = new PrefabAuto<ExtendMenuComponent>("Prefab2/Component/ExtendMenu/ExtendMenuComponent");

	private const int ToggleGroup = 47;

	private const int SubMenuShortSizeX = 30;

	public UISprite NormalIndicatorSprite;

	public UISprite ExtendIndicatorSprite;

	public UIWidget NormalWidget;

	public UIWidget SelectWidget;

	public UILabel Label;

	public UILabel SelectLabel;

	public UISprite NewSprite;

	public UISprite EventSprite;

	public UISprite BoosterSprite;

	private UIToggle _toggle;

	private IListener _iListener;

	public string LabelText { get; set; }

	public int Index { get; private set; }

	public bool IsNew { get; private set; }

	public bool IsEvent { get; private set; }

	public bool IsBooster { get; private set; }

	public bool IsSelect
	{
		get
		{
			if (_toggle == null)
			{
				return false;
			}
			return _toggle.value;
		}
	}

	public void Open(int index)
	{
		Init(index);
	}

	private void Init(int index)
	{
		Index = index;
		InitListener();
		InitToggle();
		InitIndicator();
		SetLabel();
		SetEnableNew(isEnable: false);
		SetEnableEvent(isEnable: false);
		SetEnableBooster(isEnable: false);
	}

	private void InitSubMenuSize()
	{
		ExtendMenu parent = GetParent();
		if ((!(parent != null) || parent.Index != 0) && Index != 0)
		{
			InitNormalWidgetSize();
			InitSelectWidgetSize();
		}
	}

	private void InitNormalWidgetSize()
	{
		if (!(NormalWidget == null))
		{
			Vector2 localSize = NormalWidget.localSize;
			NormalWidget.SetDimensions((int)localSize.x - 30, (int)localSize.y);
		}
	}

	private void InitSelectWidgetSize()
	{
		if (!(SelectWidget == null))
		{
			Vector2 localSize = SelectWidget.localSize;
			SelectWidget.SetDimensions((int)localSize.x - 30, (int)localSize.y);
		}
	}

	private void InitToggle()
	{
		_toggle = GetComponent<UIToggle>();
		if (!(_toggle == null))
		{
			_toggle.startsActive = false;
			int num = 47;
			if (Index == 0)
			{
				num++;
			}
			_toggle.group = ((Index == 0) ? num : (num + 1));
		}
	}

	private void InitListener()
	{
		ExtendMenu parent = GetParent();
		if (!(parent == null))
		{
			_iListener = parent;
		}
	}

	private void InitIndicator()
	{
		EnableNormalIndicator(isEnable: false);
		EnableExtendIndicator(isEnable: false);
	}

	public ExtendMenu GetParent()
	{
		Transform parent = base.transform.parent;
		if (parent == null)
		{
			return null;
		}
		return parent.GetComponent<ExtendMenu>();
	}

	public void SetLabel()
	{
		if (!(Label == null))
		{
			UIHelper.SetLabelText(Label, LabelText);
			if (!(SelectLabel == null))
			{
				UIHelper.SetLabelText(SelectLabel, LabelText);
			}
		}
	}

	public void SetLabelColor()
	{
	}

	public void EnableNormalIndicator()
	{
		EnableNormalIndicator(isEnable: true);
		EnableExtendIndicator(isEnable: false);
	}

	public void EnableExtendIndicator()
	{
		EnableExtendIndicator(isEnable: true);
		EnableNormalIndicator(isEnable: false);
	}

	private void EnableNormalIndicator(bool isEnable)
	{
		if (!(NormalIndicatorSprite == null))
		{
			UIHelper.EnableGameObject(NormalIndicatorSprite, isEnable);
		}
	}

	private void EnableExtendIndicator(bool isEnable)
	{
		if (!(ExtendIndicatorSprite == null))
		{
			UIHelper.EnableGameObject(ExtendIndicatorSprite, isEnable);
		}
	}

	public void SelectMenu(bool isSelect)
	{
		if (!(_toggle == null))
		{
			_toggle.Set(isSelect);
		}
	}

	public void SetEnableNew(bool isEnable)
	{
		if (!(NewSprite == null))
		{
			IsNew = isEnable;
			UIHelper.EnableGameObject(NewSprite, isEnable);
		}
	}

	public void SetEnableEvent(bool isEnable)
	{
		if (!(EventSprite == null))
		{
			IsEvent = isEnable;
			UIHelper.EnableGameObject(EventSprite, isEnable);
		}
	}

	public void SetEnableBooster(bool isEnable)
	{
		if (!(BoosterSprite == null))
		{
			IsBooster = isEnable;
			UIHelper.EnableGameObject(BoosterSprite, isEnable);
		}
	}

	public void OnClick()
	{
		if (_iListener != null)
		{
			_iListener.Click(this);
		}
	}
}

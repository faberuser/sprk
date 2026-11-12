using NGame2.NUI.NComponent2.NItemIconSub;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class StaminaIconComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClick(StaminaIconComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Icon/StaminaIconComponent";

	public static PrefabAuto<StaminaIconComponent> PrefabAuto = new PrefabAuto<StaminaIconComponent>("Prefab2/Component/Icon/StaminaIconComponent");

	public UIWidget WidgetThis;

	public UIWidget RootWidget;

	public UIWidget IconRoot;

	public UILabel AmountLabel;

	public UILabel AmountUnderLabel;

	public IconProxy IconProxy;

	public Transform SelectRoot;

	public UISpriteSelector SelectSprite;

	public StaminaTooltipComponent staminaTooltipFunc;

	private Listener _listener;

	private StaminaType _staminaType;

	private ProxyContainer _subProxyContainer = new ProxyContainer();

	public StaminaType StaminaType => _staminaType;

	public int _staminaAmount { get; private set; }

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		RootWidget.alpha = 1f;
		RootWidget.transform.localScale = Vector3.one;
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale: false);
		SetAmountColor(Color.white);
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
		_subProxyContainer.OnRelease();
	}

	public void Open(StaminaType staminaType, int staminaAmount, Listener listener)
	{
		_listener = listener;
		_staminaType = staminaType;
		_staminaAmount = staminaAmount;
		staminaTooltipFunc.StaminaType = staminaType;
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		IconProxy.SetupStamina(staminaType);
		UIHelper.EnableGameObject(AmountLabel, _staminaAmount > 0);
		UIHelper.SetLabelText(AmountLabel, staminaAmount);
		SetSelected(selected: false);
		SetUnderLabel(string.Empty);
	}

	public void SetSelected(bool selected, SelectSubComponent.ESelectType selectType = SelectSubComponent.ESelectType.Select_Checked)
	{
		UIHelper.EnableGameObject(SelectRoot, selected);
		if (SelectSprite != null)
		{
			SelectSprite.Select(selectType.ToString());
		}
	}

	public void SetUnderLabel(string underText)
	{
		UIHelper.EnableGameObject(AmountUnderLabel, underText != string.Empty);
		UIHelper.SetLabelText(AmountUnderLabel, underText);
	}

	public void SetMinMaxCount(int min, int max)
	{
		string text = string.Empty;
		if (max > 0 && max != min)
		{
			text = $"{min}-{max}";
		}
		UIHelper.EnableGameObject(AmountLabel, _staminaAmount > 0);
		UIHelper.SetLabelText(AmountLabel, text);
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void OnShowTooltip()
	{
		if (!(staminaTooltipFunc == null))
		{
			staminaTooltipFunc.OnShowTooltip(_staminaType);
		}
	}

	public void OnHideTooltip()
	{
		if (!(staminaTooltipFunc == null))
		{
			staminaTooltipFunc.OnHideTooltip();
		}
	}

	public void SetAmountColor(Color color)
	{
		UIHelper.SetLabelColor(AmountLabel, color);
	}

	public void SetIconGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale);
	}
}

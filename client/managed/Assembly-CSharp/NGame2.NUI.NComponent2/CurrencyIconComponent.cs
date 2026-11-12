using System.Collections;
using NGame2.NGameSetting.NUi;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NComponent2.NItemIconSub;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CurrencyIconComponent : UIScrollItemBehaviour, InitOnCreate, IIconComponent
{
	public interface Listener
	{
		void OnClick(CurrencyIconComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Icon/CurrencyIconComponent";

	public const string PrefabPath2 = "Prefab2/Component/Icon/CurrencyIconComponent2";

	public const string PrefabPath3 = "Prefab2/Component/Icon/FirstClearIconComponent";

	public static PrefabAuto<CurrencyIconComponent> PrefabAuto = new PrefabAuto<CurrencyIconComponent>("Prefab2/Component/Icon/CurrencyIconComponent");

	public static PrefabAuto<CurrencyIconComponent> PrefabAuto2 = new PrefabAuto<CurrencyIconComponent>("Prefab2/Component/Icon/CurrencyIconComponent2");

	public static PrefabAuto<CurrencyIconComponent> PrefabAuto3 = new PrefabAuto<CurrencyIconComponent>("Prefab2/Component/Icon/FirstClearIconComponent");

	public UIWidget WidgetThis;

	public UIWidget RootWidget;

	public UIWidget IconRoot;

	public UILabel AmountLabel;

	public UILabel AmountUnderLabel;

	public UIGame3DWidget UiEffect_GetItem;

	public UIGame3DWidget UiEffect_GetGold;

	public UIGame3DWidget UiEffect_GetGem;

	public UIGame3DWidget UiEffect_IncreaseGold;

	public UILabel Label_FirstClear;

	public UILabel Label_Booster;

	public UITexture Texture_Booster;

	public UISprite SpriteEventBonus;

	public UISprite SpriteFrame;

	public IconProxy IconProxy;

	public Transform SelectRoot;

	public UISpriteSelector SelectSprite;

	public CurrencyTooltipComponent currencyTooltipFunc;

	private Listener _listener;

	private CurrencyType _currencyType;

	private ProxyContainer _subProxyContainer = new ProxyContainer();

	private NGame2.NUI.NComponent2.NHelper.GetItemAnimationData _itemAnimation;

	private Coroutine _increaseGoldCo;

	private bool _skipIncrease;

	public CurrencyType currencyType => _currencyType;

	public long _currencyAmount { get; private set; }

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (_itemAnimation != null)
		{
			_itemAnimation.Close();
			_itemAnimation = null;
		}
		if (_increaseGoldCo != null)
		{
			VespaCoroutine.Stop(this, _increaseGoldCo);
			_increaseGoldCo = null;
			UiEffect_IncreaseGold.Stop(immediatelyClear: true);
		}
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

	public void Open(CurrencyType currencyType, long currencyAmount, Listener listener)
	{
		_listener = listener;
		_currencyType = currencyType;
		_currencyAmount = currencyAmount;
		currencyTooltipFunc.CurrencyType = currencyType;
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		IconProxy.SetupCurrency(currencyType);
		UILabel amountLabel = AmountLabel;
		long currencyAmount2 = _currencyAmount;
		int num = 0;
		UIHelper.EnableGameObject(amountLabel, currencyAmount2 > num);
		UIHelper.SetLabelText(AmountLabel, currencyAmount);
		ShowFirstClear(show: false);
		string code = null;
		int num2 = 0;
		ShowGoldBooster(boostApplied: false, num2, code);
		SetSelected(selected: false);
		ShowEventBonus(isShow: false);
		SetSkipIncrease(skip: false);
		SetUnderLabel(string.Empty);
	}

	private void Update()
	{
		if (_itemAnimation != null)
		{
			_itemAnimation.ChainUpdate();
		}
		if (base.gameObject.activeInHierarchy && _currencyType != CurrencyType.None && _itemAnimation != null && _increaseGoldCo == null)
		{
			_increaseGoldCo = VespaCoroutine.Start(this, coIncreaseGold());
		}
	}

	public NGame2.NUI.NComponent2.NHelper.GetItemAnimationData CreateGetAnimation()
	{
		if (_itemAnimation != null)
		{
			_itemAnimation.Close();
			_itemAnimation = null;
		}
		if (_increaseGoldCo != null)
		{
			VespaCoroutine.Stop(this, _increaseGoldCo);
			UiEffect_IncreaseGold.Stop(immediatelyClear: true);
		}
		NGame2.NGameSetting.NUi.GetItemAnimationData getItemAnimation = GameSetting.data.Ui.GetItemAnimation;
		UIGame3DWidget effect = ((_currencyType == CurrencyType.Gold) ? UiEffect_GetGold : ((_currencyType != CurrencyType.Gem) ? UiEffect_GetItem : UiEffect_GetGem));
		_itemAnimation = new NGame2.NUI.NComponent2.NHelper.GetItemAnimationData();
		_itemAnimation.Open(null, RootWidget, effect, getItemAnimation);
		return _itemAnimation;
	}

	public void SetSelected(bool selected, SelectSubComponent.ESelectType selectType = SelectSubComponent.ESelectType.Select_Checked)
	{
		UIHelper.EnableGameObject(SelectRoot, selected);
		if (SelectSprite != null)
		{
			SelectSprite.Select(selectType.ToString());
		}
	}

	public void ShowFirstClear(bool show)
	{
		UIHelper.EnableGameObject(Label_FirstClear, show);
	}

	public void SetUnderLabel(string underText)
	{
		UIHelper.EnableGameObject(AmountUnderLabel, underText != string.Empty);
		UIHelper.SetLabelText(AmountUnderLabel, underText);
	}

	public void ShowGoldBooster(bool boostApplied, long value, string code)
	{
		UIHelper.EnableGameObject(Label_Booster, boostApplied);
		UIHelper.EnableGameObject(Texture_Booster, boostApplied);
		UIHelper.SetLabelText(Label_Booster, "+" + value + "%");
		if (boostApplied && code != null)
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Booster, UIHelper.GetBoosterIcon(code));
		}
	}

	public void ShowCountBooster(bool boostApplied, long value, string code)
	{
		UIHelper.EnableGameObject(Label_Booster, boostApplied);
		UIHelper.EnableGameObject(Texture_Booster, boostApplied);
		UIHelper.SetLabelText(Label_Booster, "+" + value);
		if (boostApplied && code != null)
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Booster, UIHelper.GetBoosterIcon(code));
		}
	}

	public void ShowEventBonus(bool isShow)
	{
		UIHelper.EnableGameObject(SpriteEventBonus, isShow);
	}

	public void SetMinMaxCount(int min, int max)
	{
		string text = string.Empty;
		if (max > 0 && max != min)
		{
			text = $"{min}-{max}";
		}
		UILabel amountLabel = AmountLabel;
		long currencyAmount = _currencyAmount;
		int num = 0;
		UIHelper.EnableGameObject(amountLabel, currencyAmount > num);
		UIHelper.SetLabelText(AmountLabel, text);
	}

	private IEnumerator coIncreaseGold()
	{
		do
		{
			yield return null;
		}
		while (_itemAnimation != null && !_itemAnimation.CanNext);
		if (_skipIncrease)
		{
			UIHelper.SetLabelText(AmountLabel, _currencyAmount);
			yield break;
		}
		UiEffect_IncreaseGold.Play(null);
		float elapsedTime = 0f;
		float timeLength = GameSetting.data.Ui.CoinIncreaseTime;
		do
		{
			yield return null;
			elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			float t = Mathf.Clamp01(elapsedTime / timeLength);
			int num = Mathf.RoundToInt(Mathf.Lerp(0f, _currencyAmount, t));
			UIHelper.SetLabelText(AmountLabel, num);
		}
		while (!(elapsedTime >= timeLength));
		UIHelper.SetLabelText(AmountLabel, _currencyAmount);
		UiEffect_IncreaseGold.Stop(immediatelyClear: true);
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
		if ((_itemAnimation == null || _itemAnimation.CanNext) && !(currencyTooltipFunc == null))
		{
			currencyTooltipFunc.OnShowTooltip();
		}
	}

	public void OnHideTooltip()
	{
		if (!(currencyTooltipFunc == null))
		{
			currencyTooltipFunc.OnHideTooltip();
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

	public void SetSkipIncrease(bool skip)
	{
		_skipIncrease = skip;
	}
}

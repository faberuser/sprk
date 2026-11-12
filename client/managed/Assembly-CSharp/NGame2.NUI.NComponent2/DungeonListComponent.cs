using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NEntryMenu.NCondition;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public sealed class DungeonListComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickEnter(PurchaseDungeonData purchaseDungeonData);

		void OnClickBuy(PurchaseDungeonData purchaseDungeonData);
	}

	public const string PrefabPath = "Prefab2/Component/ParttimeDungeon/DungeonListComponent";

	public static PrefabAuto<DungeonListComponent> PrefabAuto = new PrefabAuto<DungeonListComponent>("Prefab2/Component/ParttimeDungeon/DungeonListComponent");

	public UIButton BuyButton;

	public UIButton EnterButton;

	public UIBundleLoader PortraitBundleTex;

	public UIBundleLoader RewardBundleTex;

	public UILabel NameLabel;

	public UILabel RemainTimeLabel;

	public Transform BuyRoot;

	public UILabel PriceLabel;

	public UIWidget IconProxyRoot;

	private IconProxy IconProxy;

	private Listener _listener;

	public PurchaseDungeonData PurchaseDungeonData { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	public void Open(Listener listener, PurchaseDungeonData data)
	{
		_listener = listener;
		PurchaseDungeonData = data;
		Init();
	}

	public void Init()
	{
		SetDungeonName();
		SetDungeonPortrait();
		SetCurrenyIcon();
		SetPriceLabel();
		SetBuyButton();
		SetEnterButton();
		SerRewardItemIcon();
	}

	private void SetDungeonName()
	{
		string stringDirect = LocalizationManager.GetStringDirect(PurchaseDungeonData.DungeonName);
		UIHelper.SetLabelText(NameLabel, stringDirect);
	}

	private void SetDungeonPortrait()
	{
		UIHelper.SetBundleTexture(PortraitBundleTex, PurchaseDungeonData.DungeonPortraitPath);
	}

	private void SetCurrenyIcon()
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconProxyRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(PurchaseDungeonData.CurrencyType);
		}
	}

	private void SetPriceLabel()
	{
		int price = ParttimeDungeonManagement.GetPrice(PurchaseDungeonData.Price);
		string remainTimeTextAllDelZero = UIHelper.GetRemainTimeTextAllDelZero(GetBoosterDurationSec(PurchaseDungeonData));
		string text = PurchaseDungeonData.CurrencyType.ToString();
		string stringDirect = LocalizationManager.GetStringDirect("SIDEQUEST_PURCHASE_TIME_" + text.ToUpper(), price, remainTimeTextAllDelZero);
		UIHelper.SetLabelText(PriceLabel, stringDirect);
	}

	private int GetBoosterDurationSec(PurchaseDungeonData purchaseDungeonData)
	{
		return GameRoot.Instance.BoosterItemDataContainer.GetData(purchaseDungeonData.BoosterItemCode)?.Duration ?? 0;
	}

	private void SetBuyButton()
	{
		bool enable = CanBuy();
		UIHelper.EnableGameObject(BuyRoot, enable);
		UIHelper.EnableButton(BuyButton, enable);
	}

	private bool CanBuy()
	{
		return !CanEnter();
	}

	private void SetEnterButton()
	{
		UIHelper.EnableButton(EnterButton, CanEnter());
	}

	private bool CanEnter()
	{
		if (GetBoosterRemainTime() > 0)
		{
			return true;
		}
		return GetEventRemainTime() > 0;
	}

	private void SerRewardItemIcon()
	{
		UIHelper.SetBundleTexture(RewardBundleTex, PurchaseDungeonData.DungeonRewardPath);
	}

	public void UpdateUI()
	{
		SetEnterButton();
		SetBuyButton();
		SetRemainTime();
	}

	private void SetRemainTime()
	{
		int boosterRemainTime = GetBoosterRemainTime();
		SetRemainTime((boosterRemainTime > 0) ? boosterRemainTime : GetEventRemainTime());
	}

	private int GetEventRemainTime()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PurchaseDungeonData.EnterCondition))
		{
			return 0;
		}
		DropBonusEvent effectiveEvent = EventExist.GetEffectiveEvent(PurchaseDungeonData.EnterCondition, 0);
		if (effectiveEvent == null)
		{
			return 0;
		}
		return GetRemainEventTime(effectiveEvent);
	}

	private int GetRemainEventTime(DropBonusEvent dropBonusEvent)
	{
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(dropBonusEvent.EndTime, out var o_dateTime))
		{
			return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
		}
		return 0;
	}

	private int GetBoosterRemainTime()
	{
		BoosterItemData data = GameRoot.Instance.BoosterItemDataContainer.GetData(PurchaseDungeonData.BoosterItemCode);
		if (data == null)
		{
			return 0;
		}
		PlayerItemTimeDurationInfo playerItemTimeDurationInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Get(data.BoosterItemIndex);
		if (playerItemTimeDurationInfo == null)
		{
			return 0;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(playerItemTimeDurationInfo.EndTime, out var o_dateTime))
		{
			return 0;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return Math.Max((int)(o_dateTime - currentServerUTCTime).TotalSeconds, 0);
	}

	private void SetRemainTime(int seconds)
	{
		UIHelper.EnableGameObject(RemainTimeLabel, seconds > 0);
		if (seconds != 0)
		{
			string remainTimeLabelText = UIHelper.GetRemainTimeLabelText(seconds);
			UIHelper.SetLabelText(RemainTimeLabel, remainTimeLabelText);
		}
	}

	private void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(PurchaseDungeonData);
		}
	}

	public void OnClickBuy()
	{
		if (_listener != null)
		{
			_listener.OnClickBuy(PurchaseDungeonData);
		}
	}
}

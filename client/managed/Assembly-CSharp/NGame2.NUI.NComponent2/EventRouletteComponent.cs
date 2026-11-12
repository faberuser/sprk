using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EventRouletteComponent : UIScrollItemBehaviour, ItemIconComponent.Listener, NVespa.NPrefab2.InitOnCreate
{
	public new interface IListener
	{
		void OnClickEnter(EventRouletteComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/EventCraft/EventRouletteComponent";

	public static PrefabAuto<EventRouletteComponent> PrefabAuto = new PrefabAuto<EventRouletteComponent>("Prefab2/Component/EventCraft/EventRouletteComponent");

	public Transform ItemIconRoot;

	public UILabel NameLabel;

	public UILabel MyCountLabel;

	public UILabel CountLabel;

	public Transform SelectRoot;

	private CurrencyIconComponent _currencyIconComponent;

	private ItemIconComponent _itemIconComp;

	private IListener _listener;

	private long _myCount;

	private long _curCount;

	private int _multyValue;

	public EventRouletteData EventRouletteData { get; private set; }

	public CurrencyTypeData CurrencyData { get; private set; }

	public ItemData ItemData { get; private set; }

	public bool CanChange => _myCount >= _curCount;

	private string GetKCount(long value)
	{
		string empty = string.Empty;
		int num = 1000000000;
		if (value >= num)
		{
			value /= 1000;
			return Vespa_Long_Extension_Method.ToCommaString(value) + "K";
		}
		return Vespa_Long_Extension_Method.ToCommaString(value);
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		SetSelected(selected: false);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		EventRouletteData = null;
		CurrencyData = null;
		ItemData = null;
		_myCount = 0L;
		_curCount = 0L;
		_multyValue = 0;
		Release();
		SetSelected(selected: false);
	}

	public void Open(EventRouletteData rouletteData, IListener listener)
	{
		EventRouletteData = rouletteData;
		if (EventRouletteData != null)
		{
			_listener = listener;
			Init();
		}
	}

	private void Init()
	{
		switch (EventRouletteData.Type)
		{
		case EventRouletteType.Currency:
			_multyValue = EventRouletteData.CurrencyValue;
			InitCurrency();
			break;
		case EventRouletteType.Item:
			_multyValue = EventRouletteData.ItemCount;
			InitItem();
			break;
		}
		int num = 1;
		SetNumber(num);
	}

	private void SetMyCount(long myCount)
	{
		_myCount = myCount;
		string kCount = GetKCount(myCount);
		UIHelper.SetLabelText(MyCountLabel, kCount);
		Color color = ((_myCount >= _multyValue) ? Color.white : Color.red);
		UIHelper.SetLabelColor(MyCountLabel, color);
	}

	public void SetNumber(long num)
	{
		int multyValue = _multyValue;
		long num2 = checked(num * multyValue);
		_curCount = num2;
		string kCount = GetKCount(num2);
		UIHelper.SetLabelText(CountLabel, kCount);
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SelectRoot, selected);
	}

	private void InitCurrency()
	{
		CurrencyData = GameRoot.Instance.CurrencyTypeDataContainer.GetData(EventRouletteData.CurrencyType);
		if (CurrencyData != null)
		{
			SetCurrencyName(CurrencyData);
			SetCurrencyIcon(CurrencyData);
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(CurrencyData.UserStatType);
			SetMyCount(userStat);
		}
	}

	private void SetCurrencyName(CurrencyTypeData currencyTypeData)
	{
		string stringDirect = LocalizationManager.GetStringDirect($"TOOLTIP_INFO_NAME_{currencyTypeData.CurrencyType.ToString().ToUpper()}");
		UIHelper.SetLabelText(NameLabel, stringDirect);
	}

	private void SetCurrencyIcon(CurrencyTypeData currencyTypeData)
	{
		_currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		if (!(_currencyIconComponent == null))
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_currencyIconComponent.transform);
			CurrencyIconComponent currencyIconComponent = _currencyIconComponent;
			CurrencyType currencyType = currencyTypeData.CurrencyType;
			CurrencyIconComponent.Listener listener = null;
			int num = 0;
			currencyIconComponent.Open(currencyType, num, listener);
		}
	}

	private void InitItem()
	{
		ItemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(EventRouletteData.ItemCode);
		if (ItemData != null)
		{
			SetItemName(ItemData);
			SetItemIcon(ItemData);
			int num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index)?.Count ?? 0;
			SetMyCount(num);
		}
	}

	private void SetItemName(ItemData itemData)
	{
		string text = itemData.Name;
		UIHelper.SetLabelText(NameLabel, text);
	}

	private void SetItemIcon(ItemData itemData)
	{
		_itemIconComp = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		if (!(_itemIconComp == null))
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComp.transform);
			_itemIconComp.Open(itemData, this);
		}
	}

	public void Refresh()
	{
		switch (EventRouletteData.Type)
		{
		case EventRouletteType.Currency:
			if (CurrencyData != null)
			{
				long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(CurrencyData.UserStatType);
				SetMyCount(userStat);
			}
			break;
		case EventRouletteType.Item:
			if (ItemData != null)
			{
				int num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index)?.Count ?? 0;
				SetMyCount(num);
			}
			break;
		}
	}

	private void Release()
	{
		FactoryBase.SafeReleaseComponent(ref _currencyIconComponent);
		FactoryBase.SafeReleaseComponent(ref _itemIconComp);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent component)
	{
	}
}

using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MultiCenterMessages : BaseWindow
{
	public class MessageOrder
	{
		public Transform trans;

		public int score;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MultiCenterMessages";

	public float DelayTime;

	public UITable MessageTable;

	public UIWidget RootWidget;

	public Transform EclipseKeyGaugeRoot;

	private EclipseKeyGaugeComponent _eclipseKeyGauge;

	private float _curDelayTime;

	private List<Component> _ListComponents = new List<Component>();

	private int _messageScore;

	private bool _ExecutedCreatedDrawCall;

	private List<MessageOrder> _ListMessgeOrder = new List<MessageOrder>();

	public new static MultiCenterMessages Open()
	{
		CenterMessage centerMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<CenterMessage>();
		if (centerMessage != null)
		{
			centerMessage.Close2(immediately: true);
		}
		IconCenterMessage iconCenterMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<IconCenterMessage>();
		if (iconCenterMessage != null)
		{
			iconCenterMessage.Close2(immediately: true);
		}
		WindowPrefab<MultiCenterMessages> windowPrefab = new WindowPrefab<MultiCenterMessages>("Resources/Prefab2/Window/MessagePopup/MultiCenterMessages", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListComponents);
		_ListMessgeOrder.Clear();
		_messageScore = 0;
	}

	private IconCenterMessageComponent CreateIconCenterMessageComponent()
	{
		IconCenterMessageComponent iconCenterMessageComponent = IconCenterMessageComponent.PrefabAuto.Factory.Create(MessageTable.transform);
		if (iconCenterMessageComponent == null)
		{
			return null;
		}
		_ListComponents.Add(iconCenterMessageComponent);
		return iconCenterMessageComponent;
	}

	private void OrderMessage(Transform transform)
	{
		MessageOrder item = new MessageOrder
		{
			trans = transform,
			score = ++_messageScore
		};
		_ListMessgeOrder.Add(item);
	}

	public void AddIconMessage(string message, string iconName)
	{
		Reset();
		IconCenterMessageComponent iconCenterMessageComponent = CreateIconCenterMessageComponent();
		if (!(iconCenterMessageComponent == null))
		{
			iconCenterMessageComponent.Open(message, iconName);
			OrderMessage(iconCenterMessageComponent.transform);
		}
	}

	public void AddClassBuffIconMessage(string message, string iconName)
	{
		IconCenterMessageComponent iconCenterMessageComponent = CreateIconCenterMessageComponent();
		if (!(iconCenterMessageComponent == null))
		{
			iconCenterMessageComponent.Open(message, iconName, IconProxyType.ClassBuff);
			OrderMessage(iconCenterMessageComponent.transform);
		}
	}

	public void AddMessage(string message)
	{
		CenterMessageComponent centerMessageComponent = CenterMessageComponent.PrefabAuto.Factory.Create(MessageTable.transform);
		if (centerMessageComponent != null)
		{
			centerMessageComponent.Open(message);
		}
		_ListComponents.Add(centerMessageComponent);
		OrderMessage(centerMessageComponent.transform);
	}

	private void CalcBounds()
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = MessageTable.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		RootWidget.width = (int)bounds.size.x;
		RootWidget.height = (int)bounds.size.y;
		RootWidget.ForceUpdateAnchors(includeChildren: true);
	}

	public void RepositionMessage()
	{
		if (!_ExecutedCreatedDrawCall)
		{
			NGUITools.ImmediatelyCreateDrawCalls(base.gameObject);
		}
		CalcBounds();
		CalcBounds();
		MessageTable.Reposition();
		if (!_ExecutedCreatedDrawCall)
		{
			NGUITools.ImmediatelyCreateDrawCalls(base.gameObject);
		}
		_ExecutedCreatedDrawCall = true;
	}

	private void OpenInternal()
	{
		base.Open();
		if (MessageTable.onCustomSort == null)
		{
			MessageTable.onCustomSort = delegate(Transform a, Transform b)
			{
				MessageOrder messageOrder = _ListMessgeOrder.Find((MessageOrder order) => Vespa_Common_Class_Extension_Method.EQUALS(order.trans, a));
				MessageOrder messageOrder2 = _ListMessgeOrder.Find((MessageOrder order) => Vespa_Common_Class_Extension_Method.EQUALS(order.trans, b));
				return (messageOrder != null && messageOrder2 != null) ? messageOrder.score.CompareTo(messageOrder2.score) : 0;
			};
		}
		_curDelayTime = 0f;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen())
		{
			_curDelayTime += Time.deltaTime;
			if (_curDelayTime > DelayTime)
			{
				Close();
				_curDelayTime = 0f;
			}
		}
	}

	public override void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _eclipseKeyGauge);
		base.Close();
	}

	public void ShowEclipseGauge()
	{
		if (EclipseKeyGaugeComponent.ShowEclipseKeyGauge())
		{
			_eclipseKeyGauge = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(EclipseKeyGaugeRoot);
			long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
			CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
			currencyResultInfo.CurrencyType = CurrencyType.EclipseKey;
			currencyResultInfo.AddValue = 0L;
			currencyResultInfo.NewValue = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.EclipsePoint);
			_eclipseKeyGauge.Init(stamina, currencyResultInfo);
		}
	}
}

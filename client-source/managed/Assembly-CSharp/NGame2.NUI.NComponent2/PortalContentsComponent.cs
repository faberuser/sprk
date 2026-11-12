using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NGuild;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NComponent2.NPortal;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NShop;
using NGame2.NUI.NWindow.NTower;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.GetGuildArenaSessionInfo;
using NShared.NLocalization;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public abstract class PortalContentsComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class BasicData
	{
		public UIPanel RootPanel;

		public UITexture ContentsTexture;

		public UILabel ContentsTitleLabel;

		public UILabel ContentsDescriptionLabel;

		[Header("[Contents ResetTime Group]")]
		public List<Transform> ContentsResetTimeGroups;

		public List<UILabel> ContentsResetTimeLabels;

		[Header("[Contents Currency Group]")]
		public Transform CurrencyGroup;

		public UILabel CurrencyLabel;

		public UISprite CurrencyLabelBg;

		public UIWidget CurrencyIconRoot;

		public UIWidget ItemIconRoot;

		public UILabel CurrencyRechargeTimeLabel;

		[Header("[Contents Locked State Group]")]
		public Transform LockedStateGroup;

		public UILabel LockedReasonLabel;

		public bool LockedStatePass;

		[Header("[Contents Notice State Group]")]
		public Transform NoticeStateIconRoot;

		public UISpriteSelector StateIconSelector;

		[Header("[Evnet Group]")]
		public Transform EventBuffRoot;

		[Header("[Contents State Icon]")]
		public UITable ContentsStateIconTable;

		public Transform Icon_Global;

		public Transform Icon_Multi;

		[Header("[Resolution]")]
		public UIWidget ResolutionRoot;

		public float LimitWidth;

		[Header("[Color Template]")]
		public Color UnlockedTitleColor = new Color32(155, 120, 70, byte.MaxValue);

		public Color LockedTitleColor = new Color32(195, 195, 195, byte.MaxValue);
	}

	public class RechargeData
	{
		private PortalContentsComponent portalContentsComponent;

		private UILabel targetLabel;

		public StaminaType _staminaType { get; private set; }

		public long _currAmount { get; private set; }

		public long _maxAmount { get; private set; }

		public int _rechargeCount { get; private set; }

		public DateTime _nextRechargeTime { get; private set; }

		public StaminaUpdateType _staminaUpdateType { get; private set; }

		public bool QueueUpdate { get; private set; }

		public RechargeData(PortalContentsComponent comp, StaminaType staminaType, long currAmount, long maxAmount, int rechageCount, DateTime nextRechargeTime, StaminaUpdateType staminaUpdateType)
		{
			portalContentsComponent = comp;
			targetLabel = comp._basicData.CurrencyRechargeTimeLabel;
			UIHelper.EnableGameObject(comp._basicData.CurrencyGroup, enable: true);
			_staminaType = staminaType;
			_currAmount = currAmount;
			_maxAmount = maxAmount;
			_rechargeCount = rechageCount;
			_nextRechargeTime = nextRechargeTime;
			_staminaUpdateType = staminaUpdateType;
		}

		public void Update()
		{
			if (QueueUpdate)
			{
				portalContentsComponent.SetCurrencyLabel();
			}
			else
			{
				SetLabelRemainRechargeTime(targetLabel);
			}
		}

		public void SetLabelRemainRechargeTime(UILabel targetLabel)
		{
			if (_staminaUpdateType == StaminaUpdateType.Regen && _currAmount >= _maxAmount)
			{
				string stringDirect = LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_RECHARGE_TIME", _rechargeCount, "-");
				UIHelper.SetLabelText(targetLabel, stringDirect);
				return;
			}
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			TimeSpan ts = _nextRechargeTime - currentServerUTCTime;
			QueueUpdate = ts.TotalSeconds < 0.0;
			string remainTimeLabelText = UIHelper.GetRemainTimeLabelText(ts);
			string text = string.Empty;
			if (_staminaUpdateType == StaminaUpdateType.Regen)
			{
				text = LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_RECHARGE_TIME", _rechargeCount, remainTimeLabelText);
			}
			else if (_staminaUpdateType == StaminaUpdateType.Reset)
			{
				text = LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_ALL_REGEN_TIME", remainTimeLabelText);
			}
			UIHelper.SetLabelText(targetLabel, text);
		}
	}

	public class ContentsResetData
	{
		private PortalContentsComponent portalContentsComponent;

		private UILabel targetLabel;

		private string _frontMsgKey;

		private DateTime _nextResetTime;

		private object _localParam;

		private bool _needUpdate;

		private bool _isUTC;

		public bool QueueUpdate { get; private set; }

		public ContentsResetData(PortalContentsComponent comp, string frontMsgKey, DateTime nextResetTime, object localParam = null, bool needUpdate = true, Transform targetTimeGroup = null, UILabel timeLabel = null, bool isUTC = true)
		{
			portalContentsComponent = comp;
			UIHelper.EnableGameObject((targetTimeGroup == null) ? comp.GetResetTimeGroup(0) : targetTimeGroup, enable: true);
			targetLabel = ((timeLabel == null) ? (targetLabel = comp.GetResetTimeLabel(0)) : timeLabel);
			_frontMsgKey = frontMsgKey;
			_nextResetTime = nextResetTime;
			_localParam = localParam;
			_needUpdate = needUpdate;
			_isUTC = isUTC;
		}

		public void Update()
		{
			if (QueueUpdate && portalContentsComponent != null)
			{
				portalContentsComponent.CheckContentResetTime();
			}
			else
			{
				SetLabelRemainResetTime(targetLabel);
			}
		}

		public void SetLabelRemainResetTime(UILabel target)
		{
			DateTime dateTime = ((!_isUTC) ? GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() : GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime());
			TimeSpan ts = _nextResetTime - dateTime;
			if (_needUpdate)
			{
				QueueUpdate = ts.TotalSeconds < 0.0;
			}
			string text = ((ts.TotalSeconds > 0.0) ? UIHelper.GetRemainTimeLabelText(ts) : "-");
			string text2 = ((_localParam != null) ? LocalizationManager.GetStringDirect(_frontMsgKey, Vespa_Common_Class_Extension_Method.TOSTRING(_localParam), text) : LocalizationManager.GetStringDirect(_frontMsgKey, text));
			UIHelper.SetLabelText(target, text2);
		}
	}

	[Serializable]
	public class AutoAdjustBackground
	{
		public UILabel TargetLabel;

		public UIWidget Background;

		public int Interval;

		private int prevWidth;

		public void Update()
		{
			if (!(TargetLabel == null) && !(Background == null))
			{
				int num = (int)TargetLabel.printedSize.x;
				if (prevWidth != num)
				{
					int width = num + Interval;
					Background.width = width;
					prevWidth = width;
				}
			}
		}
	}

	public interface Listener
	{
		void OnClickContentsButton(PortalRenewalData portalRenewalData);

		void ChangeContentsTitle(string title);

		void MoveNode(int chapterIndex, int dungeonIndex);

		bool IsRunningEditMode();
	}

	public BasicData _basicData;

	public List<AutoAdjustBackground> _autoAdjustBackgrounds = new List<AutoAdjustBackground>();

	protected RechargeData _rechargeData;

	protected List<ContentsResetData> _contentsResetDatas = new List<ContentsResetData>();

	private IconProxy _iconProxy;

	private ItemIconComponent _itemIconComponent;

	private Component _plainAssetTextureRoot;

	private List<PortalContentsStateIconComponent> _contentsStateIconComponentList = new List<PortalContentsStateIconComponent>();

	protected Listener _listener;

	public PortalRenewalData _portalRenewalData { get; protected set; }

	public PortalRenewal.LockedState LockedState { get; protected set; }

	public PortalRenewal.NoticeState NoticeState { get; protected set; }

	public PortalRenewalCategory Categry
	{
		get
		{
			if (_portalRenewalData == null)
			{
				return PortalRenewalCategory.NONE;
			}
			return _portalRenewalData.Category;
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(PortalRenewalData portalRenewData, Listener listener = null)
	{
		_portalRenewalData = portalRenewData;
		_listener = listener;
		_rechargeData = null;
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_contentsResetDatas);
		_plainAssetTextureRoot = this;
		_basicData.ContentsResetTimeGroups.ForEach(delegate(Transform group)
		{
			UIHelper.EnableGameObject(group, enable: false);
		});
		UIHelper.EnableGameObject(_basicData.CurrencyRechargeTimeLabel, enable: false);
		UIHelper.EnableGameObject(_basicData.EventBuffRoot, enable: false);
		Init();
	}

	private void Update()
	{
		UpdateDeliver();
	}

	public void UpdateComponent()
	{
		UpdateDeliver();
	}

	protected virtual void UpdateDeliver()
	{
		if (_rechargeData != null)
		{
			_rechargeData.Update();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_contentsResetDatas))
		{
			_contentsResetDatas.ForEach(delegate(ContentsResetData data)
			{
				data.Update();
			});
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_autoAdjustBackgrounds))
		{
			_autoAdjustBackgrounds.ForEach(delegate(AutoAdjustBackground data)
			{
				data.Update();
			});
		}
	}

	protected virtual void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.ReleaseAndClearList(_contentsStateIconComponentList);
		_rechargeData = null;
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_contentsResetDatas);
		LockedState = PortalRenewal.LockedState.NONE;
		NoticeState = PortalRenewal.NoticeState.NONE;
	}

	protected virtual void Init()
	{
		SetTitle();
		SetBackgroundTexture();
		CheckLockedState();
		SetCurrency();
		SetDescription();
		SetContentsResetTime();
		CheckEventBuffCondition();
		UpdateContentsNotice();
		UpdateContentsStateIcon();
		UpdateContentsStateIconTable();
	}

	private void SetTitle()
	{
		if (_portalRenewalData.SubCategory != PortalRenewalSubCategory.MENUBUTTON)
		{
			base.transform.name = _portalRenewalData.SubCategory.ToString();
		}
		else
		{
			base.transform.name = _portalRenewalData.Category.ToString();
		}
		UIHelper.SetLabelText(_basicData.ContentsTitleLabel, LocalizationManager.GetStringDirect(_portalRenewalData.TitleLocalKey));
	}

	private void SetBackgroundTexture()
	{
		RestoredPortal.SetupCardCurve(this, _portalRenewalData);
		if (_portalRenewalData.Category == PortalRenewalCategory.AdventureNotice && _portalRenewalData.SubCategory == PortalRenewalSubCategory.WorldBoss)
		{
			SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestWorldBossInfo(showLoadingGauge: false, delegate
			{
				WorldBossData currentWorldBossData = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.GetCurrentWorldBossData();
				if (currentWorldBossData != null)
				{
					AssetTexture newPortalBackground2 = PortalHelper.GetNewPortalBackground(currentWorldBossData.AdventureNoticeImagePath);
					UIHelper.ResetPlaneAssetTextureAsync(_basicData.ContentsTexture, _basicData.ContentsTexture, newPortalBackground2);
				}
			});
		}
		else if (!String_Extension_Method.IsNullOrEmpty(_portalRenewalData.MainImagePath) && _portalRenewalData.SubComponentShape != PortalRenewalCompType.Window)
		{
			AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(_portalRenewalData.MainImagePath);
			UIHelper.ResetPlaneAssetTextureAsync(_basicData.ContentsTexture, _basicData.ContentsTexture, newPortalBackground);
		}
	}

	protected void CheckLockedState()
	{
		PortalRenewal.LockedState lockedReason = PortalHelper.GetLockedReason(_portalRenewalData);
		ChangeLockState(lockedReason);
		if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.WorldBoss && lockedReason != PortalRenewal.LockedState.NONE)
		{
			UIHelper.SetLabelText(_basicData.ContentsTitleLabel, LocalizationManager.GetStringDirect("WORLDBOSS_MAIN_TITLE"));
		}
	}

	private void SetCurrency()
	{
		if (_portalRenewalData.SubCategory != PortalRenewalSubCategory.GuildArena && _portalRenewalData.SubCategory != PortalRenewalSubCategory.GuildSuppress)
		{
			SetCurrencyLabel();
		}
		else
		{
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: false);
		}
	}

	public void SetCurrencyLabel()
	{
		CurrencyShowType currencyShowType = _portalRenewalData.CurrencyShowType;
		bool enable = _portalRenewalData.CurrencyShowType != CurrencyShowType.NONE && LockedState == PortalRenewal.LockedState.NONE;
		UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable);
		if (currencyShowType == CurrencyShowType.NONE || (LockedState != PortalRenewal.LockedState.NONE && !_basicData.LockedStatePass))
		{
			return;
		}
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
		if (_portalRenewalData.CurrencyShowType > CurrencyShowType.NONE && _portalRenewalData.CurrencyShowType <= CurrencyShowType.NoAmount && _basicData.CurrencyIconRoot != null)
		{
			_iconProxy = IconProxy.Create(_basicData.CurrencyIconRoot);
		}
		else
		{
			_iconProxy = null;
		}
		switch (currencyShowType)
		{
		case CurrencyShowType.Amount:
		{
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: true);
			if (_basicData.CurrencyLabelBg != null)
			{
				UIHelper.EnableGameObject(_basicData.CurrencyLabelBg, enable: true);
				UIHelper.SetLabelColor(_basicData.CurrencyLabel, new Color(255f, 255f, 255f));
			}
			if (_portalRenewalData.CurrencyType == CurrencyType.Gem)
			{
				long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
				UIHelper.SetLabelText(_basicData.CurrencyLabel, Vespa_Int_Extension_Method.ToCommaString(totalGem));
				if (_iconProxy != null)
				{
					_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
				}
				break;
			}
			CurrencyTypeData data4 = GameRoot.Instance.CurrencyTypeDataContainer.GetData(_portalRenewalData.CurrencyType);
			if (data4 != null)
			{
				if (!String_Extension_Method.IsNullOrEmpty(data4.UserStatType))
				{
					string text3 = Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(data4.UserStatType));
					if (CurrencyMaxCap.ExceedMaxCap(_portalRenewalData.CurrencyType))
					{
						text3 = Vespa_String_Extension_Method.NguiColor(text3, Color.red);
					}
					UIHelper.SetLabelText(_basicData.CurrencyLabel, text3);
					if (_iconProxy != null)
					{
						_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
					}
					break;
				}
				if (!String_Extension_Method.IsNullOrEmpty(data4.StaminaType))
				{
					StaminaType o_value3 = StaminaType.None;
					EnumTools.TryParse<StaminaType>(data4.StaminaType, out o_value3);
					if (o_value3 > StaminaType.None)
					{
						long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(o_value3);
						UIHelper.SetLabelText(_basicData.CurrencyLabel, Vespa_Int_Extension_Method.ToCommaString(currentStamina));
						if (_iconProxy != null)
						{
							_iconProxy.SetupStamina(data4.StaminaType);
						}
						break;
					}
				}
			}
			UIHelper.SetLabelText(_basicData.CurrencyLabel, 0);
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
			}
			break;
		}
		case CurrencyShowType.AmountAndMax:
		{
			CurrencyTypeData data3 = GameRoot.Instance.CurrencyTypeDataContainer.GetData(_portalRenewalData.CurrencyType);
			if (data3 == null || (String_Extension_Method.IsNullOrEmpty(data3.StaminaType) && String_Extension_Method.IsNullOrEmpty(data3.UserStatType)))
			{
				UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: false);
				UIHelper.EnableGameObject(_iconProxy, enable: false);
				return;
			}
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: true);
			if (_basicData.CurrencyLabelBg != null)
			{
				UIHelper.EnableGameObject(_basicData.CurrencyLabelBg, enable: true);
				UIHelper.SetLabelColor(_basicData.CurrencyLabel, new Color(255f, 255f, 255f));
			}
			if (!String_Extension_Method.IsNullOrEmpty(data3.UserStatType))
			{
				long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(data3.UserStatType);
				int maxValue = data3.MaxValue;
				long num6 = maxValue;
				int num7 = 0;
				if (num6 > num7)
				{
					string stringDirect3 = LocalizationManager.GetStringDirect("PAYSHOP_STARTER_PICKUP_MILEAGE_COUNT", Vespa_Long_Extension_Method.ToCommaString(userStat), Vespa_Int_Extension_Method.ToCommaString(num6));
					UIHelper.SetLabelText(_basicData.CurrencyLabel, stringDirect3);
				}
				else
				{
					UIHelper.SetLabelText(_basicData.CurrencyLabel, Vespa_Long_Extension_Method.ToCommaString(userStat));
				}
				if (_iconProxy != null)
				{
					_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
				}
			}
			else if (!String_Extension_Method.IsNullOrEmpty(data3.StaminaType))
			{
				StaminaType o_value2 = StaminaType.None;
				EnumTools.TryParse<StaminaType>(data3.StaminaType, out o_value2);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(o_value2, out var currentValue, out var maxValue2);
				long num8 = maxValue2;
				int num9 = 0;
				if (num8 > num9)
				{
					string stringDirect4 = LocalizationManager.GetStringDirect("PAYSHOP_STARTER_PICKUP_MILEAGE_COUNT", Vespa_Int_Extension_Method.ToCommaString(currentValue), Vespa_Int_Extension_Method.ToCommaString(maxValue2));
					UIHelper.SetLabelText(_basicData.CurrencyLabel, stringDirect4);
				}
				else
				{
					UIHelper.SetLabelText(_basicData.CurrencyLabel, Vespa_Int_Extension_Method.ToCommaString(currentValue));
				}
				if (_iconProxy != null)
				{
					_iconProxy.SetupStamina(data3.StaminaType);
				}
				SetCurrencyRechargeData(o_value2, currentValue, maxValue2);
			}
			break;
		}
		case CurrencyShowType.NoAmount:
		{
			PlayerCurrencyInfo playerCurrencyInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Get(_portalRenewalData.CurrencyType);
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: true);
			if (_basicData.CurrencyLabelBg != null)
			{
				UIHelper.EnableGameObject(_basicData.CurrencyLabelBg, enable: false);
				UIHelper.SetLabelColor(_basicData.CurrencyLabel, new Color(0f, 0f, 0f));
			}
			UIHelper.SetLabelText(_basicData.CurrencyLabel, LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_CONSUME"));
			if (playerCurrencyInfo != null)
			{
				if (_iconProxy != null)
				{
					_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
				}
				break;
			}
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(_portalRenewalData.CurrencyType);
			if (data != null)
			{
				if (!String_Extension_Method.IsNullOrEmpty(data.UserStatType))
				{
					if (_iconProxy != null)
					{
						_iconProxy.SetupCurrencyNoBg(_portalRenewalData.CurrencyType);
					}
					break;
				}
				if (!String_Extension_Method.IsNullOrEmpty(data.StaminaType))
				{
					StaminaType o_value = StaminaType.None;
					EnumTools.TryParse<StaminaType>(data.StaminaType, out o_value);
					if (o_value > StaminaType.None)
					{
						if (_iconProxy != null)
						{
							_iconProxy.SetupStamina(data.StaminaType);
						}
						break;
					}
				}
			}
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: false);
			UIHelper.EnableGameObject(_iconProxy, enable: false);
			break;
		}
		case CurrencyShowType.AmountItem:
		{
			int valueInt2 = ArgParser.GetValueInt(_portalRenewalData.CurrencyParam, 0);
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(valueInt2);
			if (data2 != null)
			{
				FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
				_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(_basicData.ItemIconRoot.transform);
				_itemIconComponent.Open(data2);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data2.Code);
				long i_value = itemCount;
				int valueInt3 = ArgParser.GetValueInt(_portalRenewalData.CurrencyParam, 1);
				long num4 = valueInt3;
				int num5 = 0;
				if (num4 > num5)
				{
					string stringDirect2 = LocalizationManager.GetStringDirect("PAYSHOP_STARTER_PICKUP_MILEAGE_COUNT", Vespa_Int_Extension_Method.ToCommaString(i_value), Vespa_Int_Extension_Method.ToCommaString(num4));
					UIHelper.SetLabelText(_basicData.CurrencyLabel, stringDirect2);
				}
				else
				{
					UIHelper.SetLabelText(_basicData.CurrencyLabel, Vespa_Int_Extension_Method.ToCommaString(i_value));
				}
			}
			break;
		}
		case CurrencyShowType.TrialTower:
		case CurrencyShowType.GatewayToTrial:
		{
			int num = 0;
			long num2 = num;
			int valueInt = ArgParser.GetValueInt(_portalRenewalData.CurrencyParam, 0);
			int maxFloor = BaseTower.GetMaxFloor(valueInt);
			long num3 = maxFloor;
			TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(valueInt);
			if (towerInfo != null)
			{
				int completedFloor = towerInfo.CompletedFloor;
				num2 = completedFloor;
			}
			string text = LocalizationManager.GetString(Tower.FLOOR, Vespa_Int_Extension_Method.ToCommaString(num2));
			string text2 = LocalizationManager.GetString(Tower.FLOOR, Vespa_Int_Extension_Method.ToCommaString(num3));
			if (currencyShowType == CurrencyShowType.GatewayToTrial)
			{
				text = LocalizationManager.GetStringDirect("GATEWAY_TO_TRIAL.GRADE", num2);
				text2 = LocalizationManager.GetStringDirect("GATEWAY_TO_TRIAL.GRADE", num3);
			}
			string stringDirect = LocalizationManager.GetStringDirect("PAYSHOP_STARTER_PICKUP_MILEAGE_COUNT", text, text2);
			UIHelper.EnableGameObject(_basicData.CurrencyGroup, enable: true);
			UIHelper.SetLabelText(_basicData.CurrencyLabel, stringDirect);
			break;
		}
		}
		ResetCurrencyAnchors();
	}

	private void ResetCurrencyAnchors()
	{
		if (_basicData != null && !(_basicData.CurrencyGroup == null))
		{
			UIWidget component = _basicData.CurrencyGroup.GetComponent<UIWidget>();
			if (!(component == null))
			{
				component.ForceUpdateAnchors(includeChildren: true);
			}
		}
	}

	private void SetCurrencyRechargeData(StaminaType staminaType, long curAmount, long maxAmount)
	{
		if (staminaType != StaminaType.MazeKey)
		{
			StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
			if (data != null && data.UpdateType != StaminaUpdateType.None)
			{
				int rechageCount = ((data.UpdateType == StaminaUpdateType.Regen) ? 1 : GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeStamina(staminaType));
				DateTime? nextRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetNextRechargeTime(staminaType);
				StaminaUpdateType staminaUpdateType = ((!String_Extension_Method.IsNullOrEmpty(data.StaminaTooltipDesc)) ? StaminaUpdateType.Regen : data.UpdateType);
				_rechargeData = new RechargeData(this, data.StaminaType, curAmount, maxAmount, rechageCount, nextRechargeTime.Value, staminaUpdateType);
				UIHelper.EnableGameObject(_basicData.CurrencyRechargeTimeLabel, enable: true);
			}
		}
	}

	private void SetDescription()
	{
		UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: false);
		if (!String_Extension_Method.IsNullOrEmpty(_portalRenewalData.DescLocalKey) && (LockedState == PortalRenewal.LockedState.NONE || _basicData.LockedStatePass))
		{
			UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: true);
			UIHelper.SetLabelText(_basicData.ContentsDescriptionLabel, LocalizationManager.GetStringDirect(_portalRenewalData.DescLocalKey));
		}
	}

	protected void ChangeLockState(PortalRenewal.LockedState lockedState)
	{
		LockedState = lockedState;
		string text = string.Empty;
		switch (LockedState)
		{
		case PortalRenewal.LockedState.NONE:
			UIHelper.EnableGameObject(_basicData.LockedStateGroup, enable: false);
			UIHelper.SetLabelColor(_basicData.ContentsTitleLabel, _basicData.UnlockedTitleColor);
			return;
		case PortalRenewal.LockedState.CONDITION_LOCKED:
			text = LocalizationManager.GetStringDirect(_portalRenewalData.OpenConditionLocalKey);
			break;
		case PortalRenewal.LockedState.TUTORIAL_LOCKED:
			text = LocalizationManager.GetStringDirect("TUTORIAL_DISABLE_DES");
			break;
		}
		if (_portalRenewalData.SubComponentShape == PortalRenewalCompType.Window)
		{
			UIHelper.SetLabelColor(_basicData.ContentsTitleLabel, _basicData.UnlockedTitleColor);
		}
		else
		{
			UIHelper.SetLabelColor(_basicData.ContentsTitleLabel, _basicData.LockedTitleColor);
		}
		UIHelper.EnableGameObject(_basicData.LockedStateGroup, enable: true);
		UIHelper.SetLabelText(_basicData.LockedReasonLabel, text);
	}

	protected void CheckEventBuffCondition()
	{
		bool flag = PortalHelper.CheckEventCondition(_portalRenewalData);
		UIHelper.EnableGameObject(_basicData.EventBuffRoot, flag && LockedState == PortalRenewal.LockedState.NONE);
	}

	private void UpdateContentsNotice()
	{
		PortalRenewal.NoticeState noticeState = ((LockedState == PortalRenewal.LockedState.NONE) ? PortalHelper.GetContentsNoticeState(_portalRenewalData) : PortalRenewal.NoticeState.NONE);
		if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.Chapter)
		{
			noticeState = PortalRenewal.NoticeState.NONE;
		}
		ChangeNoticeState(noticeState);
	}

	public void ChangeNoticeState(PortalRenewal.NoticeState noticeState)
	{
		NoticeState = noticeState;
		UIHelper.EnableGameObject(_basicData.NoticeStateIconRoot, noticeState != PortalRenewal.NoticeState.NONE);
		if (_basicData.StateIconSelector != null)
		{
			_basicData.StateIconSelector.SelectEnum(noticeState);
		}
	}

	private void UpdateContentsStateIcon()
	{
		UIHelper.EnableGameObject(_basicData.Icon_Global, !__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalData.ContentsStateIcon) && _portalRenewalData.ContentsStateIcon.Contains(ContentsStateIcon.Global) && LockedState == PortalRenewal.LockedState.NONE && !_basicData.LockedStatePass);
		UIHelper.EnableGameObject(_basicData.Icon_Multi, !__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalData.ContentsStateIcon) && _portalRenewalData.ContentsStateIcon.Contains(ContentsStateIcon.Multi) && LockedState == PortalRenewal.LockedState.NONE && !_basicData.LockedStatePass);
	}

	private void UpdateContentsStateIconTable()
	{
		if (LockedState == PortalRenewal.LockedState.NONE && _basicData != null && !_basicData.LockedStatePass && !(_basicData.ContentsStateIconTable == null) && !__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalData.ContentsStateIcon))
		{
			for (int i = 0; i < _portalRenewalData.ContentsStateIcon.Length; i++)
			{
				_contentsStateIconComponentList.Add(CreateContentsStateIcon(_portalRenewalData.ContentsStateIcon[i], _basicData.ContentsStateIconTable.transform));
			}
			_basicData.ContentsStateIconTable.Reposition();
		}
	}

	private PortalContentsStateIconComponent CreateContentsStateIcon(ContentsStateIcon iconType, Transform rootTransform)
	{
		if (rootTransform == null)
		{
			return null;
		}
		PortalContentsStateIconComponent portalContentsStateIconComponent = PortalContentsStateIconComponent.PrefabAuto.Factory.Create(rootTransform);
		if (portalContentsStateIconComponent == null)
		{
			return null;
		}
		portalContentsStateIconComponent.Open(new PortalContentsStateIconComponent.Info
		{
			IconType = iconType
		});
		return portalContentsStateIconComponent;
	}

	private void CheckContentResetTime()
	{
		PortalRenewalSubCategory subCategory = _portalRenewalData.SubCategory;
		if ((uint)(subCategory - 500) <= 2u || (uint)(subCategory - 1200) <= 2u)
		{
			SetContentResetTimeNotAvailable();
		}
		else
		{
			SetContentsResetTime();
		}
	}

	private void SetContentResetTimeNotAvailable()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_contentsResetDatas);
		_basicData.ContentsResetTimeGroups.ForEach(delegate(Transform group)
		{
			UIHelper.EnableGameObject(group, enable: false);
		});
		UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
		string key = "RESULT_TYPE.PORTAL_COMMON_NOT_AVAILABLE";
		if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.GuildRaid)
		{
			key = "RESULT_TYPE.PORTAL_GUILDRAID_UNOPENED";
		}
		UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect(key));
	}

	private void SetContentsResetTime()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_contentsResetDatas);
		_basicData.ContentsResetTimeGroups.ForEach(delegate(Transform group)
		{
			UIHelper.EnableGameObject(group, enable: false);
		});
		if (LockedState != PortalRenewal.LockedState.NONE && !_basicData.LockedStatePass)
		{
			return;
		}
		if (_portalRenewalData.ShopIndex > 0)
		{
			SetShopResetTime();
			return;
		}
		switch (_portalRenewalData.SubCategory)
		{
		default:
			UpdateResetTime();
			break;
		case PortalRenewalSubCategory.UnderPrison:
		{
			int remainDaySecond = RequestEnterLobbyWrapper.GetRemainDaySecond();
			DateTime nextResetTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime().AddSeconds(remainDaySecond);
			_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_RESET_TIME", nextResetTime));
			break;
		}
		case PortalRenewalSubCategory.TreasureHouse:
			SceneAutoBehaviour<TreasureHouseManagement>.instance.RequestTreasureHouseInfo(delegate
			{
				PlayerTreasureHouseInfo treasureHouseInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TreasureHouseInfo;
				if (treasureHouseInfo == null)
				{
					VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
				}
				else
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					double totalSeconds = (Convert.ToDateTime(treasureHouseInfo.NextResetTime) - currentServerUTCTime).TotalSeconds;
					if (totalSeconds < 0.0)
					{
						VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
					}
					else
					{
						DateTime nextResetTime3 = currentServerUTCTime.AddSeconds(totalSeconds);
						_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_RESET_TIME", nextResetTime3, null, needUpdate: false));
					}
				}
			});
			break;
		case PortalRenewalSubCategory.TrialTower:
		case PortalRenewalSubCategory.ChallengeTower:
		case PortalRenewalSubCategory.Maze:
		{
			int towerIndex = 0;
			if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.ChallengeTower)
			{
				towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Challenge);
			}
			else if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.TrialTower)
			{
				towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Ordeal);
			}
			else if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.Maze)
			{
				towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Maze);
			}
			int remainResetTime = TowerHelper.GetRemainResetTime(towerIndex);
			if (remainResetTime < 0)
			{
				VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
				break;
			}
			DateTime nextResetTime2 = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime().AddSeconds(remainResetTime);
			_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_RESET_TIME", nextResetTime2));
			break;
		}
		case PortalRenewalSubCategory.LeagueOfVictory:
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestSeasonInfo(MatchPvpType.Normal, delegate(NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo == null)
				{
					UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
					UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
				}
				else
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					if ((seasonInfo.EndUtc - currentServerUTCTime).TotalSeconds < 0.0)
					{
						VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
					}
					else
					{
						_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME", seasonInfo.EndUtc));
					}
				}
			});
			break;
		case PortalRenewalSubCategory.LeagueOfGlory:
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo == null)
				{
					UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
					UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
				}
				else
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					if ((seasonInfo.EndUtc - currentServerUTCTime).TotalSeconds < 0.0)
					{
						VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
					}
					else if (seasonInfo.MatchSeasonType == MatchSeasonType.Free)
					{
						_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_FREESEASON_START_TIME", seasonInfo.EndUtc));
					}
					else if (seasonInfo.MatchSeasonType == MatchSeasonType.Regular)
					{
						_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_FREESEASON_FINISH_TIME", seasonInfo.EndUtc));
					}
				}
			});
			break;
		case PortalRenewalSubCategory.OrdealArena:
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestOrdealSeasonInfo(delegate(NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo == null)
				{
					UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
					UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
				}
				else
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					if ((seasonInfo.EndUtc - currentServerUTCTime).TotalSeconds < 0.0)
					{
						VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
					}
					else
					{
						_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME", seasonInfo.EndUtc));
					}
				}
			});
			break;
		case PortalRenewalSubCategory.GuildArena:
		{
			UserGuildManager guildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
			if (guildManager == null)
			{
				break;
			}
			NShared.GetGuildArenaSessionInfo.Request request = new NShared.GetGuildArenaSessionInfo.Request();
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaSessionInfo.Response response)
			{
				if (response != null && response.BaseResult == BaseResultType.Success)
				{
					if (response.Result != ResultType.Success || response.SessionInfo == null)
					{
						UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
						UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
					}
					else
					{
						guildManager.GuildArenaManager.ApplyGuildArenaSessionInfo(response);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
						GuildArenaSessionInfo sessionInfo = response.SessionInfo;
						DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
						GuildArenaSeasonData dataBySeasonIndex = GameRoot.Instance.GuildArenaSeasonDataContainer.GetDataBySeasonIndex(sessionInfo.SeasonIndex);
						if (dataBySeasonIndex != null)
						{
							switch (sessionInfo.State)
							{
							case GuildArenaSessionState.NotHeld:
								UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
								UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
								break;
							case GuildArenaSessionState.Apply:
							case GuildArenaSessionState.ApplyEnded:
							{
								DateTime o_dateTime3;
								if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) == 2)
								{
									if (Vespa_String_Extension_Method2.TryParseNetworkTime2(dataBySeasonIndex.End, out var o_dateTime))
									{
										double totalSeconds = (o_dateTime - currentServerUTCTime).TotalSeconds;
										if (totalSeconds < 0.0)
										{
											VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										}
										else
										{
											DateTime nextResetTime3 = currentServerUTCTime.AddSeconds(totalSeconds);
											string guildArenaSeasonDisplayName = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonDisplayName(sessionInfo.SeasonIndex);
											_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_FINISH_TIME_2", nextResetTime3, guildArenaSeasonDisplayName, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
											if (Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.BattleBeginTime, out var o_dateTime2))
											{
												double totalSeconds2 = (o_dateTime2 - currentServerUTCTime).TotalSeconds;
												if (totalSeconds2 < 0.0)
												{
													VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
												}
												else
												{
													DateTime nextResetTime4 = currentServerUTCTime.AddSeconds(totalSeconds2);
													_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime4, sessionInfo.SessionNumber, needUpdate: true, GetResetTimeGroup(1), GetResetTimeLabel(1)));
												}
											}
										}
									}
								}
								else if (Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.BattleBeginTime, out o_dateTime3))
								{
									double totalSeconds3 = (o_dateTime3 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds3 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
									}
									else
									{
										DateTime nextResetTime5 = currentServerUTCTime.AddSeconds(totalSeconds3);
										_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime5, sessionInfo.SessionNumber, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
									}
								}
								break;
							}
							case GuildArenaSessionState.Battle:
								if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) == 2)
								{
									if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(dataBySeasonIndex.End, out var o_dateTime4))
									{
										break;
									}
									double totalSeconds4 = (o_dateTime4 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds4 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										break;
									}
									DateTime nextResetTime6 = currentServerUTCTime.AddSeconds(totalSeconds4);
									string guildArenaSeasonDisplayName2 = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonDisplayName(sessionInfo.SeasonIndex);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_FINISH_TIME_2", nextResetTime6, guildArenaSeasonDisplayName2, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
									if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.BattleEndTime, out var o_dateTime5))
									{
										break;
									}
									double totalSeconds5 = (o_dateTime5 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds5 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										break;
									}
									DateTime nextResetTime7 = currentServerUTCTime.AddSeconds(totalSeconds5);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME_2", nextResetTime7, sessionInfo.SessionNumber, needUpdate: true, GetResetTimeGroup(1), GetResetTimeLabel(1)));
								}
								else
								{
									if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.BattleEndTime, out var o_dateTime6))
									{
										break;
									}
									double totalSeconds6 = (o_dateTime6 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds6 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										break;
									}
									DateTime nextResetTime8 = currentServerUTCTime.AddSeconds(totalSeconds6);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME_2", nextResetTime8, sessionInfo.SessionNumber, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
								}
								if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsMatched)
								{
									SetCurrencyLabel();
								}
								break;
							case GuildArenaSessionState.Ended:
								UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
								if (sessionInfo.IsLastSession == 1)
								{
									UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
								}
								else
								{
									UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_COMMON_NOT_AVAILABLE"));
								}
								break;
							}
						}
					}
				}
			});
			break;
		}
		case PortalRenewalSubCategory.GuildSuppress:
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id <= 0)
			{
				break;
			}
			SceneAutoBehaviour<GuildSuppressManagement>.instance.InternalRequestGuildSuppressSessionInfo(delegate
			{
				GuildSuppressSessionData guildSuppressSessionData = SceneAutoBehaviour<GuildSuppressManagement>.instance.GuildSuppressSessionData;
				GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
				if (guildSuppressSessionData != null && guildSuppressSessionInfo != null)
				{
					GuildSuppressSessionData lastSessionData = GameRoot.Instance.GuildSuppressSessionDataContainer.GetLastSessionData(guildSuppressSessionData.SeasonIndex);
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					switch (guildSuppressSessionInfo.State)
					{
					case GuildSuppressSessionState.Apply:
					case GuildSuppressSessionState.ApplyEnded:
					{
						DateTime o_dateTime10;
						if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) == 2)
						{
							if (Vespa_String_Extension_Method2.TryParseNetworkTime2(lastSessionData.GlobalBattleEnd, out var o_dateTime8))
							{
								double totalSeconds8 = (o_dateTime8 - currentServerUTCTime).TotalSeconds;
								if (totalSeconds8 < 0.0)
								{
									VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
								}
								else
								{
									DateTime nextResetTime10 = currentServerUTCTime.AddSeconds(totalSeconds8);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_FINISH_TIME_2", nextResetTime10, lastSessionData.SeasonIndex, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
									if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleStart, out var o_dateTime9))
									{
										double totalSeconds9 = (o_dateTime9 - currentServerUTCTime).TotalSeconds;
										if (totalSeconds9 < 0.0)
										{
											VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										}
										else
										{
											DateTime nextResetTime11 = currentServerUTCTime.AddSeconds(totalSeconds9);
											_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime11, guildSuppressSessionData.PortalViewSession, needUpdate: true, GetResetTimeGroup(1), GetResetTimeLabel(1)));
										}
									}
								}
							}
						}
						else if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleStart, out o_dateTime10))
						{
							double totalSeconds10 = (o_dateTime10 - currentServerUTCTime).TotalSeconds;
							if (totalSeconds10 < 0.0)
							{
								VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
							}
							else
							{
								DateTime nextResetTime12 = currentServerUTCTime.AddSeconds(totalSeconds10);
								_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime12, guildSuppressSessionData.PortalViewSession, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
							}
						}
						break;
					}
					case GuildSuppressSessionState.Battle:
						if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) == 2)
						{
							if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(lastSessionData.GlobalBattleEnd, out var o_dateTime5))
							{
								break;
							}
							double totalSeconds5 = (o_dateTime5 - currentServerUTCTime).TotalSeconds;
							if (totalSeconds5 < 0.0)
							{
								VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
								break;
							}
							DateTime nextResetTime7 = currentServerUTCTime.AddSeconds(totalSeconds5);
							_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_FINISH_TIME_2", nextResetTime7, lastSessionData.SeasonIndex, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
							if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleEnd, out var o_dateTime6))
							{
								break;
							}
							double totalSeconds6 = (o_dateTime6 - currentServerUTCTime).TotalSeconds;
							if (totalSeconds6 < 0.0)
							{
								VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
								break;
							}
							DateTime nextResetTime8 = currentServerUTCTime.AddSeconds(totalSeconds6);
							_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME_2", nextResetTime8, guildSuppressSessionData.PortalViewSession, needUpdate: true, GetResetTimeGroup(1), GetResetTimeLabel(1)));
						}
						else
						{
							if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleEnd, out var o_dateTime7))
							{
								break;
							}
							double totalSeconds7 = (o_dateTime7 - currentServerUTCTime).TotalSeconds;
							if (totalSeconds7 < 0.0)
							{
								VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
								break;
							}
							DateTime nextResetTime9 = currentServerUTCTime.AddSeconds(totalSeconds7);
							_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_FINISH_TIME_2", nextResetTime9, guildSuppressSessionData.PortalViewSession, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
						}
						if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Applied)
						{
							SetCurrencyLabel();
						}
						break;
					default:
					{
						int index = (int)guildSuppressSessionInfo.SessionIndex + 1;
						GuildSuppressSessionData guildSuppressSessionData2 = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, index);
						if (guildSuppressSessionData2 != null)
						{
							DateTime o_dateTime4;
							if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) == 2)
							{
								if (guildSuppressSessionData.SessionIndex == lastSessionData.SessionIndex)
								{
									if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData2.GlobalApplyStart, out var o_dateTime))
									{
										break;
									}
									double totalSeconds = (o_dateTime - currentServerUTCTime).TotalSeconds;
									if (totalSeconds < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										break;
									}
									DateTime nextResetTime3 = currentServerUTCTime.AddSeconds(totalSeconds);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_START_TIME_2", nextResetTime3, guildSuppressSessionData2.SeasonIndex, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
								}
								else
								{
									if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(lastSessionData.GlobalBattleEnd, out var o_dateTime2))
									{
										break;
									}
									double totalSeconds2 = (o_dateTime2 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds2 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
										break;
									}
									DateTime nextResetTime4 = currentServerUTCTime.AddSeconds(totalSeconds2);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_EPISODE_FINISH_TIME_2", nextResetTime4, lastSessionData.SeasonIndex, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
								}
								if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData2.GlobalBattleStart, out var o_dateTime3))
								{
									double totalSeconds3 = (o_dateTime3 - currentServerUTCTime).TotalSeconds;
									if (totalSeconds3 < 0.0)
									{
										VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
									}
									else
									{
										DateTime nextResetTime5 = currentServerUTCTime.AddSeconds(totalSeconds3);
										_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime5, guildSuppressSessionData2.PortalViewSession, needUpdate: true, GetResetTimeGroup(1), GetResetTimeLabel(1)));
									}
								}
							}
							else if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData2.GlobalBattleStart, out o_dateTime4))
							{
								double totalSeconds4 = (o_dateTime4 - currentServerUTCTime).TotalSeconds;
								if (totalSeconds4 < 0.0)
								{
									VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
								}
								else
								{
									DateTime nextResetTime6 = currentServerUTCTime.AddSeconds(totalSeconds4);
									_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_SEASON_START_TIME_2", nextResetTime6, guildSuppressSessionData2.PortalViewSession, needUpdate: true, GetResetTimeGroup(0), GetResetTimeLabel(0)));
								}
							}
						}
						else
						{
							UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
							UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
						}
						break;
					}
					}
				}
			}, enableErrorPopup: false);
			break;
		case PortalRenewalSubCategory.WorldBoss:
			UIHelper.SetLabelText(_basicData.ContentsTitleLabel, "-");
			UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: false);
			SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestWorldBossInfo(showLoadingGauge: false, delegate(WorldBossInfo info)
			{
				if (info != null)
				{
					WorldBossData currentWorldBossData = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.GetCurrentWorldBossData();
					if (currentWorldBossData != null)
					{
						string stringDirect = LocalizationManager.GetStringDirect(currentWorldBossData.Name);
						string stringDirect2 = LocalizationManager.GetStringDirect("PORTAL_ADVENTURE_WORLDBOSS", stringDirect);
						UIHelper.SetLabelText(_basicData.ContentsTitleLabel, stringDirect2);
						int num = currentWorldBossData.DealBattleDuration + currentWorldBossData.NormalBattleDuration;
						if (Vespa_String_Extension_Method2.TryParseNetworkTime2(info.CreatedTime, out var o_dateTime))
						{
							DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
							int num2 = (int)(currentServerUTCTime - o_dateTime).TotalSeconds;
							int num3 = Mathf.Max(num - num2, 0);
							if (num3 < 0)
							{
								VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
							}
							else
							{
								DateTime nextResetTime3 = currentServerUTCTime.AddSeconds(num3);
								_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_FINISH_TIME", nextResetTime3, null, needUpdate: false));
								if (_portalRenewalData.SubComponentShape == PortalRenewalCompType.Small)
								{
									AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(currentWorldBossData.AdventureNoticeImagePath);
									UIHelper.ResetPlaneAssetTextureAsync(_basicData.ContentsTexture, _basicData.ContentsTexture, newPortalBackground);
								}
							}
						}
					}
				}
			});
			break;
		case PortalRenewalSubCategory.GuildRaidShop:
			break;
		}
	}

	private void SetShopResetTime()
	{
		if (_portalRenewalData.ShopIndex <= 0)
		{
			return;
		}
		switch (_portalRenewalData.SubCategory)
		{
		case PortalRenewalSubCategory.TotalShop:
		case PortalRenewalSubCategory.MayGemShop:
			SceneAutoBehaviour<ShopFacade>.instance.Reset(_portalRenewalData.ShopIndex);
			_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_LIST_RESET_TIME", SceneAutoBehaviour<ShopFacade>.instance.NextRestockTime, null, needUpdate: false));
			return;
		case PortalRenewalSubCategory.GloryFlagShop:
			UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
			UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_LIST_RESET_SEASON_FINISH"));
			return;
		}
		int shopIndex = _portalRenewalData.ShopIndex;
		int groupIndexCount = GameRoot.Instance.ShopItemGroupDataContainer.GetGroupIndexCount(shopIndex);
		List<ShopItemData> list = new List<ShopItemData>();
		if (_portalRenewalData.SubCategory == PortalRenewalSubCategory.GuildShop2)
		{
			int valueInt = ArgParser.GetValueInt(_portalRenewalData.ShopParam, 0);
			GuildBuildingInfo guildBuildingInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildBuildingInfo(valueInt);
			if (guildBuildingInfo == null)
			{
				return;
			}
			byte buildingLevel = guildBuildingInfo.BuildingLevel;
			list = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(shopIndex, buildingLevel);
		}
		else if (groupIndexCount > 0)
		{
			for (int i = 1; i <= groupIndexCount; i++)
			{
				List<ShopItemData> shopItemListByShopItemGroup = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(shopIndex, i);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(shopItemListByShopItemGroup))
				{
					list.AddRange(shopItemListByShopItemGroup.FindAll((ShopItemData x) => x.DailyReset || x.WeeklyReset));
				}
			}
		}
		else
		{
			List<ShopItemData> shopItemList = GameRoot.Instance.ShopItemDataContainer.GetShopItemList(shopIndex);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(shopItemList))
			{
				list.AddRange(shopItemList.FindAll((ShopItemData x) => x.DailyReset || x.WeeklyReset));
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		list.Sort((ShopItemData l, ShopItemData r) => l.WeeklyReset.CompareTo(r.WeeklyReset));
		ShopItemData shopItemData = list.Find((ShopItemData x) => x.WeeklyReset || x.DailyReset);
		if (shopItemData != null)
		{
			UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
			int num = (shopItemData.WeeklyReset ? RequestEnterLobbyWrapper.GetRemainWeekSecond() : RequestEnterLobbyWrapper.GetRemainDaySecond());
			if (num < 0)
			{
				VespaDebug.LogError($"Portal {_portalRenewalData.SubCategory} subCategory Time Wrong!!!");
				return;
			}
			DateTime nextResetTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime().AddSeconds(num);
			_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_RESET_TIME", nextResetTime));
		}
	}

	public Transform GetResetTimeGroup(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeGroups) <= index)
		{
			return null;
		}
		return _basicData.ContentsResetTimeGroups[index];
	}

	public UILabel GetResetTimeLabel(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_basicData.ContentsResetTimeLabels) <= index)
		{
			return null;
		}
		return _basicData.ContentsResetTimeLabels[index];
	}

	public void OnClickContentsButton()
	{
		if (LockedState == PortalRenewal.LockedState.NONE && _listener != null)
		{
			_listener.OnClickContentsButton(_portalRenewalData);
		}
	}

	public void ForceContentsUpdate(PortalRenewalData portalRenewalData)
	{
		if (LockedState == PortalRenewal.LockedState.NONE && _listener != null)
		{
			_listener.OnClickContentsButton(portalRenewalData);
		}
	}

	public void OnChangeStaminaInfo(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType != UserManager.UserStatType.Stamina || _portalRenewalData.CurrencyShowType <= CurrencyShowType.NONE || _portalRenewalData.CurrencyShowType == CurrencyShowType.TrialTower || _portalRenewalData.CurrencyShowType == CurrencyShowType.GatewayToTrial)
		{
			return;
		}
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(_portalRenewalData.CurrencyType);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.StaminaType))
		{
			EnumTools.TryParse<StaminaType>(data.StaminaType, out var o_value);
			if (o_value > StaminaType.None && changedInfo.StaminaType == o_value)
			{
				SetCurrencyLabel();
			}
		}
	}

	public void OnChangeCurrencyInfo(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		if (_portalRenewalData.CurrencyShowType > CurrencyShowType.NONE && _portalRenewalData.CurrencyShowType != CurrencyShowType.TrialTower && _portalRenewalData.CurrencyShowType != CurrencyShowType.GatewayToTrial && _portalRenewalData.CurrencyType == changedInfo.currencyType)
		{
			SetCurrencyLabel();
		}
	}

	public void OnChangeItemCount(ItemInfo changedInfo)
	{
		if (_portalRenewalData.CurrencyShowType == CurrencyShowType.AmountItem && !__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalData.CurrencyParam) && _portalRenewalData.CurrencyParam.Length >= 1 && ArgParser.GetValueInt(_portalRenewalData.CurrencyParam, 0) == changedInfo.ItemIndex)
		{
			SetCurrencyLabel();
		}
	}

	public void FixPanelsDepth(int depth)
	{
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth = depth;
		}
	}

	public virtual void ResolutionComp(float contentsWidth)
	{
		if (_basicData.ResolutionRoot == null)
		{
			return;
		}
		float num = contentsWidth;
		if (_basicData.LimitWidth > 0f)
		{
			int num2 = (int)_basicData.LimitWidth;
			if (num > (float)num2)
			{
				float num3 = (contentsWidth - (float)num2) / 2f;
				base.transform.localPosition = Vector2.right * num3;
				num = num2;
			}
		}
		_basicData.ResolutionRoot.width = (int)num;
	}

	protected string GetBackgrounCode(string code)
	{
		return "Portal" + code + "_Image";
	}

	protected abstract void UpdateResetTime();
}

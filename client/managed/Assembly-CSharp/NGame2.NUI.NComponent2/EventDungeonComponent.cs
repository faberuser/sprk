using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EventDungeonComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickEnter(EventDungeonComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/EventDungeon/EventDungeonComponent";

	public static PrefabAuto<EventDungeonComponent> PrefabAuto = new PrefabAuto<EventDungeonComponent>("Prefab2/Component/EventDungeon/EventDungeonComponent");

	public UILabel NameLabel;

	public UILabel NamePrefixLabel;

	public UITexture BossImageTexture;

	public UIButton ButtonLargeEnter;

	public Transform RootDisableLargeEnter;

	public UIButton ButtonSmallEnter;

	public Transform RootDisableSmallEnter;

	public UIButton ButtonHelpPopup;

	public Transform RootDisableHelpPopup;

	public UIScrollGrid RewardListGrid;

	public UILabel NotOpenLabel;

	public UILabel LabelOpenedDesc;

	public UILabel LabelClosedDesc;

	public Transform OpenedRoot;

	public Transform ClearedRoot;

	public Transform NotYetRoot;

	public Transform RootLock;

	public Transform RootProp;

	public GameObject RootPropSpecial;

	public UIWidget WidgetPropStaminaOneButton;

	public UIWidget WidgetPropStamianTwoButton;

	public UIGame3DWidget ClearEffectWidget;

	public UIGame3DWidget OpenedNormalEffectWidget;

	public UIGame3DWidget OpenedFinishEffectWidget;

	public Transform PartyMemberPlayingRoot;

	public float ClearEffectDelay = 0.2f;

	public float OpeningEffectDelay = 1f;

	public float FadeDuration = 1f;

	protected bool _isOneButton;

	protected Listener _listener;

	private EventDungeonData _eventDungeonData;

	private CreatureData _bossCreatureData;

	private CampaignWaveData _bossWaveData;

	private IconProxy _iconProxy;

	private List<ItemIconComponent> _rewardList = new List<ItemIconComponent>();

	private bool _openConditionCheck;

	private DateTime _lastUpdateTime;

	public EventDungeonData EventDungeonData => _eventDungeonData;

	public CreatureData BossCreatureData => _bossCreatureData;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_openConditionCheck = false;
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
		_iconProxy = null;
		FactoryBase.ReleaseAndClearList(_rewardList);
		UIHelper.EnableGameObject(WidgetPropStaminaOneButton, enable: false);
		UIHelper.EnableGameObject(RootPropSpecial, enable: false);
	}

	public void ChainUpdate()
	{
		if (_openConditionCheck && (DateTime.Now - _lastUpdateTime).TotalSeconds > 1.0)
		{
			_lastUpdateTime = DateTime.Now;
			InitState();
		}
	}

	protected virtual int GetChapterIndex()
	{
		if (_eventDungeonData == null)
		{
			return 0;
		}
		return _eventDungeonData.ChapterIndex;
	}

	protected virtual int GetDungeonIndex()
	{
		if (_eventDungeonData == null)
		{
			return 0;
		}
		return _eventDungeonData.DungeonIndex;
	}

	protected virtual string GetPortraitPath()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.PortraitPath;
	}

	protected virtual string GetDungeonNameKey()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.DungeonCardName;
	}

	protected virtual string GetDungeonPrefixKey()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.DungeonCardPrefix;
	}

	protected virtual EventDungeonPropType[] GetPropTypes()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.PropTypes;
	}

	protected virtual string[] GetHelpTexts()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.HelpTexts;
	}

	protected virtual string GetOpenDescKey()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.OpenedDungeonDesc;
	}

	protected virtual string GetCloseDescKey()
	{
		if (_eventDungeonData == null)
		{
			return null;
		}
		return _eventDungeonData.ClosedDungeonDesc;
	}

	protected virtual int GetRewardIndex()
	{
		if (_eventDungeonData == null)
		{
			return 0;
		}
		return _eventDungeonData.RewardIndex;
	}

	protected virtual bool IsOpenDungeon(out string notOpenText)
	{
		notOpenText = string.Empty;
		if (_eventDungeonData == null)
		{
			return false;
		}
		return UIHelper.CheckEventDungeonOpenCondition(_eventDungeonData, out notOpenText);
	}

	protected virtual EEventDungeonCloseType GetCloseType()
	{
		if (_eventDungeonData == null)
		{
			return EEventDungeonCloseType.None;
		}
		return _eventDungeonData.CloseType;
	}

	public void Open(Listener listener, EventDungeonData eventDungeonData)
	{
		_listener = listener;
		_eventDungeonData = eventDungeonData;
		_isOneButton = __Extension_Method_ICollection__.IsNullOrEmpty(GetHelpTexts());
		Init();
		InitBossImage();
		InitRewardList();
		SetProps(GetPropTypes());
	}

	protected void Init()
	{
		int chapterIndex = GetChapterIndex();
		int dungeonIndex = GetDungeonIndex();
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		DungeonDifficultyType difficulty = DungeonDifficultyType.Easy;
		if (waveCollection != null)
		{
			difficulty = waveCollection.MaxDifficulty;
		}
		_bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out _bossWaveData);
		UIHelper.EnableGameObject(OpenedRoot, enable: false);
		UIHelper.EnableGameObject(ClearedRoot, enable: false);
		UIHelper.EnableGameObject(NotYetRoot, enable: false);
		UIHelper.EnableGameObject(PartyMemberPlayingRoot, enable: false);
		UIHelper.EnableGameObject(NameLabel, enable: false);
		UIHelper.EnableGameObject(NamePrefixLabel, enable: false);
		UIHelper.EnableGameObject(ButtonLargeEnter, enable: false);
		UIHelper.EnableGameObject(RootDisableLargeEnter, enable: false);
		UIHelper.EnableGameObject(ButtonSmallEnter, enable: false);
		UIHelper.EnableGameObject(RootDisableSmallEnter, enable: false);
		UIHelper.EnableGameObject(ButtonHelpPopup, enable: false);
		UIHelper.EnableGameObject(RootDisableHelpPopup, enable: false);
		UIHelper.EnableGameObject(RootLock, enable: false);
		UIHelper.SetLabelTextLocalizeDirect(NameLabel, GetDungeonNameKey());
		UIHelper.SetLabelTextLocalizeDirect(NamePrefixLabel, GetDungeonPrefixKey());
		UIHelper.SetLabelTextLocalizeDirect(LabelOpenedDesc, GetOpenDescKey());
		UIHelper.SetLabelTextLocalizeDirect(LabelClosedDesc, GetCloseDescKey());
	}

	protected void InitBossImage()
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(GetPortraitPath());
		UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, plainTexture, UIHelper.EAssetTextureAssign.None, delegate
		{
			InitState();
		});
	}

	private void InitState()
	{
		bool flag = true;
		string notOpenText = "";
		flag = IsOpenDungeon(out notOpenText);
		_openConditionCheck = true;
		UIHelper.EnableGameObject(OpenedRoot, flag);
		UIHelper.EnableGameObject(NotYetRoot, !flag);
		UIHelper.SetWidgetGrayScale(BossImageTexture, !flag);
		bool flag2 = !flag && GetCloseType() == EEventDungeonCloseType.Lock;
		UIHelper.EnableGameObject(RootLock, flag2);
		UIHelper.EnableGameObject(NameLabel, !flag2);
		UIHelper.EnableGameObject(NamePrefixLabel, !flag2);
		UIHelper.EnableGameObject(ButtonLargeEnter, flag && _isOneButton);
		UIHelper.EnableGameObject(RootDisableLargeEnter, !flag && _isOneButton);
		UIHelper.EnableGameObject(ButtonSmallEnter, flag && !_isOneButton);
		UIHelper.EnableGameObject(ButtonHelpPopup, flag && !_isOneButton);
		UIHelper.EnableGameObject(RootDisableSmallEnter, !flag && !_isOneButton);
		UIHelper.EnableGameObject(RootDisableHelpPopup, !flag && !_isOneButton);
		if (!flag)
		{
			UIHelper.SetLabelText(NotOpenLabel, notOpenText);
		}
	}

	protected void InitRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		int rewardIndex = GetRewardIndex();
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		int chapterIndex = GetChapterIndex();
		int dungeonIndex = GetDungeonIndex();
		bool getMark = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardListGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					int itemMin = rewardElementData.ItemMin;
					Color? color = null;
					int num = itemMin;
					itemIconComponent.SetItemCount(num, color);
					itemIconComponent.SetGetMark(getMark);
					_rewardList.Add(itemIconComponent);
				}
			}
		}
		RewardListGrid.RepositionGrid();
	}

	protected void SetProps(EventDungeonPropType[] propTypes)
	{
		bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(propTypes);
		UIHelper.EnableGameObject(RootProp, flag);
		if (flag)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(propTypes); i++)
			{
				EventDungeonPropType prop = propTypes[i];
				SetProp(prop);
			}
		}
	}

	private void SetProp(EventDungeonPropType propType)
	{
		switch (propType)
		{
		case EventDungeonPropType.Special:
			UIHelper.EnableGameObject(RootPropSpecial, enable: true);
			break;
		case EventDungeonPropType.Stamina:
		{
			if (_iconProxy == null)
			{
				if (_isOneButton)
				{
					_iconProxy = IconProxy.Create(WidgetPropStaminaOneButton);
				}
				else
				{
					_iconProxy = IconProxy.Create(WidgetPropStamianTwoButton);
				}
			}
			int chapterIndex = GetChapterIndex();
			int dungeonIndex = GetDungeonIndex();
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data != null)
			{
				StaminaType reqStaminaType = data.ReqStaminaType;
				if (_iconProxy != null)
				{
					_iconProxy.SetupStamina(reqStaminaType);
				}
				string notOpenText = "";
				bool flag = IsOpenDungeon(out notOpenText);
				WidgetPropStaminaOneButton.isGrayscale = !flag;
				UIHelper.EnableGameObject(WidgetPropStaminaOneButton, enable: false);
				UIHelper.EnableGameObject(WidgetPropStaminaOneButton, enable: true);
			}
			break;
		}
		}
	}

	public virtual void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this);
		}
	}

	public void OnClickHelpPopup()
	{
		string[] helpTexts = GetHelpTexts();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(helpTexts))
		{
			EventGuidePopup.Open(helpTexts);
		}
	}
}

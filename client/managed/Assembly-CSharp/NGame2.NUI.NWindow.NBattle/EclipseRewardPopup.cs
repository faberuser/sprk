using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NEclipse;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class EclipseRewardPopup : BaseRewardPopup, EclipseRewardAnim.IListener
{
	public interface IListener
	{
		void ProgressNextTeam();

		void ShowEclipseBattleRecord();
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Battle/EclipseRewardPopup";

	public new static PrefabAuto<EclipseRewardPopup> PrefabAuto = new PrefabAuto<EclipseRewardPopup>("Resources/Prefab2/Window/Battle/EclipseRewardPopup");

	public UILabel Label_CurrentWave;

	public UILabel Label_BestWave;

	public UILabel UseEclipseKeyCountLabel;

	public UILabel NoAdvanceWaveDescLabel;

	public GameObject NewRecord_Reward;

	public GameObject NewRecord_CurrentWave;

	public GameObject NewRecord_BestWave;

	public Transform NextTeamButtonTransform;

	public Transform BattleRecordButtonTransform;

	public Transform RewardInfoWidgetTransform;

	public Transform TitleRootTransform;

	public UIGame3DWidget BGEffect;

	public EclipseRewardAnim EclipseRewardAnim;

	public int MaxItemCountPerLine;

	public float TitleYPosition;

	private int _bestWave;

	private bool _newRecord;

	private bool _endEclipse;

	private int _waveIndex;

	private string EffectCode_RedEnd = "UI.EclipsDungeon.BG.RedEnd";

	private IListener _iListener;

	private List<RewardElementData> RewardElementDataList
	{
		get
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return null;
			}
			int num = _waveIndex - 1;
			if (num <= 0)
			{
				return null;
			}
			if (!GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.CheckAdvanceWave(num))
			{
				return null;
			}
			return GameRoot.Instance.EclipseRewardDataContainer.GetWaveRangeRewardList(num);
		}
	}

	public override void OnClickBackground()
	{
	}

	public static BaseRewardPopup OpenEclipseRewardPopup(PopupOpenData popupOpen, bool isGlobal, Action onClose = null, IListener iListener = null)
	{
		if (popupOpen.CalcRewardCount() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return null;
		}
		EclipseRewardPopup eclipseRewardPopup = PrefabAuto.Factory.Create(ParentTranform(isGlobal));
		if (eclipseRewardPopup == null)
		{
			return null;
		}
		eclipseRewardPopup.Init(iListener, endEclipse: true);
		eclipseRewardPopup.ShowOnlyReward(popupOpen.ShowOnlyReward);
		VespaCoroutine.Start(eclipseRewardPopup.coPlayBGEffect(popupOpen, isGlobal, onClose));
		return eclipseRewardPopup;
	}

	public static void Open(IListener iListener, int waveIndex, bool endEclipse, bool isGlobal = false)
	{
		EclipseRewardPopup eclipseRewardPopup = PrefabAuto.Factory.Create(ParentTranform(isGlobal));
		if (!(eclipseRewardPopup == null))
		{
			eclipseRewardPopup.OpenInternal(iListener, waveIndex, endEclipse, isGlobal);
		}
	}

	private void OpenInternal(IListener iListener, int waveIndex, bool endEclipse, bool isGlobal)
	{
		Init(iListener, endEclipse, waveIndex);
		VespaCoroutine.Start(coPlayBGEffect(null, isGlobal));
	}

	private static Transform ParentTranform(bool isGlobal)
	{
		if (isGlobal)
		{
			return GlobalUIRoot.instance.ContentsGroup.transform;
		}
		return GameUIRoot.instance.ContentsGroup.transform;
	}

	public void Init(IListener iListener, bool endEclipse, int waveIndex = 0)
	{
		_iListener = iListener;
		_endEclipse = endEclipse;
		_waveIndex = waveIndex;
		if (EclipseRewardAnim != null)
		{
			EclipseRewardAnim.Open(this);
		}
		PlayBGEffect();
		SetEclipseRewardInfo();
		DisableEclipseState();
		UIHelper.EnableGameObject(TitleRootTransform, enable: false);
	}

	private IEnumerator coPlayBGEffect(PopupOpenData popupOpen, bool isGlobal, Action onClose = null)
	{
		while (!BGEffect.ISDONE())
		{
			yield return null;
		}
		OpenWithData(popupOpen, isGlobal, onClose, !_endEclipse);
		if (!_endEclipse)
		{
			SetRewardList();
			yield break;
		}
		UIHelper.EnableGameObject(RewardInfoWidgetTransform, enable: true);
		SoundStock stock = new SoundStock
		{
			Code = "UI.Eclipse.End"
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_rewardList);
	}

	private void SetTitle(bool noAdvanceWave)
	{
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("CAMPAIGN_REWARD"));
		if (TitleLabel != null)
		{
			TitleLabel.transform.localPosition = Vector3.zero;
		}
		if (_endEclipse)
		{
			SetUseTicketCountLabel();
		}
		else
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return;
			}
			EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
			if (eclipseInfo != null)
			{
				if (noAdvanceWave)
				{
					UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("ECLIPSE_TEAM_BATTLE_FAIL_TITLE", eclipseInfo.DeckIndex));
					UIHelper.EnableGameObject(NoAdvanceWaveDescLabel, enable: true);
				}
				else
				{
					int num = _waveIndex - 1;
					UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("ECLIPSE_TEAM_BATTLE_FINISH", eclipseInfo.DeckIndex, (num <= 0) ? 1 : num));
				}
			}
		}
	}

	private void SetUseTicketCountLabel()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo() != null)
		{
			int enterTicketCount = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo().EnterTicketCount;
			UIHelper.SetLabelText(UseEclipseKeyCountLabel, LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_END_TICKET_COUNT", enterTicketCount));
			bool flag = enterTicketCount > 1 && _endEclipse;
			UIHelper.EnableGameObject(UseEclipseKeyCountLabel, flag);
			if (TitleLabel != null && flag)
			{
				TitleLabel.transform.localPosition = new Vector3(0f, TitleYPosition, 0f);
			}
		}
	}

	private void SetRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		if (RewardItemGrid == null)
		{
			return;
		}
		List<RewardElementData> rewardElementDataList = RewardElementDataList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardElementDataList))
		{
			return;
		}
		RewardElementData rewardElementData = null;
		CampaignRewardComponent campaignRewardComponent = null;
		for (int i = 0; i < rewardElementDataList.Count; i++)
		{
			rewardElementData = rewardElementDataList[i];
			if (rewardElementData != null)
			{
				campaignRewardComponent = null;
				if (!rewardElementData.ItemCode.IsNullOrEmpty())
				{
					campaignRewardComponent = GetItemRewardComponent(rewardElementData);
				}
				if (rewardElementData.GetCurrencyType() != CurrencyType.None)
				{
					campaignRewardComponent = GetCurrencyRewardComponent(rewardElementData);
				}
				if (!(campaignRewardComponent == null))
				{
					_rewardList.Add(campaignRewardComponent);
				}
			}
		}
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	private CampaignRewardComponent GetItemRewardComponent(RewardElementData rewardData)
	{
		if (rewardData == null)
		{
			return null;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardData.ItemCode);
		if (dataByCode == null)
		{
			return null;
		}
		CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
		if (campaignRewardComponent == null)
		{
			return null;
		}
		_itemAnimationContainer.Add(campaignRewardComponent.Open(dataByCode, rewardData.ItemMax, 0, 0, 0, null, displayInvenCount: false, displayBoosterInfo: false, isFIrstClearReward: false, isBestTeam: false));
		return campaignRewardComponent;
	}

	private CampaignRewardComponent GetCurrencyRewardComponent(RewardElementData rewardData)
	{
		if (rewardData == null)
		{
			return null;
		}
		CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
		if (campaignRewardComponent == null)
		{
			return null;
		}
		ItemAnimationContainer itemAnimationContainer = _itemAnimationContainer;
		CurrencyType currencyType = rewardData.GetCurrencyType();
		int itemMax = rewardData.ItemMax;
		string empty = string.Empty;
		CurrencyIconComponent.Listener listener = null;
		int num = 0;
		string fieldName = empty;
		int num2 = itemMax;
		itemAnimationContainer.Add(campaignRewardComponent.Open(currencyType, num2, fieldName, num, listener));
		return campaignRewardComponent;
	}

	protected override List<ItemResultInfo> MergeItemInfo(ItemResultInfo[] items)
	{
		List<ItemResultInfo> list = new List<ItemResultInfo>();
		foreach (ItemResultInfo itemInfo in items)
		{
			if (itemInfo != null)
			{
				ItemResultInfo itemResultInfo = list.Find((ItemResultInfo find) => find.ItemIndex == itemInfo.ItemIndex && find.IsFirstClearReward == itemInfo.IsFirstClearReward);
				if (itemResultInfo == null)
				{
					itemResultInfo = new ItemResultInfo
					{
						ItemIndex = itemInfo.ItemIndex,
						NewCount = itemInfo.NewCount,
						AddCount = itemInfo.AddCount,
						AddBoosterCount = itemInfo.AddBoosterCount,
						AddNPCBoosterCount = itemInfo.AddNPCBoosterCount,
						AddBonusAssignedItemPercent = itemInfo.AddBonusAssignedItemPercent,
						IsFirstClearReward = itemInfo.IsFirstClearReward
					};
					list.Add(itemResultInfo);
				}
				else
				{
					itemResultInfo.NewCount = itemInfo.NewCount;
					itemResultInfo.AddCount += itemInfo.AddCount;
					itemResultInfo.AddBoosterCount += itemInfo.AddBoosterCount;
					itemResultInfo.AddNPCBoosterCount += itemInfo.AddNPCBoosterCount;
					itemResultInfo.AddBonusAssignedItemPercent += itemInfo.AddBonusAssignedItemPercent;
				}
			}
		}
		return list;
	}

	protected override void SetBattleType(out BattleType battleType)
	{
		battleType = BattleType.Eclipse;
	}

	public void PlayBGEffect()
	{
		TransformData transformData = new TransformData();
		transformData.Position = new Vector3(0f, 0f, 0f);
		transformData.RotationEuler = new Vector3(0f, 0f, 0f);
		transformData.Scale = 112f;
		string effectCode = (_endEclipse ? EffectCode_RedEnd : EclipseMain.EffectCode_RedMoon);
		BGEffect.PlayInstantEffect(effectCode, transformData, null, playSync: true);
	}

	private void SetEclipseRewardInfo()
	{
		EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
		if (eclipseRewardInfo != null)
		{
			EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
			if (eclipseInfo != null)
			{
				int labels = (_endEclipse ? eclipseRewardInfo.MaxWaveIndex : _waveIndex);
				_bestWave = ((eclipseRewardInfo.MaxWaveIndex > eclipseInfo.MaxWaveIndex) ? eclipseRewardInfo.MaxWaveIndex : eclipseInfo.MaxWaveIndex);
				_newRecord = eclipseRewardInfo.MaxWaveIndex > eclipseInfo.MaxWaveIndex;
				SetNewRecords();
				SetLabels(labels);
			}
		}
	}

	private void SetNewRecords()
	{
		UIHelper.EnableGameObject(NewRecord_Reward, _newRecord);
		UIHelper.EnableGameObject(NewRecord_CurrentWave, _newRecord);
		UIHelper.EnableGameObject(NewRecord_BestWave, _newRecord);
	}

	private void SetLabels(int waveIndex)
	{
		string text = LocalizationManager.GetStringDirect(_endEclipse ? "CONTENT_ECLIPSE_FINISH_END_WAVE_RECORD" : "CONTENT_ECLIPSE_FINISH_END_WAVE_TITLE", waveIndex);
		string text2 = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_FINISH_END_WAVE_RECORD", _bestWave);
		if (waveIndex <= 0)
		{
			text = "-";
		}
		if (_bestWave <= 0)
		{
			text2 = "-";
		}
		UIHelper.SetLabelText(Label_CurrentWave, text);
		UIHelper.SetLabelText(Label_BestWave, text2);
	}

	private void DisableEclipseState()
	{
		UIHelper.EnableGameObject(RewardInfoWidgetTransform, enable: false);
		UIHelper.EnableGameObject(UseEclipseKeyCountLabel, enable: false);
		UIHelper.EnableGameObject(NextTeamButtonTransform, enable: false);
		UIHelper.EnableGameObject(BattleRecordButtonTransform, enable: false);
		UIHelper.EnableGameObject(NoAdvanceWaveDescLabel, enable: false);
	}

	private void ReOpenEclipseMain()
	{
		Close();
		SceneAutoBehaviour<SoulWeaponManagement>.instance.ReOpenEclipseMain();
	}

	public void OnClickExitEclipseReward()
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
		{
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
		}
		else
		{
			ReOpenEclipseMain();
		}
	}

	public void OnClickNextTeam()
	{
		if (_iListener != null)
		{
			_iListener.ProgressNextTeam();
		}
	}

	public void OnClickEclipseBattleRecord()
	{
		if (_iListener != null)
		{
			_iListener.ShowEclipseBattleRecord();
		}
	}

	public override void Reposition()
	{
		base.Reposition();
		if (!(RewardItemGrid == null) && !(RewardItemGrid.ScrollView == null) && !(RewardItemGrid.Grid == null))
		{
			int gridChildCount = RewardItemGrid.Grid.GetGridChildCount();
			if (MaxItemCountPerLine <= gridChildCount)
			{
				RewardItemGrid.ScrollView.contentPivot = UIWidget.Pivot.Left;
				RewardItemGrid.ResetScroll();
			}
		}
	}

	public override void Close()
	{
		base.Close();
		if (RewardItemGrid != null && RewardItemGrid.ScrollView != null)
		{
			RewardItemGrid.ScrollView.contentPivot = UIWidget.Pivot.Center;
			RewardItemGrid.ResetScroll();
		}
	}

	void EclipseRewardAnim.IListener.StartShowTitle()
	{
		SetTitle(__Extension_Method_ICollection__.IsNullOrEmpty(RewardElementDataList));
		UIHelper.EnableGameObject(TitleRootTransform, enable: true);
	}

	void EclipseRewardAnim.IListener.FinishAppear()
	{
		UIHelper.EnableGameObject(NextTeamButtonTransform, !_endEclipse);
		UIHelper.EnableGameObject(BattleRecordButtonTransform, _endEclipse);
	}
}

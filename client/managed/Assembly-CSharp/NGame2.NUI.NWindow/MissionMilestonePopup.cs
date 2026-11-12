using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContents;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NGameRoot;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow.Mission;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MissionMilestonePopup : BaseWindow, Hero3DViewTexture.Listener
{
	public delegate void NoticeOpenMissionPopup();

	public delegate void NoticeCloseMissionPopup();

	public delegate void NoticeRewardSkip();

	public interface Listener
	{
		void Reward(MissionComponentData data, Action complete, float openDelayTime);

		void Move(string moveTaskType, string[] moveTaskValue, AchievementReqType type = AchievementReqType.None);

		void ResponseReward();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MissionMilestone/MissionMilestonePopup";

	private static GlobalWindowPrefab<MissionMilestonePopup> _globalWindowPrefab;

	private static WindowPrefab<MissionMilestonePopup> _windowPrefab;

	[Header("[MissionPopup]")]
	public Color EventRewardCountLabelColor = Color.green;

	public UISimpleGrid RewardListGrid;

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UILabel RequireLabel;

	public UISprite EventSprite;

	public UISprite BoosterSprite;

	public UIButton RewardButton;

	public UIButton MoveButton;

	public UISprite CompleteSprite;

	public UISprite RequireSprite;

	public WindowAnimation WindowAnimation;

	public UISprite DescBgSprite;

	public UIScrollGrid DescScrollGrid;

	public GameObject OpenConditionObject;

	public UILabel OpenConditionLabel;

	public BoxCollider BoxColliderBody;

	public UICreatureWidget Creature;

	public GameObject GoSpeechBubble;

	public TweenAlpha TweenSpeechBubble;

	public UILabel LabelSpeech;

	private bool Availabled;

	private bool Cleared;

	private bool Completed;

	private bool Waiting;

	private Listener _iListener;

	private float RewardDelayTime;

	private bool RewardDelaySkip;

	private NPCData _npcData;

	private bool _isTutorialProgress;

	private const float DefaultCreatureWidgetScale = 800f;

	protected List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	protected List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	public static NoticeOpenMissionPopup noticeOpenMissionPopup;

	public static NoticeCloseMissionPopup noticeCloseMissionPopup;

	public static NoticeRewardSkip noticeRewardSkip;

	public MissionComponentData ElementData { get; private set; }

	public bool RewardSkip
	{
		get
		{
			return RewardDelaySkip;
		}
		set
		{
			if (RewardDelaySkip != value)
			{
				RewardDelaySkip = value;
				if (RewardDelaySkip && noticeRewardSkip != null)
				{
					noticeRewardSkip();
				}
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (!_isTutorialProgress)
		{
			string arg = EnumTools.GetName(GameUIRoot.ELayerType.UI3D);
			int layerDepth = GenericLayerManager<GameUIRoot.ELayerType>.GetLayerDepth(GameUIRoot.ELayerType.UI3D);
			GlobalCameraManager.SetCameraDepth($"{arg} Camera", layerDepth);
		}
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
		if (noticeCloseMissionPopup != null)
		{
			noticeCloseMissionPopup();
		}
	}

	public void Open2()
	{
		Open(delegate
		{
			CreateCreature();
		}, delegate
		{
			SetNpcOpenAction();
			if (noticeOpenMissionPopup != null)
			{
				noticeOpenMissionPopup();
			}
			RunOpenCloseAnimation(reversse: false, null);
			if (DescScrollGrid != null)
			{
				DescScrollGrid.RepositionGrid();
			}
		});
		Waiting = false;
		if (BoxColliderBody != null)
		{
			BoxColliderBody.enabled = Waiting;
		}
		RewardDelayTime = 0f;
		RewardSkip = false;
		if (!_isTutorialProgress)
		{
			string arg = EnumTools.GetName(GameUIRoot.ELayerType.UI3D);
			int layerDepth = GenericLayerManager<GlobalUIRoot.ELayerType>.GetLayerDepth(GlobalUIRoot.ELayerType.Global3D);
			GlobalCameraManager.SetCameraDepth($"{arg} Camera", layerDepth);
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (Creature != null)
		{
			Creature.Close();
		}
		if (_npcData != null)
		{
			_npcData = null;
		}
		if (_isTutorialProgress)
		{
			MissionMain instance = SceneAutoBehaviour<MissionManagement>.instance.GetInstance<MissionMain>(openedOnly: true);
			if (instance != null)
			{
				instance.SetVisibleHeroComponent(visible: true);
			}
		}
		_isTutorialProgress = false;
	}

	public static void ReceiveReward()
	{
		if (_windowPrefab != null && _windowPrefab.IsValidInstance && _windowPrefab.instance.IsOpen())
		{
			_windowPrefab.instance.SetNpcRewardAction();
		}
	}

	public static void ReleaseWindow()
	{
		if (_globalWindowPrefab != null)
		{
			_globalWindowPrefab.ReleaseWindow();
		}
	}

	public void CloseWithAnimation(Action onClose)
	{
		ActionClose(onClose);
	}

	private void ActionClose(Action onClose)
	{
		RunOpenCloseAnimation(reversse: true, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			Close();
		});
	}

	private IEnumerator RewardActionClose(MissionComponentData data, Action complete)
	{
		if (!SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			ActionClose(null);
			yield break;
		}
		MissionMain instance = SceneAutoBehaviour<MissionManagement>.instance.GetInstance<MissionMain>();
		if (instance == null)
		{
			ActionClose(null);
			yield break;
		}
		RewardDelayTime = 0f;
		RewardSkip = false;
		Waiting = true;
		if (BoxColliderBody != null)
		{
			BoxColliderBody.enabled = Waiting;
		}
		float nPCRewardActionTime = instance.GetNPCRewardActionTime();
		float mdfDelayTime = nPCRewardActionTime + 0.2f;
		if (_iListener == null)
		{
			ActionClose(null);
			yield break;
		}
		_iListener.Reward(data, complete, mdfDelayTime);
		while (RewardDelayTime < mdfDelayTime && (!RewardSkip || !(RewardDelayTime >= 1f)))
		{
			RewardDelayTime += Time.deltaTime;
			yield return null;
		}
		yield return null;
		Waiting = false;
		if (BoxColliderBody != null)
		{
			BoxColliderBody.enabled = Waiting;
		}
		ActionClose(null);
	}

	public static MissionMilestonePopup OpenGlobal(Listener listener, MissionComponentData elementData, bool Completed, bool Available)
	{
		if (_globalWindowPrefab == null)
		{
			_globalWindowPrefab = new GlobalWindowPrefab<MissionMilestonePopup>("Resources/Prefab2/Window/MissionMilestone/MissionMilestonePopup", GlobalUIRoot.ELayerType.GlobalUI);
		}
		_globalWindowPrefab.instance.OpenInternal(listener, elementData, Completed, Available, isTutorialProgress: false);
		return _globalWindowPrefab.instance;
	}

	public static MissionMilestonePopup OpenNormal(Listener listener, MissionComponentData elementData, bool Completed, bool Available)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<MissionMilestonePopup>("Resources/Prefab2/Window/MissionMilestone/MissionMilestonePopup", GameUIRoot.ELayerType.UI3DUI);
		}
		_windowPrefab.instance.OpenInternal(listener, elementData, Completed, Available, isTutorialProgress: true);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
		return _windowPrefab.instance;
	}

	public void OpenInternal(Listener listener, MissionComponentData elementData, bool Completed, bool Available, bool isTutorialProgress)
	{
		_isTutorialProgress = isTutorialProgress;
		_iListener = listener;
		Availabled = Available;
		ElementData = elementData;
		if (ElementData != null)
		{
			Open2();
			SetupMission();
		}
	}

	private void RunOpenCloseAnimation(bool reversse, Action onPlayed)
	{
		if (!(WindowAnimation == null))
		{
			WindowAnimation.PlayAnimation(reversse, onPlayed);
		}
	}

	public void SetupHeroTexture(int heroIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex) == null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + heroIndex);
			}
			else
			{
				new CachedHeroInfo(data);
			}
		}
	}

	void Hero3DViewTexture.Listener.OnClick()
	{
	}

	protected void SetTitleLabel(string text)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, text);
		}
	}

	protected void SetDescLabel(string text)
	{
		if (!(DescLabel == null))
		{
			UIHelper.SetLabelText(DescLabel, text);
			UIHelper.EnableGameObject(DescBgSprite, !String_Extension_Method.IsNullOrEmpty(text));
		}
	}

	protected void SetRequireLabel(string text)
	{
		if (!(RequireLabel == null))
		{
			UIHelper.SetLabelText(RequireLabel, text);
		}
	}

	protected bool SetOpenConditionLabel(string text)
	{
		if (OpenConditionObject == null)
		{
			return false;
		}
		if (OpenConditionLabel == null)
		{
			return false;
		}
		UIHelper.EnableGameObject(OpenConditionObject, enable: false);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return false;
		}
		if (ElementData == null)
		{
			return false;
		}
		if (ElementData.VisualData == null)
		{
			return false;
		}
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(ElementData.VisualData.MainCategoryIndex, ElementData.VisualData.SubCategoryIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.IsNodeMission())
		{
			return false;
		}
		if (ElementData.GetData<AchievementData>() != null)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(ElementData.Index) != null)
			{
				return false;
			}
		}
		else
		{
			if (ElementData.GetData<SubQuestData>() == null)
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(ElementData.GetData<SubQuestData>()) != QuesetStatusType.Lock)
			{
				return false;
			}
		}
		UIHelper.EnableGameObject(OpenConditionObject, enable: true);
		UIHelper.SetLabelText(OpenConditionLabel, text);
		return true;
	}

	protected void SetOpenConditionLabelCanMove(string text)
	{
		if (OpenConditionObject == null || OpenConditionLabel == null)
		{
			return;
		}
		UIHelper.EnableGameObject(OpenConditionObject, enable: false);
		if (!String_Extension_Method.IsNullOrEmpty(text) && ElementData != null && ElementData.VisualData != null)
		{
			SubQuestData data = ElementData.GetData<SubQuestData>();
			bool flag = false;
			switch (data.MoveTaskType)
			{
			case ClientActivityType.MoveToNode:
			{
				int valueInt = ArgParser.GetValueInt(data.MoveTaskValue, 0);
				int valueInt2 = ArgParser.GetValueInt(data.MoveTaskValue, 1);
				flag = !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(valueInt, valueInt2);
				break;
			}
			case ClientActivityType.OpenRaid:
			case ClientActivityType.OpenWorldBoss:
			case ClientActivityType.OpenEclipse:
			case ClientActivityType.OpenGodkingTrial:
			case ClientActivityType.OpenChallengeRaid:
				flag = !IsCanMoveContents(data);
				break;
			}
			if (flag)
			{
				UIHelper.EnableGameObject(OpenConditionObject, enable: true);
				UIHelper.SetLabelText(OpenConditionLabel, text);
			}
		}
	}

	protected virtual void SetupMission()
	{
		SetTitleLabel(ElementData.Title);
		SetDescLabel(ElementData.Desc);
		SetRequireLabel(ElementData.GoalRequirement);
		bool flag = SetOpenConditionLabel(ElementData.OpenConditionText);
		Completed = false;
		Cleared = false;
		bool flag2 = false;
		bool flag3 = true;
		if (ElementData.GetData<AchievementData>() != null)
		{
			AchievementData data = ElementData.GetData<AchievementData>();
			Completed = AchievementChecker.Completed(ElementData.Index);
			Cleared = AchievementChecker.Cleared(ElementData.Index);
			flag2 = !String_Extension_Method.IsNullOrEmpty(data.MoveTaskType);
			CommonMission.SetupAchievementReward(data, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, EventRewardCountLabelColor);
			UIHelper.EnableGameObject(RequireSprite, Completed || Cleared);
			EnableObjectsAchievement(flag2, Availabled, Completed, Cleared);
		}
		else if (ElementData.GetData<SubQuestData>() != null)
		{
			SubQuestData data2 = ElementData.GetData<SubQuestData>();
			QuesetStatusType subQuestStatus = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data2);
			Completed = subQuestStatus == QuesetStatusType.Complete;
			Cleared = subQuestStatus == QuesetStatusType.Reward;
			flag2 = data2.MoveTaskType != ClientActivityType.None;
			flag3 = IsCanMoveContents(data2);
			if (!flag)
			{
				SetOpenConditionLabelCanMove(ElementData.OpenConditionText);
			}
			CommonMission.SetupSubQuestReward(data2, null, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid);
			UIHelper.EnableGameObject(RequireSprite, Completed || Cleared);
			EnableObjectsSubQuest(flag2, Availabled, subQuestStatus, flag3);
		}
		RewardListGrid.RepositionGrid();
	}

	protected bool IsCanMoveContents(SubQuestData subQuestData)
	{
		switch (subQuestData.MoveTaskType)
		{
		case ClientActivityType.OpenRaid:
		case ClientActivityType.OpenChallengeRaid:
			return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Raid);
		case ClientActivityType.OpenWorldBoss:
			return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.WorldBoss);
		case ClientActivityType.OpenEclipse:
			return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Eclipse);
		case ClientActivityType.OpenGodkingTrial:
			return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.GodkingTrial);
		default:
			return true;
		}
	}

	private void EnableObjectsAchievement(bool MoveTaskExist, bool Acailabled, bool Complete, bool Cleared)
	{
		if (MoveTaskExist)
		{
			UIHelper.EnableButton(MoveButton, !Complete && !Cleared, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, !Complete && Cleared, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, Complete);
			UIHelper.EnableGameObject(RewardButton, !Complete);
		}
		else
		{
			UIHelper.EnableButton(MoveButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, !Complete && Cleared, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, Complete);
			UIHelper.EnableGameObject(RewardButton, !Complete);
		}
		if (!Availabled)
		{
			UIHelper.EnableButton(MoveButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, enable: false);
			UIHelper.EnableGameObject(RewardButton, enable: true);
		}
	}

	private void EnableObjectsSubQuest(bool MoveTaskExist, bool Acailabled, QuesetStatusType Status, bool IsCanMove)
	{
		if (MoveTaskExist)
		{
			UIHelper.EnableButton(MoveButton, Status == QuesetStatusType.Progress && IsCanMove, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, Status == QuesetStatusType.Reward, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, Status == QuesetStatusType.Complete);
			UIHelper.EnableGameObject(RewardButton, Status != QuesetStatusType.Complete);
		}
		else
		{
			UIHelper.EnableButton(MoveButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, Status == QuesetStatusType.Reward, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, Status == QuesetStatusType.Complete);
			UIHelper.EnableGameObject(RewardButton, Status != QuesetStatusType.Complete);
		}
		if (!Availabled)
		{
			UIHelper.EnableButton(MoveButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(RewardButton, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(CompleteSprite, enable: false);
			UIHelper.EnableGameObject(RewardButton, enable: true);
		}
	}

	private void CreateCreature()
	{
		if (!_isTutorialProgress || Creature == null)
		{
			return;
		}
		NPCData popupNpcData = GetPopupNpcData(ElementData.VisualData);
		CachedHeroInfo popupNpcHeroInfo = GetPopupNpcHeroInfo(popupNpcData);
		bool flag = popupNpcData != null && popupNpcHeroInfo != null;
		MissionMain instance = SceneAutoBehaviour<MissionManagement>.instance.GetInstance<MissionMain>(openedOnly: true);
		if (!(instance == null))
		{
			instance.SetVisibleHeroComponent(visible: false);
			_npcData = ((!flag) ? instance.GetNpcData() : popupNpcData);
			CachedHeroInfo cachedHeroInfo = ((!flag) ? instance.GetHeroInfo() : popupNpcHeroInfo);
			Creature.OpneMission(cachedHeroInfo);
			Creature.SetCustomIdle(DefineCreature.EMotion.EmotiMotion, "EventIdle");
			if (_npcData.NpcScale > 1)
			{
				float num = 800f * ((float)_npcData.NpcScale / 100f);
				Creature.PivotTm.localScale = Vector3.one * num;
			}
		}
	}

	private NPCData GetPopupNpcData(MissionVisualData visualData)
	{
		if (visualData == null || String_Extension_Method.IsNullOrEmpty(visualData.MilestonePopupNpcCode))
		{
			return null;
		}
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(visualData.MilestonePopupNpcCode);
		if (data != null)
		{
			return data;
		}
		return null;
	}

	private CachedHeroInfo GetPopupNpcHeroInfo(NPCData npcData)
	{
		if (npcData == null)
		{
			return null;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(npcData.CreatureIndex);
		if (data == null)
		{
			return null;
		}
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(data);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.Star = (sbyte)npcData.CreatureStar;
			cachedHeroInfo.CostumeIndex = npcData.CreatureCostumeIndex;
			return cachedHeroInfo;
		}
		return null;
	}

	public void SetNpcOpenAction()
	{
		if (_isTutorialProgress && _npcData != null)
		{
			SetNpcAction(_npcData.OnOpenMissionPopup);
		}
	}

	public void SetNpcRewardAction()
	{
		if (_isTutorialProgress && _npcData != null)
		{
			SetNpcAction(_npcData.OnGetMissionReward);
		}
	}

	private void SetNpcAction(string[] npcActions)
	{
		if (_isTutorialProgress && !__Extension_Method_ICollection__.IsNullOrEmpty(npcActions) && !(Creature == null))
		{
			EntryMenuManagement.GetActionValues(npcActions, out var speech, out var _, out var motion);
			if (!String_Extension_Method.IsNullOrEmpty(speech))
			{
				SetSpeech(speech);
				Creature.SetEmotiAction(motion);
			}
		}
	}

	private void SetSpeech(string speech)
	{
		UIHelper.EnableGameObject(GoSpeechBubble, enable: true);
		UIHelper.SetLabelText(LabelSpeech, speech);
		if (TweenSpeechBubble != null)
		{
			TweenSpeechBubble.ResetToBeginning();
			TweenSpeechBubble.PlayForward();
			TweenSpeechBubble.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(GoSpeechBubble, enable: false);
			});
		}
	}

	public void OnClickClose()
	{
		if (Waiting)
		{
			RewardSkip = true;
		}
		else
		{
			ActionClose(null);
		}
	}

	public void OnClickPopupBody()
	{
		if (Waiting)
		{
			RewardSkip = true;
		}
	}

	public void OnClickReward()
	{
		if (ElementData == null || _iListener == null || Waiting)
		{
			return;
		}
		if (IsSpecialMissionTimeOut())
		{
			ActionClose(null);
			return;
		}
		if (ElementData.GetData<AchievementData>() != null)
		{
			AchievementData data = ElementData.GetData<AchievementData>();
			if (data.RewardType == AchievementRewardType.Booster)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.RewardCode);
				if (dataByCode != null)
				{
					string stringDirect = LocalizationManager.GetStringDirect("DAILYMISSION_FREE_BOOSTER_POPUP_NAME");
					string stringDirect2 = LocalizationManager.GetStringDirect("DAILYMISSION_FREE_BOOSTER_POPUP_DESC", dataByCode.Name);
					MessagePopup.Open(stringDirect, stringDirect2, delegate
					{
						_iListener.Reward(ElementData, ResponseReward, 0f);
					}, null);
					ActionClose(null);
				}
				return;
			}
		}
		CurrencyMaxCap.WarningOrExecute(CommonMission.GetCurrencyTypesFromReward(ElementData), delegate
		{
			VespaCoroutine.Start(RewardActionClose(ElementData, ResponseReward));
		});
	}

	private void ResponseReward()
	{
		if (_iListener != null)
		{
			_iListener.ResponseReward();
		}
	}

	public void OnClickMove()
	{
		if (!Availabled || Completed || Waiting || _iListener == null)
		{
			return;
		}
		ActionClose(delegate
		{
			if (ElementData.GetData<AchievementData>() != null)
			{
				AchievementData data = ElementData.GetData<AchievementData>();
				_iListener.Move(data.MoveTaskType, data.MoveTaskValue, data.ReqType);
			}
			else if (ElementData.GetData<SubQuestData>() != null)
			{
				SubQuestData data2 = ElementData.GetData<SubQuestData>();
				_iListener.Move(data2.MoveTaskType.ToString(), data2.MoveTaskValue);
			}
		});
	}

	private bool IsSpecialMissionTimeOut()
	{
		if (ElementData == null || ElementData.VisualData == null)
		{
			return false;
		}
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(ElementData.VisualData.MainCategoryIndex, ElementData.VisualData.SubCategoryIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.IsAnySpecialMission())
		{
			return false;
		}
		return SingletonAuto<MissionStatus>.instance.IsMissionTimeout(data);
	}
}

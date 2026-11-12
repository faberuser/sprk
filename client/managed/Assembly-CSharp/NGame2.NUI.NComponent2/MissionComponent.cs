using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NLogin;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[RequireComponent(typeof(UIWidget))]
public class MissionComponent : MonoBehaviour, NVespa.NPrefab2.InitOnCreate, CurrencyIconComponent.Listener
{
	public enum ENodeState
	{
		Normal,
		Disable,
		Selected,
		Lock
	}

	public enum EStateIconType
	{
		Reward_Progress,
		Reward_Clear,
		Check_Progress,
		Check_Clear,
		Unconfirmed,
		DirectMove
	}

	public enum MissionBordType
	{
		QuickBord,
		Etc
	}

	public enum MissionIcon
	{
		Mission_Main,
		Mission_Event,
		Mission_Etc
	}

	public interface IListner
	{
		void OnClick(MissionComponentData data);

		void RequestReward(MissionComponentData data);

		void RequestMove(string moveTaskType, string[] moveTaskValue);
	}

	[Serializable]
	public class ProgressBarNormal
	{
		public Transform Root;

		public UILabel Text;

		public UIClipSprite Bar;

		public UILabel CompleteText;

		public UISprite CompleteBar;

		public void SetValue(long Achieved, long Requirement)
		{
			float num = Mathf.Clamp01((float)Achieved / (float)Requirement);
			if (num < 1f)
			{
				UIHelper.EnableGameObject(Text, enable: true);
				UIHelper.EnableGameObject(Bar, enable: true);
				UIHelper.SetLabelText(Text, Achieved + "/" + Requirement);
				UIHelper.SetSpriteFillAmount(Bar, num);
				UIHelper.EnableGameObject(CompleteBar, enable: false);
				UIHelper.EnableGameObject(CompleteText, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(Text, enable: false);
				UIHelper.EnableGameObject(Bar, enable: false);
				UIHelper.EnableGameObject(CompleteBar, enable: true);
				UIHelper.EnableGameObject(CompleteText, enable: true);
				UIHelper.SetLabelText(CompleteText, Requirement + "/" + Requirement);
			}
		}
	}

	[Serializable]
	public class ProgressBarRemainTime : ProgressBarNormal
	{
		public UILabel RemainTime;
	}

	[Serializable]
	public class DetailDisplayUI
	{
		public ProgressBarNormal BarNormal;

		public ProgressBarRemainTime BarRemainTime;

		public UILabel DetailText;

		public void SetRemainTimeString(string remainTimeString)
		{
			UIHelper.SetLabelText(BarRemainTime.RemainTime, remainTimeString);
		}

		private void DisableAll()
		{
			UIHelper.EnableGameObject(BarNormal.Root, enable: false);
			UIHelper.EnableGameObject(BarRemainTime.Root, enable: false);
			UIHelper.EnableGameObject(DetailText, enable: false);
		}

		private void SetupDetailText(MissionVisualData visualData, long achieved, long requirement)
		{
			UIHelper.EnableGameObject(BarNormal.Root, enable: false);
			UIHelper.EnableGameObject(BarRemainTime.Root, enable: false);
			UIHelper.EnableGameObject(DetailText, enable: true);
			if (__Extension_Method_ICollection__.COUNT(visualData.DetailDisplayLocalize) == 0)
			{
				return;
			}
			string key = visualData.DetailDisplayLocalize[0];
			if (achieved == 0L && __Extension_Method_ICollection__.COUNT(visualData.DetailDisplayLocalize) >= 2)
			{
				string text = visualData.DetailDisplayLocalize[1];
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					key = text;
				}
			}
			if (achieved == requirement && __Extension_Method_ICollection__.COUNT(visualData.DetailDisplayLocalize) >= 3)
			{
				string text2 = visualData.DetailDisplayLocalize[2];
				if (!String_Extension_Method.IsNullOrEmpty(text2))
				{
					key = text2;
				}
			}
			string stringDirect = LocalizationManager.GetStringDirect(key, achieved, requirement);
			UIHelper.SetLabelText(DetailText, stringDirect);
		}

		public void Setup(MissionComponentData elementData, bool isRemainTime)
		{
			if (elementData == null)
			{
				DisableAll();
				return;
			}
			long curProgress;
			long requirement;
			if (elementData.GetData<AchievementData>() != null)
			{
				SingletonAuto<AchievementChecker>.instance.GetAchievementAndRequirement(elementData.Index, out curProgress, out requirement, out var _);
				if (AchievementChecker.Completed(elementData.Index))
				{
					requirement = curProgress;
				}
			}
			else
			{
				if (elementData.GetData<SubQuestData>() == null)
				{
					elementData.GetData<WebContentInfo>();
					DisableAll();
					return;
				}
				QuesetStatusType subQuestStatus = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(elementData.GetData<SubQuestData>());
				SubQuestInfo subQuestInfoByQuestIndex = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(elementData.Index);
				if (subQuestInfoByQuestIndex != null)
				{
					curProgress = subQuestInfoByQuestIndex.Progress;
					requirement = elementData.GetData<SubQuestData>().GetReqProgress();
				}
				else
				{
					curProgress = 0L;
					requirement = elementData.GetData<SubQuestData>().GetReqProgress();
				}
				if (subQuestStatus == QuesetStatusType.Complete)
				{
					curProgress = requirement;
				}
			}
			MissionVisualData missionVisualDataByQuestIndex = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataByQuestIndex(elementData.Index);
			if (missionVisualDataByQuestIndex != null && __Extension_Method_ICollection__.COUNT(missionVisualDataByQuestIndex.DetailDisplayLocalize) > 0)
			{
				SetupDetailText(missionVisualDataByQuestIndex, curProgress, requirement);
				return;
			}
			ProgressBarNormal progressBarNormal;
			ProgressBarNormal progressBarNormal2;
			if (!isRemainTime)
			{
				progressBarNormal = BarNormal;
				progressBarNormal2 = BarRemainTime;
			}
			else
			{
				progressBarNormal = BarRemainTime;
				progressBarNormal2 = BarNormal;
			}
			UIHelper.EnableGameObject(progressBarNormal.Root, enable: true);
			UIHelper.EnableGameObject(progressBarNormal2.Root, enable: false);
			UIHelper.EnableGameObject(DetailText, enable: false);
			progressBarNormal.SetValue(curProgress, requirement);
		}
	}

	public const string PrefabPath = "Prefab2/Component/QuestIntegration/MissionListComponent";

	public static PrefabAuto<MissionComponent> PrefabAuto = new PrefabAuto<MissionComponent>("Prefab2/Component/QuestIntegration/MissionListComponent");

	[Header("[CommonMission]")]
	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UISprite NodeSprite;

	public GameObject DisableObject;

	public GameObject SelectedObject;

	public UISpriteSelector SpriteSelectorStateIcon;

	public UISimpleGrid RewardListGrid;

	protected List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	protected List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	protected ENodeState eNodeState;

	protected EStateIconType eRewardState;

	protected IListner iListner;

	[Header("[MissionComponent]")]
	public int GridHeight = 200;

	public UISpriteSelector MissionIconSelector;

	public UISprite Sprite_BgMissionMain;

	public UISprite Sprite_BgEvent;

	public Color TitleMainMissionColor;

	public Color TitleEventMissionColor;

	public Color TitleNormalColor;

	[Header("--<Effect>")]
	public Transform RewardEffectTransform;

	[Header("--<DetailDisplayUI>")]
	public DetailDisplayUI DetailDisplay;

	[Header("--<Lock Icon>")]
	public Transform LockIconTransform;

	private bool IsHeroAchievement;

	private bool IsRemainTime;

	private MissionBordType missionBordType = MissionBordType.Etc;

	private int missionCompValue;

	private bool NotAvailable;

	private float remainUpdateTime;

	private bool WebEventTimeOut;

	private bool SpecialEventTimeOut;

	public MissionComponentData ElementData { get; set; }

	public Transform _ProgressTransform => DetailDisplay.BarNormal.Root;

	public UILabel _Label_QuestProgress => DetailDisplay.BarNormal.Text;

	public UIClipSprite _Sprite_QuestProgressBar => DetailDisplay.BarNormal.Bar;

	public UILabel _Label_CompleteProgress => DetailDisplay.BarNormal.CompleteText;

	public UISprite _Sprite_CompleteProgress => DetailDisplay.BarNormal.CompleteBar;

	public Transform _RemainTimeProgressBarTransform => DetailDisplay.BarRemainTime.Root;

	public UILabel _Label_RemainProgress => DetailDisplay.BarRemainTime.Text;

	public UIClipSprite _Sprite_RemainProgressBar => DetailDisplay.BarRemainTime.Bar;

	public UISprite _Sprite_RemainCompleteProgress => DetailDisplay.BarRemainTime.CompleteBar;

	public UILabel _Label_RemainCompleteProgress => DetailDisplay.BarRemainTime.CompleteText;

	public UILabel _Label_RemainTime => DetailDisplay.BarRemainTime.RemainTime;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		Clear();
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void ExpansionOpen(params object[] args)
	{
		if (args != null && args.Length != 0)
		{
			MissionComponentData data = args[0] as MissionComponentData;
			IListner listner = args[1] as IListner;
			NotAvailable = !Convert.ToBoolean(args[2]);
			missionCompValue = 0;
			missionBordType = MissionBordType.Etc;
			Open(data, listner);
		}
	}

	public void ExpansionQuickOpen(params object[] args)
	{
		if (args != null && args.Length != 0)
		{
			MissionComponentData data = args[0] as MissionComponentData;
			IListner listner = args[1] as IListner;
			missionCompValue = Convert.ToInt32(args[2]);
			missionBordType = MissionBordType.QuickBord;
			NotAvailable = false;
			Open(data, listner);
		}
	}

	public void ExpansionHeroOpen(params object[] args)
	{
		if (args != null && args.Length != 0)
		{
			MissionComponentData data = args[0] as MissionComponentData;
			IListner listner = args[1] as IListner;
			IsHeroAchievement = true;
			missionCompValue = 0;
			missionBordType = MissionBordType.Etc;
			NotAvailable = false;
			Open(data, listner);
		}
	}

	public void Open(MissionComponentData data, IListner listner)
	{
		ElementData = data;
		iListner = listner;
		Init();
	}

	public void Init()
	{
		SetDrawInfos();
	}

	public void Update()
	{
		remainUpdateTime += Time.time;
		if (remainUpdateTime > 1f)
		{
			remainUpdateTime = 0f;
			if (!IsHeroAchievement)
			{
				SetupLabel();
			}
		}
	}

	protected void Clear()
	{
		eNodeState = ENodeState.Normal;
		NormalState();
		NotAvailable = false;
		IsHeroAchievement = false;
		missionCompValue = 0;
		missionBordType = MissionBordType.Etc;
	}

	private void SetTitleLabel(string text)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, text);
		}
	}

	protected void SetState(ENodeState state)
	{
		if (eNodeState != state)
		{
			eNodeState = state;
			DoState();
		}
	}

	protected void DoState()
	{
		switch (eNodeState)
		{
		case ENodeState.Normal:
			NormalState();
			break;
		case ENodeState.Disable:
			DisableState();
			break;
		case ENodeState.Selected:
			SelectedState();
			break;
		case ENodeState.Lock:
			LockState();
			break;
		}
	}

	protected void NormalState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(LockIconTransform, enable: false);
	}

	protected void DisableState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(LockIconTransform, enable: false);
	}

	protected void SelectedState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: true);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(LockIconTransform, enable: false);
	}

	protected void LockState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(LockIconTransform, enable: true);
	}

	protected void SetStateIcon(EStateIconType type, bool isGrayScale = false)
	{
		if (eRewardState != type)
		{
			eRewardState = type;
			if (SpriteSelectorStateIcon != null)
			{
				SpriteSelectorStateIcon.SelectEnum(eRewardState);
				SpriteSelectorStateIcon.GetTargetSprite().isGrayscale = isGrayScale;
			}
		}
	}

	protected void EnableStateIcon(bool isEnable)
	{
		if (SpriteSelectorStateIcon != null)
		{
			UIHelper.EnableGameObject(SpriteSelectorStateIcon, isEnable);
		}
	}

	private void SetupAchievementReward()
	{
		if (ElementData != null)
		{
			AchievementData data = ElementData.GetData<AchievementData>();
			if (data != null)
			{
				FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
				FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
				CommonMission.SetupAchievementReward(data, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, Color.green);
				UIHelper.RepositionGrid(RewardListGrid);
			}
		}
	}

	private void SetupSubQuestReward()
	{
		if (ElementData != null)
		{
			SubQuestData data = ElementData.GetData<SubQuestData>();
			if (data != null)
			{
				FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
				FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
				CommonMission.SetupSubQuestReward(data, this, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid);
				UIHelper.RepositionGrid(RewardListGrid);
			}
		}
	}

	public void SetupRewardList()
	{
		if (ElementData != null)
		{
			if (ElementData.GetData<AchievementData>() != null)
			{
				SetupAchievementReward();
			}
			else if (ElementData.GetData<SubQuestData>() != null)
			{
				SetupSubQuestReward();
			}
			UIHelper.EnableGameObject(RewardListGrid.gameObject, (RewardListGrid != null && __Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0) || __Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0);
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
	}

	public void OnClick()
	{
		if (!NotAvailable && !WebEventTimeOut && (!IsRemainTime || !SpecialEventTimeOut))
		{
			SaveOpenLocalInfo();
			if (iListner != null)
			{
				iListner.OnClick(ElementData);
			}
		}
	}

	public void SaveOpenLocalInfo()
	{
		if (ElementData.GetData<WebContentInfo>() == null || LoginData.WebEvent == null || LoginData.WebEvent.WebContents == null)
		{
			return;
		}
		WebContentInfo[] webContents = LoginData.WebEvent.WebContents;
		foreach (WebContentInfo webContentInfo in webContents)
		{
			if (!SingletonAuto<PlayerPrefsOpenHistoryData>.instance.IsOpened_WebContent(webContentInfo.Index, webContentInfo.BeginTime))
			{
				SingletonAuto<PlayerPrefsOpenHistoryData>.instance.SetOpen_WebContent(webContentInfo.Index, webContentInfo.BeginTime);
			}
		}
	}

	private void SetDrawInfos()
	{
		if (ElementData == null || ElementData.Data == null)
		{
			return;
		}
		if (IsHeroAchievement)
		{
			AchievementData data = ElementData.GetData<AchievementData>();
			if (data != null)
			{
				AchievementTextCombine instance = SingletonAuto<AchievementTextCombine>.instance;
				UIHelper.SetLabelText(text: ((AchievementComponent.ITextProvider)instance).GetAchievementName(data), label: TitleLabel);
				string achievementDesc = ((AchievementComponent.ITextProvider)instance).GetAchievementDesc(data);
				UIHelper.SetLabelText(DescLabel, achievementDesc);
			}
			else
			{
				SetupLabel();
			}
		}
		else
		{
			SetupLabel();
		}
		if (MissionIconSelector != null)
		{
			MissionIcon value = MissionIcon.Mission_Etc;
			if (missionCompValue == 1)
			{
				value = MissionIcon.Mission_Main;
			}
			else if (missionCompValue == 2)
			{
				value = MissionIcon.Mission_Event;
			}
			MissionIconSelector.SelectEnum(value);
		}
		UIHelper.EnableGameObject(Sprite_BgMissionMain, missionCompValue == 1);
		UIHelper.EnableGameObject(Sprite_BgEvent, missionCompValue == 2);
		if (missionCompValue == 1)
		{
			UIHelper.SetLabelColor(TitleLabel, TitleMainMissionColor);
		}
		else if (missionCompValue == 2)
		{
			UIHelper.SetLabelColor(TitleLabel, TitleEventMissionColor);
		}
		else
		{
			UIHelper.SetLabelColor(TitleLabel, TitleNormalColor);
		}
		SetState(ENodeState.Normal);
		EnableStateIcon(isEnable: true);
		EnableRewardParticle(enable: false);
		SetStateIcon(EStateIconType.Reward_Progress);
		bool flag = true;
		if (ElementData.GetData<AchievementData>() != null)
		{
			AchievementData data2 = ElementData.GetData<AchievementData>();
			if (AchievementChecker.Completed(ElementData.Index))
			{
				SetState(ENodeState.Disable);
				SetStateIcon(EStateIconType.Reward_Progress);
			}
			else if (AchievementChecker.Cleared(ElementData.Index))
			{
				SetStateIcon(EStateIconType.Reward_Clear);
			}
			else
			{
				bool flag2 = !String_Extension_Method.IsNullOrEmpty(data2.MoveTaskType);
				SetStateIcon(flag2 ? EStateIconType.DirectMove : EStateIconType.Reward_Progress);
			}
		}
		else if (ElementData.GetData<SubQuestData>() != null)
		{
			SubQuestData data3 = ElementData.GetData<SubQuestData>();
			switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data3))
			{
			case QuesetStatusType.Progress:
			{
				bool flag3 = data3.MoveTaskType != ClientActivityType.None;
				SetStateIcon(flag3 ? EStateIconType.DirectMove : EStateIconType.Reward_Progress);
				break;
			}
			case QuesetStatusType.Reward:
				SetStateIcon(EStateIconType.Reward_Clear);
				break;
			case QuesetStatusType.Complete:
				SetState(ENodeState.Disable);
				SetStateIcon(EStateIconType.Reward_Progress);
				break;
			default:
				SetState(ENodeState.Lock);
				SetStateIcon(EStateIconType.Reward_Progress);
				break;
			}
		}
		else if (ElementData.GetData<WebContentInfo>() != null)
		{
			WebContentInfo data4 = ElementData.GetData<WebContentInfo>();
			if (!SingletonAuto<PlayerPrefsOpenHistoryData>.instance.IsOpened_WebContent(data4.Index, data4.BeginTime))
			{
				SetStateIcon(EStateIconType.Unconfirmed);
			}
			else
			{
				bool isComplete = data4.IsComplete;
				bool hasAReward = data4.HasAReward;
				SetStateIcon(hasAReward ? EStateIconType.Reward_Clear : (isComplete ? EStateIconType.Check_Clear : EStateIconType.Check_Progress));
			}
			flag = false;
		}
		else if (ElementData.GetData<AttendanceData>() != null)
		{
			SetStateIcon(EStateIconType.Check_Progress);
			flag = false;
		}
		else if (ElementData.GetData<BoosterInfo>() != null)
		{
			SetStateIcon(EStateIconType.Check_Progress);
			flag = false;
		}
		else
		{
			VespaDebug.LogError("MissionComp Not Found Data.");
		}
		if (NotAvailable)
		{
			SetState(ENodeState.Lock);
		}
		DetailDisplay.Setup(ElementData, IsRemainTime);
		if (flag)
		{
			SetupRewardList();
		}
		else
		{
			UIHelper.EnableGameObject(RewardListGrid.gameObject, enable: false);
		}
	}

	private void SetupLabel()
	{
		if (ElementData != null)
		{
			UIHelper.SetLabelText(TitleLabel, ElementData.MissionComponentTitle);
			UIHelper.SetLabelText(DescLabel, ElementData.Goal);
			if (ElementData.GetData<WebContentInfo>() != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(ElementData.GetData<WebContentInfo>().EndTime, out var o_dateTime);
				int num = Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
				WebEventTimeOut = num <= 0;
			}
			else if (ElementData.GetData<AttendanceData>() != null)
			{
				WebEventTimeOut = String_Extension_Method.IsNullOrEmpty(ElementData.Goal);
			}
			else if (ElementData.GetData<BoosterInfo>() != null)
			{
				WebEventTimeOut = String_Extension_Method.IsNullOrEmpty(ElementData.Goal);
			}
			else
			{
				WebEventTimeOut = false;
			}
			IsRemainTime = SetupRemainTime();
		}
	}

	private bool SetupRemainTime()
	{
		if (ElementData == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			return false;
		}
		if (ElementData.VisualData == null)
		{
			return false;
		}
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(ElementData.VisualData.MainCategoryIndex, ElementData.VisualData.SubCategoryIndex);
		if (!SingletonAuto<MissionStatus>.instance.TryGetRemainTime(data, out var remainTimeSpan))
		{
			return false;
		}
		string remainTimeString_Short = SingletonAuto<MissionStatus>.instance.GetRemainTimeString_Short(remainTimeSpan);
		DetailDisplay.SetRemainTimeString(remainTimeString_Short);
		SpecialEventTimeOut = remainTimeSpan <= TimeSpan.Zero;
		return true;
	}

	public void EnableRewardParticle(bool enable)
	{
		if (eRewardState != EStateIconType.Reward_Clear)
		{
			UIHelper.EnableGameObject(RewardEffectTransform, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(RewardEffectTransform, enable);
		}
	}

	public void OnClickReward()
	{
		if (ElementData == null)
		{
			return;
		}
		if (ElementData.GetData<WebContentInfo>() != null)
		{
			OnClick();
		}
		else
		{
			if (iListner == null || eNodeState == ENodeState.Lock)
			{
				return;
			}
			if (eRewardState == EStateIconType.Reward_Clear)
			{
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
								iListner.RequestReward(ElementData);
							}, null);
						}
						return;
					}
				}
				iListner.RequestReward(ElementData);
			}
			else if (eRewardState == EStateIconType.DirectMove)
			{
				if (ElementData.GetData<AchievementData>() != null)
				{
					AchievementData data2 = ElementData.GetData<AchievementData>();
					iListner.RequestMove(data2.MoveTaskType, data2.MoveTaskValue);
				}
				else if (ElementData.GetData<SubQuestData>() != null)
				{
					SubQuestData data3 = ElementData.GetData<SubQuestData>();
					iListner.RequestMove(data3.MoveTaskType.ToString(), data3.MoveTaskValue);
				}
			}
		}
	}
}

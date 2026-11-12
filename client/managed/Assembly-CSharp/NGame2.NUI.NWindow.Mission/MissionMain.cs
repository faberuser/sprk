using System;
using NGame2.NAccount;
using NGame2.NFunctionality;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionMain : FrameControlWindow, MissionCategoryPanel.IListener
{
	public interface IListener
	{
	}

	public enum EQuestType
	{
		Daily = 1,
		Weekly,
		Limited,
		Main,
		Sub,
		End
	}

	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/MissionMain";

	[Header("[QuestIntegrationMain]")]
	public MissionCategoryPanel CategoryMenuPanel;

	public MissionListPanel MissionListPanel;

	public HeroInUIComponent HeroComponent;

	public Transform UI3DRoot;

	public Transform UI3DUIRoot;

	private IListener _iListener;

	private string _npcCode;

	private ISound _npcSound;

	private NPCData _NPCData;

	private DateTime _NPCTouchedTime = DateTime.MinValue;

	private float _NPCActionDelaySecond = 3f;

	private CachedHeroInfo _cachedHeroInfo;

	private void OnEnable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Combine(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
	}

	private void OnDisable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Remove(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
	}

	public override void Reset()
	{
		base.Reset();
		if (HeroComponent != null)
		{
			HeroComponent.Clear();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_npcSound != null)
		{
			_npcSound.Update();
		}
	}

	public void OpenMain(System.Action onOpen, System.Action onComplete, IListener listener, string npcCode, int mainCategory, int subCategory)
	{
		Open(onOpen, onComplete);
		_npcCode = npcCode;
		_NPCData = GameRoot.Instance.NPCDataContainer.GetData(_npcCode);
		if (_NPCData == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(_NPCData.CreatureIndex);
		if (cachedHeroInfo == null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_NPCData.CreatureIndex);
			if (data == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + _NPCData.CreatureIndex);
				return;
			}
			cachedHeroInfo = new CachedHeroInfo(data);
		}
		_cachedHeroInfo = cachedHeroInfo;
		CreatureInUIVisualData visualData = new CreatureInUIVisualData(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Normal"), CreatureStock.ELoadLevel.IncludeFacial);
		HeroComponent.Open(cachedHeroInfo, isPlaySound: true, visualData, delegate
		{
			SetNPCWelcomeAction(playSound: true);
		});
		Init(listener, mainCategory, subCategory);
	}

	public CachedHeroInfo GetHeroInfo()
	{
		return _cachedHeroInfo;
	}

	public NPCData GetNpcData()
	{
		return _NPCData;
	}

	public override void Close()
	{
		base.Close();
		SceneAutoBehaviour<MissionManagement>.instance.ChangeTopMostMenuLayer(GameUIRoot.ELayerType.UI);
	}

	public void SetVisibleHeroComponent(bool visible)
	{
		UIHelper.EnableGameObject(HeroComponent, visible);
	}

	private void Init(IListener iListener, int mainCategory, int subCategory = 0)
	{
		_iListener = iListener;
		SceneAutoBehaviour<MissionManagement>.instance.ChangeTopMostMenuLayer(GameUIRoot.ELayerType.UI3DUI);
		InitCategoryMenuPanel();
		InitQuestIntegrationListPanel();
		if (mainCategory == 0)
		{
			SelectDefaultCategory();
		}
		else
		{
			SelectMainCategory(mainCategory, subCategory);
		}
	}

	public void SelectDefaultCategory()
	{
		SelectMainCategory(2);
	}

	private void SetupTitlleKey(int MainCategoryIndex)
	{
		MissionCategoryData missionCategoryDataHead = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(MainCategoryIndex);
		if (missionCategoryDataHead != null)
		{
			title.Key = missionCategoryDataHead.MainCategoryLocalKey;
		}
	}

	private void SelectMainCategory(int mainCategory, int subCategory = 0)
	{
		MissionCategoryData missionCategoryDataHead = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(mainCategory);
		if (missionCategoryDataHead != null)
		{
			if (CategoryMenuPanel != null)
			{
				CategoryMenuPanel.SetupToggle(missionCategoryDataHead.MainCategoryIndex);
			}
			MissionListPanel.SelectCategory(missionCategoryDataHead.MainCategoryIndex, subCategory);
			SetupTitlleKey(missionCategoryDataHead.MainCategoryIndex);
		}
	}

	private void InitCategoryMenuPanel()
	{
		if (!(CategoryMenuPanel == null))
		{
			CategoryMenuPanel.Init(this);
		}
	}

	private void InitQuestIntegrationListPanel()
	{
		if (!(MissionListPanel == null))
		{
			MissionListPanel.Init();
		}
	}

	public void Refresh()
	{
		if (CategoryMenuPanel != null)
		{
			CategoryMenuPanel.Refresh();
		}
	}

	public void EnableMainMissionUI(bool enable)
	{
		UIHelper.EnableGameObject(UI3DRoot, enable);
		UIHelper.EnableGameObject(UI3DUIRoot, enable);
	}

	private void NoticeOpenMissionPopup()
	{
		OnOpenMissionPopupNPC();
	}

	public void ResponseReward()
	{
		OnGetMissionRewardNPC();
	}

	private void OnGetMissionRewardNPC()
	{
		if (_NPCData != null)
		{
			DateTime utcNow = DateTime.UtcNow;
			_NPCTouchedTime = utcNow;
			ExecNPCAction(_NPCData.OnGetMissionReward, playSound: true);
		}
	}

	private void OnOpenMissionPopupNPC()
	{
		if (_NPCData != null)
		{
			DateTime utcNow = DateTime.UtcNow;
			_NPCTouchedTime = utcNow;
			ExecNPCAction(_NPCData.OnOpenMissionPopup, playSound: true);
		}
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
	}

	public void OnClickNPC()
	{
		if (_NPCData != null)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (!((utcNow - _NPCTouchedTime).TotalSeconds < (double)_NPCActionDelaySecond))
			{
				_NPCTouchedTime = utcNow;
				ExecNPCAction(_NPCData.OnTouch, playSound: true);
			}
		}
	}

	private void SetNPCWelcomeAction(bool playSound)
	{
		if (_NPCData != null && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.EntryMenuNpcWelcomAction, value: true))
		{
			ExecNPCAction(_NPCData.OnOpenEntryMenu, playSound);
			_NPCTouchedTime = DateTime.UtcNow;
		}
	}

	private void ExecNPCAction(string[] actionValues, bool playSound)
	{
		SetNPCAction(actionValues, playSound);
	}

	private void SetNPCAction(string[] ActionValues, bool playsound = true)
	{
		EntryMenuManagement.GetActionValues(ActionValues, out var speech, out var sound, out var motion);
		SetNPCAction(speech, playsound ? sound : null, motion);
	}

	private void SetNPCAction(string speech, string sound = null, string motion = null)
	{
		if (!String_Extension_Method.IsNullOrEmpty(speech))
		{
			if (HeroComponent != null)
			{
				HeroComponent.SetSpeech(speech);
			}
			if (!String_Extension_Method.IsNullOrEmpty(sound))
			{
				StopNPCSound();
				SoundStock stock = new SoundStock
				{
					Code = sound,
					LocalTimer = GlobalTimer.DefaultLocal
				};
				_npcSound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
			}
			if (!String_Extension_Method.IsNullOrEmpty(motion))
			{
				HeroComponent.SetMotion(motion, isLoop: false);
			}
		}
	}

	private void StopNPCSound()
	{
		if (_npcSound != null)
		{
			SoundContainer.ReleaseSound(_npcSound);
			_npcSound = null;
		}
	}

	private void MotionNpc(RenderFacade npcRenderFacade, string motionName, bool isLoop = false)
	{
		if (!(npcRenderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, motionName);
			stockData.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			npcRenderFacade.Action.Motion(stockData, null);
		}
	}

	public float GetNPCRewardActionTime()
	{
		if (HeroComponent == null)
		{
			return 1f;
		}
		if (_NPCData == null)
		{
			return 1f;
		}
		EntryMenuManagement.GetActionValues(_NPCData.OnGetMissionReward, out var _, out var _, out var motion);
		return HeroComponent.GetNPCActionTime(motion);
	}

	void MissionCategoryPanel.IListener.SelectCategoryMenu(MissionCategoryData data)
	{
		if (MissionListPanel != null)
		{
			MissionListPanel.SelectCategory(data.MainCategoryIndex);
			title.Key = data.MainCategoryLocalKey;
		}
	}

	public void ChangedQuestListView()
	{
		if (CategoryMenuPanel != null)
		{
			CategoryMenuPanel.Refresh();
		}
	}
}

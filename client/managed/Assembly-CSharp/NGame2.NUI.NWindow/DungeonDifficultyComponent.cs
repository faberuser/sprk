using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DungeonDifficultyComponent : MonoBehaviour, IInspectorGuiDrawer, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class PopupButtonUiData
	{
		public bool IsScenarioButton;

		public DungeonDifficultyType DifficultyType;

		public UIButton Button;

		public UIWidget NewMarkWidget;

		public UIWidget LockedWidget;
	}

	public UIWidget IconRoot;

	public IconProxy IconProxy;

	public UISpriteSelector DifficultyBgSelector;

	public GameObject StaminaRoot;

	public UILabel StaminaLabel;

	public UILabel DifficultyLabel;

	public UIWidget DifficultyNewMark;

	public UIButton CurrentDifficultyButton;

	public UISprite Sprite_Dir;

	public UIGrid PopupButtonGrid;

	public UIPopupRoot uIPopupRoot;

	[HideInInspector]
	public List<PopupButtonUiData> PopupButtonList;

	private DungeonDifficultyType _difficultyType;

	private Vector3 DifficultyLabelDefaultPosition;

	private bool OpenDifficulty;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public DungeonDifficultyType DifficultyType
	{
		get
		{
			if (PlayScenario && GameRoot.isValidInstance && GameRoot.Instance.CampaignWaveDataContainer != null)
			{
				CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(ChapterIndex, DungeonIndex);
				VespaDebug.LogError($"[Error] CampaignWaveData not found! {ChapterIndex},{DungeonIndex}");
				return waveCollection?.MinDifficulty ?? _difficultyType;
			}
			return _difficultyType;
		}
	}

	public bool PlayScenario { get; private set; }

	public bool IsScenarioReplay
	{
		get
		{
			if (!PlayScenario)
			{
				return false;
			}
			if (!AccountManager.IsLogined())
			{
				return false;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(ChapterIndex, DungeonIndex);
		}
	}

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

	private void Awake()
	{
		DifficultyLabelDefaultPosition = DifficultyLabel.transform.localPosition;
	}

	public void Init(int chapterIndex, int dungeonIndex)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(chapterIndex, dungeonIndex);
		OpenDifficulty = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex).OpenDifficulty;
		_difficultyType = (DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
		PlayScenario = flag2 && !flag;
		RefreshButton();
	}

	public bool OnClickDifficultyButton(GameObject senderObject)
	{
		if (senderObject == null)
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData popupButtonUiData = PopupButtonList[i];
			if (popupButtonUiData != null && !(popupButtonUiData.Button.gameObject != senderObject))
			{
				if (popupButtonUiData.IsScenarioButton)
				{
					return SelectDifficulyScenario();
				}
				return SelectDifficuly(popupButtonUiData.DifficultyType);
			}
		}
		return false;
	}

	public bool SelectDifficuly(DungeonDifficultyType difficulty)
	{
		if (!PlayScenario && _difficultyType == difficulty)
		{
			return false;
		}
		int maxAvailableDifficulty = GetMaxAvailableDifficulty();
		if ((int)difficulty > maxAvailableDifficulty)
		{
			return false;
		}
		_difficultyType = difficulty;
		PlayScenario = false;
		SingletonAuto<NGame2.NAccount.UserCampaignManager.ChapterDungeonInfo>.instance.SetChapterDifficulty(ChapterIndex, DifficultyType);
		if (DifficultyType > NGame2.NAccount.UserCampaignManager.GetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex))
		{
			NGame2.NAccount.UserCampaignManager.SetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex, DifficultyType);
		}
		RefreshButton();
		return true;
	}

	public bool SelectDifficulyScenario()
	{
		GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(ChapterIndex, DungeonIndex);
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(ChapterIndex, DungeonIndex))
		{
			return false;
		}
		PlayScenario = true;
		RefreshButton();
		return true;
	}

	private void RefreshButton()
	{
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(ChapterIndex, DungeonIndex);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(ChapterIndex, DungeonIndex);
		UIHelper.EnableGameObject(StaminaRoot, enable: true);
		if (num && !flag)
		{
			RefreshButtonScenarioOnly();
		}
		else
		{
			RefreshButtonNormal();
		}
	}

	private void RefreshButtonScenarioOnly()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return;
		}
		DifficultyBgSelector.Select("Scenario");
		UIHelper.SetLabelText(StaminaLabel, data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), DifficultyType).ToString());
		SetupIconStamina(data.BattleType, data.ReqStaminaType);
		if ((bool)IconProxy)
		{
			if (data.ReqStaminaType == StaminaType.None)
			{
				UIHelper.EnableGameObject(StaminaLabel, enable: false);
				DifficultyLabel.alignment = NGUIText.Alignment.Center;
				DifficultyLabel.pivot = UIWidget.Pivot.Center;
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(DifficultyLabel.transform);
			}
			else
			{
				UIHelper.EnableGameObject(StaminaLabel, enable: true);
				DifficultyLabel.alignment = NGUIText.Alignment.Left;
				DifficultyLabel.pivot = UIWidget.Pivot.Left;
				DifficultyLabel.transform.localPosition = DifficultyLabelDefaultPosition;
			}
		}
		PlayScenario = true;
		UIHelper.SetLabelText(DifficultyLabel, LocalizationManager.GetStringDirect("CHAPTERCLEAR_STORY"));
		UIHelper.EnableButton(CurrentDifficultyButton, enable: true);
		UIHelper.EnableGameObject(Sprite_Dir, enable: false);
		UIHelper.EnableGameObject(DifficultyNewMark, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData popupButtonUiData = PopupButtonList[i];
			if (!popupButtonUiData.IsScenarioButton)
			{
				UIHelper.EnableGameObject(popupButtonUiData.Button, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(popupButtonUiData.Button, enable: true);
			UIHelper.EnableButton(popupButtonUiData.Button, enable: true, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(popupButtonUiData.NewMarkWidget, enable: false);
		}
		PopupButtonGrid.Reposition();
		DungeonDifficultyType minDifficulty = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(ChapterIndex, DungeonIndex).MinDifficulty;
		if (minDifficulty > NGame2.NAccount.UserCampaignManager.GetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex))
		{
			NGame2.NAccount.UserCampaignManager.SetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex, minDifficulty);
		}
	}

	private void RefreshButtonNormal()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return;
		}
		if (DifficultyBgSelector != null)
		{
			if (!PlayScenario)
			{
				DifficultyBgSelector.SelectEnum(DifficultyType);
			}
			else
			{
				DifficultyBgSelector.Select("Scenario");
			}
		}
		SetupIconStamina(data.BattleType, data.ReqStaminaType);
		if ((bool)IconProxy)
		{
			if (data.ReqStaminaType == StaminaType.None)
			{
				UIHelper.EnableGameObject(StaminaLabel, enable: false);
				DifficultyLabel.alignment = NGUIText.Alignment.Center;
				DifficultyLabel.pivot = UIWidget.Pivot.Center;
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(DifficultyLabel.transform);
			}
			else
			{
				UIHelper.EnableGameObject(StaminaLabel, enable: true);
				DifficultyLabel.alignment = NGUIText.Alignment.Left;
				DifficultyLabel.pivot = UIWidget.Pivot.Left;
				DifficultyLabel.transform.localPosition = DifficultyLabelDefaultPosition;
			}
		}
		if (IsScenarioReplay)
		{
			UIHelper.SetLabelText(StaminaLabel, "0");
		}
		else
		{
			SaveStaminaBoosterInfo saveStaminaBoosterInfo = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina();
			bool flag = saveStaminaBoosterInfo.IsActiveBattleType(data.BattleType);
			if (data.GetStaminaOfDifficulty(saveStaminaBoosterInfo, DifficultyType) == 0 && !flag)
			{
				UIHelper.EnableGameObject(StaminaRoot, enable: false);
			}
			else
			{
				UIHelper.SetLabelText(StaminaLabel, data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), DifficultyType).ToString());
			}
		}
		if (PlayScenario)
		{
			UIHelper.SetLabelText(DifficultyLabel, LocalizationManager.GetStringDirect("CHAPTERCLEAR_STORY"));
		}
		else
		{
			UIHelper.SetLabelText(DifficultyLabel, UIHelper.DungeonDifficultyLocale(DifficultyType));
		}
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(ChapterIndex, DungeonIndex);
		bool flag3 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleteScenario(ChapterIndex, DungeonIndex);
		DungeonDifficultyType maxDungeonDifficultyType = NGame2.NAccount.UserCampaignManager.GetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex);
		int maxAvailableDifficulty = GetMaxAvailableDifficulty();
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(ChapterIndex, DungeonIndex);
		if (waveCollection == null)
		{
			VespaDebug.LogError($"DungeonDifficultyComponent::RefreshButtonNormal - CampaignWaveData null");
			return;
		}
		bool enable = __Extension_Method_ICollection__.COUNT(waveCollection.DifficultyDic) > 1;
		bool enable2 = maxAvailableDifficulty > (int)maxDungeonDifficultyType;
		if (flag2)
		{
			enable = true;
			if (!flag3)
			{
				enable2 = true;
			}
		}
		UIHelper.EnableButton(CurrentDifficultyButton, enable: true);
		UIHelper.EnableGameObject(Sprite_Dir, enable);
		UIHelper.EnableGameObject(DifficultyNewMark, enable2);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData popupButtonUiData = PopupButtonList[i];
			if (popupButtonUiData.IsScenarioButton)
			{
				if (!flag2)
				{
					UIHelper.EnableGameObject(popupButtonUiData.Button, enable: false);
					continue;
				}
				bool enable3 = !flag3;
				UIHelper.EnableGameObject(popupButtonUiData.Button, enable: true);
				UIHelper.EnableButton(popupButtonUiData.Button, enable: true, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(popupButtonUiData.LockedWidget, enable: false);
				UIHelper.EnableGameObject(popupButtonUiData.NewMarkWidget, enable3);
				continue;
			}
			DungeonDifficultyType difficultyType = popupButtonUiData.DifficultyType;
			if (!waveCollection.DifficultyDic.ContainsKey(difficultyType))
			{
				UIHelper.EnableGameObject(popupButtonUiData.Button, enable: false);
				continue;
			}
			bool flag4 = (int)difficultyType <= maxAvailableDifficulty;
			bool enable4 = flag4 && difficultyType > maxDungeonDifficultyType;
			UIHelper.EnableGameObject(popupButtonUiData.Button, enable: true);
			if (OpenDifficulty)
			{
				UIHelper.EnableButton(popupButtonUiData.Button, enable: true, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(popupButtonUiData.LockedWidget, enable: false);
			}
			else
			{
				UIHelper.EnableButton(popupButtonUiData.Button, flag4, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(popupButtonUiData.LockedWidget, !flag4);
			}
			UIHelper.EnableGameObject(popupButtonUiData.NewMarkWidget, enable4);
		}
		PopupButtonGrid.Reposition();
		if (DifficultyType > NGame2.NAccount.UserCampaignManager.GetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex))
		{
			NGame2.NAccount.UserCampaignManager.SetMaxDungeonDifficultyType(ChapterIndex, DungeonIndex, DifficultyType);
		}
	}

	private int GetMaxAvailableDifficulty()
	{
		if (OpenDifficulty)
		{
			return Helper.GetMaxDifficulty(ChapterIndex, DungeonIndex);
		}
		return Helper.GetMaxAvailableDifficulty(ChapterIndex, DungeonIndex);
	}

	public void HidePopup()
	{
		uIPopupRoot.HidePopup();
	}

	public void CreateIconProxy()
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
	}

	public void SetupIconStamina(BattleType battleType, StaminaType staminaType)
	{
		CreateIconProxy();
		SaveStaminaBoosterInfo saveStaminaBoosterInfo = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina();
		if (IconProxy != null)
		{
			if (staminaType == StaminaType.Chicken && saveStaminaBoosterInfo.IsActiveBattleType(battleType))
			{
				IconProxy.SetupStamina("SaveStamina");
			}
			else
			{
				IconProxy.SetupStamina(staminaType);
			}
		}
	}
}

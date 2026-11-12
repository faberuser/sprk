using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroTranscendSkillListPanel : MonoBehaviour, IHeroPanel, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, HeroTranscendSkillGroupComponent.Listener, ITutorialListener
{
	public interface Listener
	{
		void SelectTranscendSkill(int page, int GroupIndex, int SkillSlotIndex, int SkillIndex);

		void ResetTranscendSkill(int page);

		void SelectTranscendPreset(int heroIndex, int page);

		void AddTranscendPreset(int page);

		void RecommendTranscend(HeroInfo heroInfo, int page);
	}

	[Serializable]
	public class PresetButton
	{
		public UIButton Button;

		public UISprite Select;

		public UIButton Add;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroTranscendSkillListPanel";

	public static PrefabAuto<HeroTranscendSkillListPanel> PrefabAuto = new PrefabAuto<HeroTranscendSkillListPanel>("Resources/Prefab2/Component/Hero/HeroTranscendSkillListPanel");

	public UITable SkillGroupTable;

	public UIButton ResetTranscendSkillButton;

	public UIToggle RecommendTranscendToggle;

	public UIButton RecommendTranscendButton;

	public UILabel TranscendSkillPointLabel;

	public GameObject EditButtonRoot;

	public List<PresetButton> PresetButtonList;

	private int CurrentSelectPage = 1;

	private int MaxPage = 3;

	private Listener _listener;

	private bool _isMine;

	private bool _refreshOnEnable;

	private List<HeroTranscendSkillGroupComponent> _ListTranscendSkillGroup = new List<HeroTranscendSkillGroupComponent>();

	private bool _isEditing;

	private HeroInfo _editingHeroInfo;

	private TranscendSkillPageInfo _originalPageInfo;

	public HeroInfo HeroInfo { get; private set; }

	public void Close()
	{
		ShowRemainTranscendPointAlarm();
		SendPrevSelectedTranscendPage();
		CheckEndEditing();
		base.gameObject.SetActive(value: false);
		SingletonUpdater<TutorialManager>.instance.RemoveListener(this);
	}

	private void OnEnable()
	{
		if (_refreshOnEnable)
		{
			Refresh();
		}
		_refreshOnEnable = false;
	}

	private void Reset()
	{
		CheckEndEditing();
		ClearGroupList();
	}

	private void ClearGroupList()
	{
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
	}

	public void SendPrevSelectedTranscendPage(bool reset = true)
	{
		if (HeroInfo != null && HeroInfo.SkillPage != CurrentSelectPage && _listener != null)
		{
			_listener.SelectTranscendPreset(HeroInfo.HeroIndex, CurrentSelectPage);
		}
		if (reset)
		{
			HeroInfo = null;
			CurrentSelectPage = 1;
		}
	}

	public void ShowRemainTranscendPointAlarm()
	{
		if (HeroInfo != null && _isMine && HeroInfo.IsRemainLearnableTranscendSkill(CurrentSelectPage))
		{
			CenterMessage.Open(LocalizationManager.GetString(HeroTranscend.REMAIN_TRANSCEND_SKILL_POINT, HeroInfo.CreatureData.Name));
		}
	}

	public void Open(HeroInfo heroInfo, Listener listener, bool isMine = true)
	{
		Reset();
		_isMine = isMine;
		ShowRemainTranscendPointAlarm();
		SendPrevSelectedTranscendPage();
		base.gameObject.SetActive(value: true);
		HeroInfo = heroInfo;
		_listener = listener;
		MaxPage = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendHeroSkillPage");
		ChagnePage(HeroInfo.SkillPage);
		UIHelper.EnableButton(RecommendTranscendButton, enable: false);
		int usableTranscendSkillPoint = HeroInfo.GetUsableTranscendSkillPoint(CurrentSelectPage);
		bool flag = SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new EnterHeroTranscendSkillListPage
		{
			IsMine = _isMine,
			HasPoint = (usableTranscendSkillPoint > 0)
		});
		if (isMine)
		{
			if (flag)
			{
				SingletonUpdater<TutorialManager>.instance.AddListener(this);
			}
			else
			{
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenRecommendTranscend());
			}
		}
	}

	private void ChagnePage(int page)
	{
		CheckEndEditing();
		CurrentSelectPage = page;
		UpdatePresetButton(page);
		SetupResetAllTranscendSkillButton(page);
		SetupTranscendSkillGroupList(page);
		UpdateTranscendSkillPoint(page);
		RecommendTranscendContainer instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<RecommendTranscendContainer>(openedOnly: true);
		if (instance != null && instance.IsOpen())
		{
			instance.ChangePageIndex(page);
		}
	}

	private void UpdateTranscendSkillPoint(int page)
	{
		int currentTotalTrancendPoint = HeroInfo.GetCurrentTotalTrancendPoint();
		int usableTranscendSkillPoint = HeroInfo.GetUsableTranscendSkillPoint(page);
		SetTranscendSkillPoint(usableTranscendSkillPoint, currentTotalTrancendPoint);
	}

	private void SetupResetAllTranscendSkillButton(int page)
	{
		UIHelper.EnableButton(ResetTranscendSkillButton, HeroInfo.AnyTranscendSkillLearned(page));
	}

	private void SetupTranscendSkillGroupList(int page)
	{
		ClearGroupList();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("TotalTranscendSkill");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (valueInt2 == 0 || valueInt == 0)
		{
			return;
		}
		int num = Mathf.Min(valueInt2, valueInt);
		for (int i = 1; i <= num; i++)
		{
			if (__Extension_Method_ICollection__.COUNT(HeroInfo.CreatureData.GetTranscendSkill(i)) != 0)
			{
				HeroTranscendSkillGroupComponent heroTranscendSkillGroupComponent = HeroTranscendSkillGroupComponent.PrefabAuto.Factory.Create(SkillGroupTable.transform);
				heroTranscendSkillGroupComponent.Open(HeroInfo, page, i, this, !_isMine);
				_ListTranscendSkillGroup.Add(heroTranscendSkillGroupComponent);
			}
		}
		SkillGroupTable.Reposition();
	}

	public bool IsMaxTranscended()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend") == HeroInfo.Transcended;
	}

	public bool IsMaxTranscendPoint()
	{
		int currentTotalTrancendPoint = HeroInfo.GetCurrentTotalTrancendPoint();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendPoint");
		return currentTotalTrancendPoint >= valueInt;
	}

	public bool HasTranscendPointItem()
	{
		return !__Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.TranscendPoint));
	}

	public void OnClickTranscendPoint()
	{
		if (!IsMaxTranscended())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string message = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_CONFIRM_CONDITION);
			MessagePopup.Open(stringDirect, message, null);
		}
		else if (IsMaxTranscendPoint())
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string message2 = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_NOT_CONFIRM);
			MessagePopup.Open(stringDirect2, message2, null);
		}
		else if (!HasTranscendPointItem())
		{
			string stringDirect3 = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string message3 = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_SHOP_CONFIRM);
			MessagePopup.Open(stringDirect3, message3, delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Growth.ToString());
			}, null);
		}
		else if (HeroInfo is CachedHeroInfo heroInfo)
		{
			UseTpItemSelectPopup.Open(heroInfo);
		}
	}

	public void OnClickResetAllTranscendSkill()
	{
		if (_listener != null)
		{
			_listener.ResetTranscendSkill(CurrentSelectPage);
		}
	}

	public void OnClickRecommendTranscend()
	{
		VespaDebug.LogError("WARNING(yung): 추천 특성 임시로 막아둠");
	}

	public void OnClickSelectPreset1()
	{
		SelectPreset(1);
	}

	public void OnClickSelectPreset2()
	{
		SelectPreset(2);
	}

	public void OnClickSelectPreset3()
	{
		SelectPreset(3);
	}

	public void OnClickSelectPreset4()
	{
		SelectPreset(4);
	}

	public void OnClickSelectPreset5()
	{
		SelectPreset(5);
	}

	public void OnClickSelectPreset6()
	{
		SelectPreset(6);
	}

	public void OnClickAddPreset1()
	{
		OnAddPreset(1);
	}

	public void OnClickAddPreset2()
	{
		OnAddPreset(2);
	}

	public void OnClickAddPreset3()
	{
		OnAddPreset(3);
	}

	public void OnClickAddPreset4()
	{
		OnAddPreset(4);
	}

	public void OnClickAddPreset5()
	{
		OnAddPreset(5);
	}

	public void OnClickAddPreset6()
	{
		OnAddPreset(6);
	}

	private void UpdatePresetButton(int selectPage)
	{
		int openedTranscendSkillPageCount = HeroInfo.GetOpenedTranscendSkillPageCount();
		for (int i = 1; i <= MaxPage; i++)
		{
			PresetButton presetButton = GetPresetButton(i);
			if (HeroInfo.GetTranscendSkillPageInfo(i) != null)
			{
				UIHelper.EnableButton(presetButton.Button, i != selectPage);
				UIHelper.EnableGameObject(presetButton.Button, i != selectPage);
				UIHelper.EnableGameObject(presetButton.Select, i == selectPage);
				UIHelper.EnableGameObject(presetButton.Add, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(presetButton.Select, enable: false);
				if (_isMine)
				{
					UpdatePresetBtnChildForMine(presetButton, i, openedTranscendSkillPageCount);
				}
				else
				{
					UpdatePresetBtnChildForOthers(presetButton);
				}
			}
		}
	}

	private void UpdatePresetBtnChildForMine(PresetButton presetBtn, int page, int count)
	{
		UIHelper.EnableGameObject(presetBtn.Add, enable: true);
		UIHelper.EnableGameObject(presetBtn.Button, enable: false);
		if (page == count + 1)
		{
			UIHelper.EnableButton(presetBtn.Add, enable: true, grayscaleIfDisable: true);
		}
		else
		{
			UIHelper.EnableButton(presetBtn.Add, enable: false, grayscaleIfDisable: true);
		}
	}

	private void UpdatePresetBtnChildForOthers(PresetButton presetBtn)
	{
		UIHelper.EnableGameObject(presetBtn.Add, enable: false);
		UIHelper.EnableGameObject(presetBtn.Button, enable: true);
		UIHelper.EnableButton(presetBtn.Button, enable: false);
	}

	private PresetButton GetPresetButton(int page)
	{
		if (__Extension_Method_ICollection__.COUNT(PresetButtonList) < page)
		{
			return null;
		}
		return PresetButtonList[page - 1];
	}

	public void SelectPreset(int index)
	{
		ChagnePage(index);
	}

	private void OnAddPreset(int index)
	{
		if (_listener != null)
		{
			_listener.AddTranscendPreset(index);
		}
	}

	private void RefreshSkillGroupComponent()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListTranscendSkillGroup); i++)
		{
			HeroTranscendSkillGroupComponent heroTranscendSkillGroupComponent = _ListTranscendSkillGroup[i];
			if (!(heroTranscendSkillGroupComponent == null))
			{
				heroTranscendSkillGroupComponent.Refresh();
			}
		}
	}

	public void Refresh()
	{
		if (!base.gameObject.activeSelf)
		{
			_refreshOnEnable = true;
			return;
		}
		UpdateTranscendSkillPoint(CurrentSelectPage);
		SetupTranscendSkillGroupList(CurrentSelectPage);
	}

	public void SetTranscendSkillPoint(int curr, int max)
	{
		string text = $"{curr:00}/{max:00}";
		UIHelper.SetLabelText(TranscendSkillPointLabel, text);
	}

	void HeroTranscendSkillGroupComponent.Listener.OnSelectComponent(int transcended, int skillSlot, int skillIndex)
	{
		if (_listener != null)
		{
			_listener.SelectTranscendSkill(CurrentSelectPage, transcended, skillSlot, skillIndex);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo info)
	{
		if (info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.TranscendSkillSelected || info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.ResetAllTranscendSkill)
		{
			SetupResetAllTranscendSkillButton(CurrentSelectPage);
			UpdateTranscendSkillPoint(CurrentSelectPage);
			RefreshSkillGroupComponent();
		}
		else if (info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddTranscendSkillPage)
		{
			ChagnePage(info.NewValue);
		}
		else if (info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.ExtendTranscendPoint)
		{
			UpdateTranscendSkillPoint(CurrentSelectPage);
			RefreshSkillGroupComponent();
		}
	}

	void ITutorialListener.OnTutorialBegin(TutorialData tutorialData)
	{
	}

	void ITutorialListener.OnTutorialEnd(int tutorialIndex, Action onEnd)
	{
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenRecommendTranscend());
	}

	public void BeginEditing()
	{
		if (!_isEditing)
		{
			_isEditing = true;
			_editingHeroInfo = HeroInfo;
			UIHelper.EnableGameObject(EditButtonRoot, enable: true);
			TranscendSkillPageInfo transcendSkillPageInfo = HeroInfo.GetTranscendSkillPageInfo(CurrentSelectPage);
			_originalPageInfo = new TranscendSkillPageInfo
			{
				Page = CurrentSelectPage,
				TranscendSkillDatas = transcendSkillPageInfo.TranscendSkillDatas.ConvertAll((CreatureData.TranscendSkill data) => JsonMarshaler.Parse<CreatureData.TranscendSkill>(JsonMarshaler.EncodeJsonString(data)))
			};
		}
	}

	public void ApplyEdit(Action<bool> onResponse)
	{
		int page = _originalPageInfo.Page;
		TranscendSkillPageInfo transcendSkillPageInfo = _editingHeroInfo.GetTranscendSkillPageInfo(page);
		SceneAutoBehaviour<MyHeroManagement>.instance.LearnHeroTranscendSkill(_editingHeroInfo.HeroIndex, page, transcendSkillPageInfo.TranscendSkillDatas, delegate
		{
			_isEditing = false;
			UIHelper.EnableGameObject(EditButtonRoot, enable: false);
			Refresh();
		});
	}

	public void CancelEdit()
	{
		_isEditing = false;
		TranscendSkillPageInfo transcendSkillPageInfo = _editingHeroInfo.GetTranscendSkillPageInfo(_originalPageInfo.Page);
		if (transcendSkillPageInfo != null)
		{
			transcendSkillPageInfo.TranscendSkillDatas = _originalPageInfo.TranscendSkillDatas.ConvertAll((CreatureData.TranscendSkill data) => JsonMarshaler.Parse<CreatureData.TranscendSkill>(JsonMarshaler.EncodeJsonString(data)));
			SceneAutoBehaviour<MyHeroManagement>.instance.RefreshHeroStat(_editingHeroInfo.HeroIndex);
		}
		else
		{
			MessagePopup.OpenError(BaseResultType.InternalError);
		}
		UIHelper.EnableGameObject(EditButtonRoot, enable: false);
		Refresh();
	}

	public void CheckEndEditing()
	{
		if (_isEditing)
		{
			MessagePopup3Button.OpenGlobal(LocalizationManager.GetStringDirect("HERO_TRANSCEND_ATTRIBUTE"), LocalizationManager.GetStringDirect("WANNA_SAVE_CHANGES"), delegate
			{
				ApplyEdit(null);
			}, CancelEdit, CancelEdit);
		}
		UIHelper.EnableGameObject(EditButtonRoot, enable: false);
		_isEditing = false;
	}

	private void OnDisable()
	{
		UIHelper.EnableGameObject(EditButtonRoot, enable: false);
		CheckEndEditing();
	}

	private void OnDestroy()
	{
		CheckEndEditing();
	}
}

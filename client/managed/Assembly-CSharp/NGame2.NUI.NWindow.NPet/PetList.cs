using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NPet;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NChatting;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetList : RenewResourceWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver<ItemInfo>, NGame2.NUI.NComponent2.NPet.IDataProvider, PetProfilePanel.IListener, PetAwakenPanel.IListener, PetTotalViewPanel.IListener, PetPortraitComponent.IListener, PetStatePanel.IListener
{
	public enum EToggle
	{
		None,
		Profile,
		Awaken
	}

	public interface IListener
	{
		void OnClickMainPet(int petIndex);

		void OnClickAwaken(int petIndex, Action onCallback);

		void OnClickDetailView(int petIndex, Action openAction, Action closeAction);

		void OnClickArchive(int petIndex);

		void OnClickSelectPetPlacePopup();

		void OnClickOpenPetTotalBuffPopup();

		void OnClose();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetList";

	public UILabel TitleLabel;

	public UIBundleLoader BackgroundLoader;

	public PetModelPanel PetModelPanel;

	[Header("LeftPanel")]
	public PetPortraitListPanel PetPortraitListPanel;

	public PetStatePanel PetStatePanel;

	public PetTotalViewPanel PetTotalViewPanel;

	[Header("RightPanel")]
	public PetProfilePanel PetProfilePanel;

	public PetAwakenPanel PetAwakenPanel;

	public float NPCActionDelaySecond = 3f;

	private PetInfo _seletedPetInfo;

	private List<PetInfo> _petInfoList;

	private IListener _listener;

	private EToggle _curToggle;

	private DateTime _motionStartedTime = DateTime.MinValue;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (PayShopResourceList != null)
		{
			PayShopResourceList.OnChanged(changedInfo);
		}
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo.PetIndex == _seletedPetInfo.PetIndex)
		{
			_seletedPetInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(changedInfo.PetIndex);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petInfoList); i++)
		{
			if (_petInfoList[i].PetIndex == changedInfo.PetIndex)
			{
				_petInfoList[i] = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(changedInfo.PetIndex);
				break;
			}
		}
		if (PetAwakenPanel != null)
		{
			PetAwakenPanel.Refresh(changedInfo);
		}
		if (PetProfilePanel != null)
		{
			PetProfilePanel.Refresh(changedInfo);
		}
		if (PetStatePanel != null)
		{
			PetStatePanel.Refresh(changedInfo);
		}
		if (PetPortraitListPanel != null)
		{
			PetPortraitListPanel.Refresh(changedInfo);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && PetAwakenPanel != null)
		{
			PetAwakenPanel.Refresh(changedInfo);
		}
	}

	PetInfo NGame2.NUI.NComponent2.NPet.IDataProvider.GetSelectedPetInfo()
	{
		return _seletedPetInfo;
	}

	List<PetInfo> NGame2.NUI.NComponent2.NPet.IDataProvider.GetPetInfoList()
	{
		return _petInfoList;
	}

	public void Open(PetInfo petInfo, IListener listener)
	{
		Open(delegate
		{
			_listener = listener;
			_petInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoList(sort: true, PetManager.EPetCompareType.PetManagement);
			_curToggle = EToggle.Profile;
			AddUserObserver();
			Init(petInfo);
		}, delegate
		{
			OpenRaiderManagementUI triggerEvent = new OpenRaiderManagementUI();
			if (!SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent) && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(11440) && GlobalAutoBehaviour<AccountManager>.instance.CanOpenPetAwakenTutorial())
			{
				OpenRaiderAwakenUI triggerEvent2 = new OpenRaiderAwakenUI();
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent2);
			}
		});
	}

	private void Init(PetInfo petInfo)
	{
		Init();
		_seletedPetInfo = petInfo;
		if (_seletedPetInfo == null)
		{
			_seletedPetInfo = _petInfoList.FirstOrDefault();
			if (_seletedPetInfo != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAddedPet(_seletedPetInfo.PetIndex);
			}
		}
		InitSelectPet();
	}

	private void InitSelectPet()
	{
		if (PetPortraitListPanel != null)
		{
			PetPortraitListPanel.Open(this, this);
		}
		InitPetStatePanel();
		InitPetProfilePanel();
		SetPetModel(_seletedPetInfo);
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
	}

	private void InitPetStatePanel()
	{
		if (!(PetStatePanel == null))
		{
			PetStatePanel.Open(new PetStatePanel.PetStatePanelInfo
			{
				IDataProvider = this,
				IPetLisListener = _listener,
				IListener = this
			});
		}
	}

	private void InitPetProfilePanel()
	{
		if (!(PetProfilePanel == null))
		{
			PetProfilePanel.Open(this, _listener, this);
		}
	}

	public void SetSelectPet(PetInfo petInfo)
	{
		if (petInfo != null && _seletedPetInfo != petInfo)
		{
			PetInfo seletedPetInfo = _seletedPetInfo;
			_seletedPetInfo = petInfo;
			RefreshCompList(seletedPetInfo, petInfo);
			SetPetModel(petInfo);
			SetIPetPanel(_curToggle);
			GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAddedPet(petInfo.PetIndex);
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
	}

	private void RefreshCompList(PetInfo oldPet, PetInfo petInfo)
	{
		if (PetPortraitListPanel != null)
		{
			PetPortraitListPanel.SetSelectionComp(oldPet, petInfo);
		}
	}

	private void SetPetModel(PetInfo petInfo)
	{
		if (petInfo != null)
		{
			ClosePetModel();
			CreatureInUIVisualData visualData = new CreatureInUIVisualData(new AnimationKeyData(DefineCreature.EMotion.Idle, string.Empty), CreatureStock.ELoadLevel.IncludeFacial);
			if (PetModelPanel != null)
			{
				PetModelPanel.Open(petInfo, isPlaySound: true, visualData);
			}
		}
	}

	private void SetIPetPanel(EToggle eToggle, bool isInit = true)
	{
		if (_curToggle == eToggle)
		{
			RefreshIPetPanel();
			return;
		}
		CloseAllPanel();
		_curToggle = eToggle;
		if (isInit)
		{
			if (_curToggle == EToggle.Profile)
			{
				_petInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoList(sort: true, PetManager.EPetCompareType.PetManagement);
			}
			else if (_curToggle == EToggle.Awaken)
			{
				_petInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetAwakenInfoList();
			}
		}
		switch (_curToggle)
		{
		case EToggle.Profile:
			if (PetPortraitListPanel != null)
			{
				PetPortraitListPanel.Open(this, this);
				PetPortraitListPanel.SetTotalViewButton(onOff: true);
				PetPortraitListPanel.SetExclamationComp(onOff: false);
			}
			InitPetStatePanel();
			InitPetProfilePanel();
			UIHelper.SetLabelTextLocalizeDirect(TitleLabel, "PETHOUSE_BACKBUTTON_MANAGEMENT");
			break;
		case EToggle.Awaken:
			if (PetPortraitListPanel != null)
			{
				PetPortraitListPanel.Open(this, this);
				PetPortraitListPanel.SetTotalViewButton(onOff: false);
				PetPortraitListPanel.SetExclamationComp(onOff: true);
			}
			if (PetAwakenPanel != null)
			{
				PetAwakenPanel.Open(this, this);
			}
			UIHelper.SetLabelTextLocalizeDirect(TitleLabel, "PET_BUTTON_AWAKENING");
			break;
		}
	}

	private void RefreshIPetPanel()
	{
		switch (_curToggle)
		{
		case EToggle.Profile:
			InitPetStatePanel();
			InitPetProfilePanel();
			break;
		case EToggle.Awaken:
			if (PetAwakenPanel != null)
			{
				PetAwakenPanel.Open(this, this);
			}
			break;
		}
	}

	public void EnableCreatureWidget(bool enabled)
	{
		if (!(PetModelPanel == null) && !(PetModelPanel.CreatureWidget == null))
		{
			UIHelper.EnableGameObject(PetModelPanel.CreatureWidget, enabled);
		}
	}

	private void SetSpecialMotion()
	{
		_motionStartedTime = DateTime.UtcNow;
		if (PetModelPanel != null)
		{
			PetModelPanel.SetSpecialMotion();
		}
	}

	public override void Close()
	{
		if (!(PetAwakenPanel != null) || !PetAwakenPanel.IsPlayAwakenAnim)
		{
			if (_curToggle == EToggle.Awaken)
			{
				SetIPetPanel(EToggle.Profile);
				return;
			}
			_listener.OnClose();
			base.Close();
		}
	}

	public override void OnClosing()
	{
		RemoveObserver();
		base.OnClosing();
		_curToggle = EToggle.None;
		_seletedPetInfo = null;
		if (_petInfoList != null)
		{
			_petInfoList.Clear();
			_petInfoList = null;
		}
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAllAddedPet();
		ClosePetModel();
		CloseAllPanel();
	}

	public override void Reset()
	{
		base.Reset();
	}

	private void ClosePetModel()
	{
		_motionStartedTime = DateTime.MinValue;
		if (PetModelPanel != null)
		{
			PetModelPanel.Clear();
		}
	}

	public new void OnClickPetBuff()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenPetTotalBuffPopup();
		}
	}

	public void OnClickChatting()
	{
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<ChattingWindow>())
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
		}
		else
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		if (!(PetAwakenPanel != null) || !PetAwakenPanel.IsPlayAwakenAnim)
		{
			SetSelectPet(comp.PetInfo);
		}
	}

	public void OnClickPetTotalView()
	{
		if (PetTotalViewPanel != null)
		{
			PetTotalViewPanel.Open(this, this);
		}
	}

	public void OnClickPet()
	{
		if (!((DateTime.UtcNow - _motionStartedTime).TotalSeconds < (double)NPCActionDelaySecond))
		{
			SetSpecialMotion();
		}
	}

	public void OnDragStart()
	{
		if (PetModelPanel != null)
		{
			PetModelPanel.OnDragStart();
		}
	}

	public void OnDragEnd()
	{
		if (PetModelPanel != null)
		{
			PetModelPanel.OnDragEnd();
		}
	}

	public IEnumerable<IPetPanel> GetPetPanels()
	{
		yield return PetPortraitListPanel;
		yield return PetProfilePanel;
		yield return PetAwakenPanel;
		yield return PetStatePanel;
		yield return PetTotalViewPanel;
	}

	public void CloseAllPanel()
	{
		foreach (IPetPanel petPanel in GetPetPanels())
		{
			petPanel.Close();
		}
	}

	private void AddUserObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	private void RemoveObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	void PetProfilePanel.IListener.OpenAwakenPanel()
	{
		SetIPetPanel(EToggle.Awaken);
	}

	void PetAwakenPanel.IListener.OnClickAwaken()
	{
		if (_listener == null)
		{
			return;
		}
		_listener.OnClickAwaken(_seletedPetInfo.PetIndex, delegate
		{
			if (PetAwakenPanel != null)
			{
				PetAwakenPanel.PlayDefaultAnim();
			}
			EnableCreatureWidget(enabled: false);
		});
	}

	void PetAwakenPanel.IListener.OnClose()
	{
		SetIPetPanel(EToggle.Profile);
	}

	void PetTotalViewPanel.IListener.OnClick(PetPortraitComponent comp)
	{
		SetSelectPet(comp.PetInfo);
	}

	void PetStatePanel.IListener.OnClickPetDetailView(int petIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickDetailView(petIndex, delegate
			{
				EnableCreatureWidget(enabled: false);
			}, delegate
			{
				EnableCreatureWidget(enabled: true);
			});
		}
	}
}

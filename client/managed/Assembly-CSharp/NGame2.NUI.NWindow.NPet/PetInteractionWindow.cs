using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetInteractionWindow : RenewResourceWindow, PetFullPointGaugeComponent.IListener, PetInteractionFeedButton.IListener, PetInteractionSpecialButton.IListener, IPetInteractionProvider, IUserObserver<PetHouseManager.ChangedPetInfo>, IUserObserver
{
	public interface IListener
	{
		void PetEat(int petIndex, int eatCount, Action<bool> endAction);

		void NextPetInteraction(int currentPeIndex, int nextIndex);

		void OpenPetDetailView(int petIndex);

		void OpenSelectPetPlacePopup();

		void OpenManagePetWindow(PetInfo petInfo);
	}

	public class PetInteractionInfo
	{
		public IListener IListener { get; set; }

		public PetInfo PetInfo { get; set; }

		public IPetAction IPetAction { get; set; }

		public IPetInteraction IPetInteraction { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetInteractionWindow";

	public PetInteractionFeedButton PetInteractionFeedButton;

	public PetInteractionSpecialButton PetInteractionSpecialButton;

	public List<Transform> HideRootTransformList = new List<Transform>();

	public Transform ShowUIButtonTransform;

	public UIButton LeftButton;

	public UIButton RightButton;

	public Transform PetHappyGaugeComponentRootTranform;

	private PetInteractionInfo _info;

	private PetFullPointGaugeComponent _petFullPointGaugeComponent;

	private NGame2.NUI.NWindow.IntervalUpdater _intervalUpdater;

	private bool _isShowUI;

	bool IPetInteractionProvider.IsFullFullPoint
	{
		get
		{
			if (_petFullPointGaugeComponent == null)
			{
				return false;
			}
			return _petFullPointGaugeComponent.IsFullFullPoint;
		}
	}

	bool PetInteractionFeedButton.IListener.CheckStartPetEat
	{
		get
		{
			if (_info == null || _info.IPetAction == null)
			{
				return false;
			}
			if (_info.IPetAction.EPetActionType == EPetActionType.Sleep)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PET_POPUP_SLEEPING_DISPLAY"));
				return false;
			}
			return true;
		}
	}

	public void Open(PetInteractionInfo info)
	{
		Init(info);
		Open(delegate
		{
			ShowUI();
		});
	}

	public override void Close()
	{
		EndInteraction();
		FactoryBase.SafeReleaseComponent(ref _petFullPointGaugeComponent);
		_intervalUpdater = null;
		PetHouseManager.ObserverManager.RemoveObserver(this);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_intervalUpdater != null)
		{
			_intervalUpdater.Update();
		}
	}

	private void EndInteraction()
	{
		if (_info != null && _info.IPetInteraction != null)
		{
			Close2(immediately: true);
			_info.IPetInteraction.End(isNext: false);
		}
	}

	private void Init(PetInteractionInfo info)
	{
		Init();
		_info = info;
		CreatePetHappyGaugeComponent();
		InitButton();
		SetSpecialButtonState();
		SetFeedButtonState(isRefreshFeedInfo: true);
		SetPetFullPointGaugeComponent();
		EnableButton();
		PetHouseManager.ObserverManager.AddObserver(this);
		_intervalUpdater = new NGame2.NUI.NWindow.IntervalUpdater(RefreshButtonState);
		_intervalUpdater.SetInterval(0.5f);
	}

	private void ShowUI()
	{
		ShowHideUI(isShow: true);
		SetNextInteractionButtonState(enable: true);
	}

	private void InitButton()
	{
		if (PetInteractionFeedButton != null)
		{
			PetInteractionFeedButton.Init(this, this);
		}
		if (PetInteractionSpecialButton != null)
		{
			PetInteractionSpecialButton.Init(this);
		}
	}

	private void SetSpecialButtonState()
	{
		if (!(PetInteractionSpecialButton == null) && _info != null && _info.PetInfo != null)
		{
			bool flag = _info.PetInfo.Grade == CreatureGrade.Legend;
			if (flag && !PetHelper.CanExplorePetStatus(_info.PetInfo))
			{
				flag = false;
			}
			PetInteractionSpecialButton.Enable(flag);
		}
	}

	private void SetFeedButtonState(bool isRefreshFeedInfo)
	{
		if (PetInteractionFeedButton == null || _info == null || _info.PetInfo == null)
		{
			return;
		}
		if (_info.PetInfo.IsLinePet)
		{
			PetInteractionFeedButton.Enable(enable: false);
			return;
		}
		if (isRefreshFeedInfo)
		{
			PetInteractionFeedButton.SetPetEatInfo();
		}
		PetInteractionFeedButton.Enable(PetHelper.CanExplorePetStatus(_info.PetInfo) && !((IPetInteractionProvider)this).IsFullFullPoint);
	}

	private void SetPetFullPointGaugeComponent()
	{
		if (_info.PetInfo != null && _info.PetInfo.CreatureData != null && !(_petFullPointGaugeComponent == null))
		{
			if (_info.PetInfo.IsLinePet)
			{
				UIHelper.EnableGameObject(_petFullPointGaugeComponent, enable: false);
				return;
			}
			UIHelper.EnableGameObject(_petFullPointGaugeComponent, enable: true);
			_petFullPointGaugeComponent.UpdatePetInfo(_info.PetInfo);
		}
	}

	private void CreatePetHappyGaugeComponent()
	{
		if (!(PetHappyGaugeComponentRootTranform == null))
		{
			FactoryBase.SafeReleaseComponent(ref _petFullPointGaugeComponent);
			_petFullPointGaugeComponent = PetFullPointGaugeComponent.PrefabAuto.Factory.Create(PetHappyGaugeComponentRootTranform);
			if (!(_petFullPointGaugeComponent == null))
			{
				_petFullPointGaugeComponent.Open(this, _info.PetInfo);
			}
		}
	}

	private void NextPetInteraction(int nextIndex)
	{
		if (_info != null && _info.PetInfo != null && _info.IListener != null)
		{
			_info.IListener.NextPetInteraction(_info.PetInfo.PetIndex, nextIndex);
		}
	}

	private void EnableShowUIButton(bool enable)
	{
		UIHelper.EnableGameObject(ShowUIButtonTransform, enable);
	}

	private void EnableButton()
	{
		bool enable = false;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.PlacePetCount > 1)
		{
			enable = true;
		}
		UIHelper.EnableGameObject(LeftButton, enable);
		UIHelper.EnableGameObject(RightButton, enable);
	}

	private void SetNextInteractionButtonState(bool enable)
	{
		UIHelper.EnableButton(LeftButton, enable);
		UIHelper.EnableButton(RightButton, enable);
	}

	private void ShowHideUI(bool isShow)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(HideRootTransformList))
		{
			return;
		}
		Transform transform = null;
		for (int i = 0; i < HideRootTransformList.Count; i++)
		{
			transform = HideRootTransformList[i];
			if (!(transform == null))
			{
				UIHelper.EnableGameObject(transform, isShow);
			}
		}
		EnableShowUIButton(!isShow);
		_isShowUI = isShow;
	}

	private void RefreshPetInfo()
	{
		if (_info != null && _info.PetInfo != null && !(_petFullPointGaugeComponent == null))
		{
			PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(_info.PetInfo.PetIndex);
			if (petInfo != null)
			{
				_info.PetInfo = petInfo;
				SetPetFullPointGaugeComponent();
				SetSpecialButtonState();
				SetFeedButtonState(isRefreshFeedInfo: true);
			}
		}
	}

	private void RefreshButtonState()
	{
		if (_info != null && _info.PetInfo != null && !(_petFullPointGaugeComponent == null) && GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(_info.PetInfo.PetIndex) != null)
		{
			SetSpecialButtonState();
			SetFeedButtonState(isRefreshFeedInfo: false);
		}
	}

	public void OnClickLeft()
	{
		Close2(immediately: true);
		NextPetInteraction(-1);
	}

	public void OnClickRight()
	{
		Close2(immediately: true);
		NextPetInteraction(1);
	}

	public void OnClickHideUI()
	{
		ShowHideUI(isShow: false);
	}

	public void OnClickShowUI()
	{
		ShowHideUI(isShow: true);
	}

	public void OnClickPetMotion()
	{
		if (_info != null && _info.IListener != null && _info.PetInfo != null)
		{
			_info.IListener.OpenPetDetailView(_info.PetInfo.PetIndex);
		}
	}

	public void OnClickPlacePet()
	{
		if (_info != null && _info.IPetInteraction != null && _info.IListener != null)
		{
			_info.IListener.OpenSelectPetPlacePopup();
			EndInteraction();
		}
	}

	public void OnClickManagePet()
	{
		if (_info != null && _info.IPetInteraction != null && _info.IListener != null && _info.PetInfo != null)
		{
			_info.IListener.OpenManagePetWindow(_info.PetInfo);
			EndInteraction();
		}
	}

	void PetFullPointGaugeComponent.IListener.OnClick()
	{
	}

	void PetInteractionFeedButton.IListener.PetEating(int addFullPoint)
	{
		if (!(_petFullPointGaugeComponent == null))
		{
			_petFullPointGaugeComponent.UpdateFullPoint(addFullPoint);
		}
	}

	void PetInteractionFeedButton.IListener.EndPetEat(int eatCount, Action failAction)
	{
		if (_info == null || _info.IListener == null || _info.PetInfo == null)
		{
			if (failAction != null)
			{
				Vespa_System_Action_Extension_Method.fire(failAction);
			}
			return;
		}
		_info.IListener.PetEat(_info.PetInfo.PetIndex, eatCount, delegate(bool isSuccess)
		{
			RefreshPetInfo();
			if (isSuccess && _info.IPetAction != null)
			{
				_info.IPetAction.HappyForInteraction();
			}
		});
	}

	void PetInteractionSpecialButton.IListener.PetPlay()
	{
		if (_info != null && _info.IPetAction != null)
		{
			_info.IPetAction.Special();
			ShowHideUI(isShow: false);
			if (_petFullPointGaugeComponent != null)
			{
				_petFullPointGaugeComponent.UpdateFullPoint(0);
			}
		}
	}

	void IUserObserver<PetHouseManager.ChangedPetInfo>.OnChanged(PetHouseManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null)
		{
			RefreshPetInfo();
			if (!_isShowUI)
			{
				ShowHideUI(isShow: true);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}

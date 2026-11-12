using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NPet;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectAvatarPet : BaseWindow, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver, PetPortraitComponent.IListener
{
	[Serializable]
	public class GameOptionMyInfo
	{
		public UIBundleLoader BundleTex_MainPet;

		public GameObject PetLockedState;

		public UIButton Button_ReleaseMainPet;

		public UIButton Button_ChangeMainPet;

		public UIScrollGrid PetScrollGrid;
	}

	public interface Listener
	{
		void ChangeAvatarPet(int petIndex = 0);

		void OpenPetHouseManagement();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/SelectAvatarPet";

	public GameOptionMyInfo MyPetInfo = new GameOptionMyInfo();

	private List<PetPortraitComponent> _petPortraitComponentList = new List<PetPortraitComponent>();

	private Dictionary<CreatureGrade, bool> _newMarkGradeDic = new Dictionary<CreatureGrade, bool>();

	private PetPortraitComponent _selectComp;

	private Listener _listener;

	private List<PetInfo> _petInfoList;

	private bool _isMainPet;

	private int _selectPetIndex;

	private int _mainPetIndex;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		Refresh(changedInfo);
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		SetSelectPet(comp);
	}

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	private void Init(Listener listener)
	{
		Reset();
		_listener = listener;
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		RefreshAvatarPetPortrait();
		SetPetList();
	}

	private void RefreshAvatarPetPortrait(int selectIndex = 0)
	{
		string assetPath = "Portrait/Pet/CREATURE_PET_EXPONENT_UNSELECTED";
		SetPetLockedState();
		if (selectIndex > 0)
		{
			assetPath = UIHelper.GetPetPortraitPath(selectIndex);
		}
		else
		{
			PetMiscInfo petMiscInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetPetMiscInfo(PetMiscType.AvatarPetIndex);
			if (petMiscInfo != null)
			{
				int num = Vespa_String_Extension_Method.TOINT(petMiscInfo.MiscValue, 0);
				assetPath = UIHelper.GetPetPortraitPath(num);
				_isMainPet = true;
				_mainPetIndex = num;
				if (_selectPetIndex == 0)
				{
					_selectPetIndex = num;
				}
			}
		}
		SetButtonEnabled();
		UIHelper.SetBundleTexture(MyPetInfo.BundleTex_MainPet, assetPath, UIHelper.EAssetTextureAssign.ClipCenter);
	}

	private void SetPetList(CreatureGrade selectGrade = CreatureGrade.None)
	{
		ClearPetList();
		List<PetInfo> petInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoList(sort: true, PetManager.EPetCompareType.PetManagement);
		PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(_selectPetIndex);
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petInfoList); i++)
		{
			PetInfo petInfo2 = petInfoList[i];
			if (petInfo2 == null)
			{
				continue;
			}
			CreatureData creatureData = petInfo2.CreatureData;
			if (creatureData == null)
			{
				continue;
			}
			CreatureGrade creatureGrade = creatureData.CreatureGrade;
			if (selectGrade == CreatureGrade.None || selectGrade == creatureGrade)
			{
				flag = GlobalAutoBehaviour<AccountManager>.instance.PetManager.IsNew(creatureData.Index);
				if (!_newMarkGradeDic.ContainsKey(creatureGrade) && flag)
				{
					_newMarkGradeDic[creatureGrade] = flag;
				}
				bool mainPet = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex == petInfo2.PetIndex;
				bool sleep = PetHelper.CheckIsSleep(petInfo2);
				PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(MyPetInfo.PetScrollGrid, PetPortraitComponent.SmallPrefabAuto);
				petPortraitComponent.Open(petInfo2, this);
				petPortraitComponent.SetNew(flag);
				petPortraitComponent.SetMainPet(mainPet);
				petPortraitComponent.SetSleep(sleep);
				if (petInfo == petInfo2)
				{
					_selectComp = petPortraitComponent;
					petPortraitComponent.SetSelection(select: true);
					petPortraitComponent.SetNew(show: false);
				}
				_petPortraitComponentList.Add(petPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(MyPetInfo.PetScrollGrid);
	}

	public void Refresh(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo == null)
		{
			return;
		}
		PetPortraitComponent petPortraitComponent = null;
		if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.CreateMainAvatarPet)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petPortraitComponentList); i++)
			{
				petPortraitComponent = _petPortraitComponentList[i];
				if (petPortraitComponent.PetIndex == changedInfo.PetIndex)
				{
					_mainPetIndex = changedInfo.PetIndex;
					_selectComp = petPortraitComponent;
					petPortraitComponent.SetMainPet(show: true);
					break;
				}
			}
		}
		else if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.ChangeMainAvatarPet)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_petPortraitComponentList); j++)
			{
				petPortraitComponent = _petPortraitComponentList[j];
				if (petPortraitComponent.PetIndex == _mainPetIndex)
				{
					petPortraitComponent.SetMainPet(show: false);
				}
				else if (petPortraitComponent.PetIndex == changedInfo.PetIndex)
				{
					petPortraitComponent.SetMainPet(show: true);
				}
			}
			_mainPetIndex = changedInfo.PetIndex;
			CheckMainPet(changedInfo);
		}
		RefreshAvatarPetPortrait(_selectPetIndex);
	}

	private void CheckMainPet(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetPetMiscInfo(PetMiscType.AvatarPetIndex) == null && changedInfo.PetIndex > 0)
		{
			if ((bool)_selectComp)
			{
				_selectComp.SetSelection(select: false);
			}
			_isMainPet = false;
			_mainPetIndex = 0;
			_selectPetIndex = 0;
			_selectComp = null;
		}
	}

	private void SetSelectPet(PetPortraitComponent comp)
	{
		if ((bool)_selectComp)
		{
			_selectComp.SetSelection(select: false);
		}
		comp.SetSelection(select: true);
		comp.SetNew(show: false);
		_selectComp = comp;
		_selectPetIndex = comp.PetIndex;
		RefreshAvatarPetPortrait(_selectPetIndex);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAddedPet(_selectPetIndex);
	}

	private void SetButtonEnabled()
	{
		bool enable = false;
		bool enable2 = false;
		if (_selectPetIndex > 0)
		{
			if (_selectPetIndex == _mainPetIndex)
			{
				enable = true;
			}
			else if (_selectPetIndex != _mainPetIndex)
			{
				enable2 = true;
			}
		}
		UIHelper.EnableButton(MyPetInfo.Button_ReleaseMainPet, enable);
		UIHelper.EnableButton(MyPetInfo.Button_ChangeMainPet, enable2);
	}

	public new void Reset()
	{
		_listener = null;
		_isMainPet = false;
		_selectPetIndex = 0;
		_mainPetIndex = 0;
		if (_newMarkGradeDic != null)
		{
			_newMarkGradeDic.Clear();
		}
		ClearPetList();
		UIHelper.EnableButton(MyPetInfo.Button_ReleaseMainPet, enable: false);
		UIHelper.EnableButton(MyPetInfo.Button_ChangeMainPet, enable: false);
	}

	public bool SetPetLockedState()
	{
		bool flag = false;
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null)
		{
			flag = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(14000);
		}
		UIHelper.EnableGameObject(MyPetInfo.PetLockedState, !flag);
		return flag;
	}

	private void ClearPetList()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitComponentList);
	}

	private void SetMainAvatarPetPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
		string stringDirect2 = LocalizationManager.GetStringDirect("PET_MAINPET_POPUP_DESC_TEXT");
		int changeIndex = _selectPetIndex;
		if (_mainPetIndex == _selectPetIndex)
		{
			stringDirect2 = LocalizationManager.GetStringDirect("PET_MAINPET_RELEASE_NOTICE_DESC");
			changeIndex = 0;
		}
		PayShopMessage3ButtonPopup.Open(stringDirect, stringDirect2, delegate
		{
			_listener.ChangeAvatarPet(changeIndex);
		});
	}

	public void OnClickChangeMainPetButton()
	{
		SetMainAvatarPetPopup();
	}

	public void OnClickRelesasMainPetButton()
	{
		SetMainAvatarPetPopup();
	}

	public void OnClickOpenPetManagement()
	{
		_listener.OpenPetHouseManagement();
	}

	public void OnClickExitButton()
	{
		Reset();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAllAddedPet();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
		Close();
	}
}

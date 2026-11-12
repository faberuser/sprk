using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class SelectPlacePetPopup : BaseWindow, PetPortraitComponent.IListener, PetGradeTabComponent.IListener, IUserObserver<PetHouseManager.ChangedPetInfo>, IUserObserver
{
	public interface IListener
	{
		void CompletePlacePet(bool isRequest, List<PlacePetInfo> placePetInfoList);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/Popup/SelectPlacePetPopup";

	private readonly int EmptyPlaceIndex;

	public UIScrollGrid PetInfoListScrollGrid;

	public UISimpleGrid PetGradeTabSimpleGrid;

	public Transform NoPetDescLabelTransform;

	public List<Transform> PlacedPetRootTransformList = new List<Transform>();

	private List<PetPortraitComponent> _petPortraitComponentList = new List<PetPortraitComponent>();

	private List<PetPortraitComponent> _placedPetPortraitComponentList = new List<PetPortraitComponent>();

	private List<PetGradeTabComponent> _petGradeTabComponentList = new List<PetGradeTabComponent>();

	private List<PetInfo> _userPetInfoList;

	private Dictionary<int, int> _userPlacePetIndexDic;

	private Dictionary<int, PlacePetInfo> _placePetInfoDic;

	private List<int> _sleepPetIndexList;

	private IListener _iListener;

	private IPetHouseProvider _iPetHouseProvider;

	private CreatureGrade _selectGrade;

	private int _mainPetIndex;

	private Dictionary<CreatureGrade, bool> _newMarkGradeDic = new Dictionary<CreatureGrade, bool>();

	private int PlaceCount
	{
		get
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(PlacedPetRootTransformList))
			{
				return PlacedPetRootTransformList.Count;
			}
			return 5;
		}
	}

	private bool CheckIsRequest
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_placePetInfoDic))
			{
				return true;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_userPlacePetIndexDic))
			{
				return true;
			}
			bool result = false;
			if (_placePetInfoDic.Count > 0)
			{
				PlacePetInfo placePetInfo = null;
				int num = 0;
				for (int i = 0; i < _placePetInfoDic.Count; i++)
				{
					placePetInfo = _placePetInfoDic[i];
					if (placePetInfo != null)
					{
						num = _userPlacePetIndexDic[i];
						if (placePetInfo.PetIndex != num)
						{
							placePetInfo.IsPlace = true;
							result = true;
						}
					}
				}
			}
			return result;
		}
	}

	private List<int> UserPlacePetIndexList
	{
		get
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return null;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.PetManager.PlacePetIndexList;
		}
	}

	public void Open(IListener iListener, IPetHouseProvider iPetHouseProvider)
	{
		base.Open();
		Init(iListener, iPetHouseProvider);
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_sleepPetIndexList = null;
		FactoryBase.ReleaseAndClearList(_petGradeTabComponentList);
		FactoryBase.ReleaseAndClearList(_placedPetPortraitComponentList);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAllAddedPet();
		PetHouseManager.ObserverManager.RemoveObserver(this);
	}

	public override void Reset()
	{
		base.Reset();
		if (_newMarkGradeDic != null)
		{
			_newMarkGradeDic.Clear();
		}
	}

	private void Init(IListener iListener, IPetHouseProvider iPetHouseProvider)
	{
		_iListener = iListener;
		_iPetHouseProvider = iPetHouseProvider;
		_mainPetIndex = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex;
		PetHouseManager.ObserverManager.AddObserver(this);
		InitPlacePetIndexList();
		InitPetGradeTabList();
		SetPetList();
		SetPlacePetList();
		RefreshNewNotice();
		if (_iPetHouseProvider != null)
		{
			_iPetHouseProvider.CurrentPetsState();
		}
		OpenRaiderDisplayUI triggerEvent = new OpenRaiderDisplayUI();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void InitPlacePetIndexList()
	{
		_userPlacePetIndexDic = new Dictionary<int, int>();
		_userPlacePetIndexDic.Clear();
		_placePetInfoDic = new Dictionary<int, PlacePetInfo>();
		_placePetInfoDic.Clear();
		int placeCount = PlaceCount;
		int num = 0;
		for (int i = 0; i < placeCount; i++)
		{
			num = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(UserPlacePetIndexList) && UserPlacePetIndexList.Count > i)
			{
				num = UserPlacePetIndexList[i];
			}
			_userPlacePetIndexDic.Add(i, num);
			_placePetInfoDic.Add(i, new PlacePetInfo
			{
				PetIndex = num,
				IsPlace = false
			});
		}
	}

	private void InitPetGradeTabList()
	{
		_selectGrade = CreatureGrade.None;
		FactoryBase.ReleaseAndClearList(_petGradeTabComponentList);
		if (PetGradeTabSimpleGrid == null)
		{
			return;
		}
		PetGradeTabComponent petGradeTabComponent = null;
		CreatureGrade[] array = Enum.GetValues(typeof(CreatureGrade)) as CreatureGrade[];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			CreatureGrade creatureGrade = array[i];
			petGradeTabComponent = UIHelper.AddGridItem(PetGradeTabSimpleGrid, PetGradeTabComponent.PrefabAuto);
			if (!(petGradeTabComponent == null))
			{
				petGradeTabComponent.Init(creatureGrade, this);
				petGradeTabComponent.SelectToggle(_selectGrade == creatureGrade);
				_petGradeTabComponentList.Add(petGradeTabComponent);
			}
		}
		UIHelper.RepositionGrid(PetGradeTabSimpleGrid);
	}

	private void SetPlacePetList()
	{
		FactoryBase.ReleaseAndClearList(_placedPetPortraitComponentList);
		if (_iPetHouseProvider == null || __Extension_Method_ICollection__.IsNullOrEmpty(PlacedPetRootTransformList) || __Extension_Method_ICollection__.IsNullOrEmpty(_placePetInfoDic))
		{
			return;
		}
		List<PlacePetInfo> list = _placePetInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		int num = 0;
		Transform transform = null;
		for (int i = 0; i < _iPetHouseProvider.PetPlaceCount; i++)
		{
			if (PlacedPetRootTransformList.Count <= i)
			{
				continue;
			}
			transform = PlacedPetRootTransformList[i];
			if (transform == null)
			{
				continue;
			}
			if (transform.childCount > 0)
			{
				FactoryBase.ReleaseGameObject(transform.GetChild(0).gameObject);
			}
			if (list.Count > i)
			{
				num = list[i].PetIndex;
				if (num > EmptyPlaceIndex)
				{
					AddPlacePetPortraitComponent(transform, num);
				}
			}
		}
	}

	private void AddPlacePetPortraitComponent(Transform rootTransform, int petIndex)
	{
		if (rootTransform == null)
		{
			return;
		}
		PetPortraitComponent petPortraitComponent = PetPortraitComponent.SmallPrefabAuto.Factory.Create(rootTransform);
		if (!(petPortraitComponent == null))
		{
			PetInfo userPetInfo = GetUserPetInfo(petIndex);
			if (userPetInfo != null)
			{
				SetPetPortrait(petPortraitComponent, userPetInfo, isPlace: true);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(petPortraitComponent.transform);
				_placedPetPortraitComponentList.Add(petPortraitComponent);
			}
		}
	}

	private void AddPlacePet(int addPetIndex)
	{
		if (addPetIndex <= EmptyPlaceIndex)
		{
			return;
		}
		List<PlacePetInfo> list = _placePetInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].PetIndex;
			if (num == addPetIndex)
			{
				num2 = -1;
				break;
			}
			if (num2 == -1 && num <= EmptyPlaceIndex)
			{
				num2 = i;
			}
		}
		if (num2 > -1)
		{
			_placePetInfoDic[num2].PetIndex = addPetIndex;
		}
	}

	private void RemovePlacedPet(int removePetIndex)
	{
		if (removePetIndex <= EmptyPlaceIndex)
		{
			return;
		}
		List<PlacePetInfo> list = _placePetInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].PetIndex;
			if (removePetIndex == num)
			{
				_placePetInfoDic[i].PetIndex = EmptyPlaceIndex;
				break;
			}
		}
	}

	private void SetPetList()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitComponentList);
		UIHelper.EnableGameObject(NoPetDescLabelTransform, enable: false);
		if (PetInfoListScrollGrid == null)
		{
			return;
		}
		if (_petPortraitComponentList == null)
		{
			_petPortraitComponentList = new List<PetPortraitComponent>();
		}
		_userPetInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoList(sort: true);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_userPetInfoList))
		{
			UIHelper.EnableGameObject(NoPetDescLabelTransform, enable: true);
			return;
		}
		PetInfo petInfo = null;
		PetPortraitComponent petPortraitComponent = null;
		for (int i = 0; i < _userPetInfoList.Count; i++)
		{
			petInfo = _userPetInfoList[i];
			if (petInfo == null || (_selectGrade != CreatureGrade.None && petInfo.Grade != _selectGrade))
			{
				continue;
			}
			petPortraitComponent = UIHelper.AddGridItem(PetInfoListScrollGrid, PetPortraitComponent.SmallPrefabAuto);
			if (!(petPortraitComponent == null))
			{
				if (PetHelper.CheckIsSleep(petInfo))
				{
					AddSleepPetIndex(petInfo.PetIndex);
				}
				SetPetPortrait(petPortraitComponent, petInfo);
				_petPortraitComponentList.Add(petPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(PetInfoListScrollGrid);
		PetInfoListScrollGrid.ResetScroll();
	}

	private void SetPetPortrait(PetPortraitComponent component, PetInfo petInfo, bool isPlace = false)
	{
		if (!(component == null) && petInfo != null)
		{
			component.Open(petInfo, this);
			component.SetSleep(PetHelper.CheckIsSleep(petInfo));
			component.SetMainPet(_mainPetIndex == petInfo.PetIndex);
			if (!isPlace)
			{
				component.SetNew(GlobalAutoBehaviour<AccountManager>.instance.PetManager.IsNew(petInfo.PetIndex));
				component.SetSelection(IsPlacePet(petInfo.PetIndex));
			}
		}
	}

	private void RefreshPetListSelectState()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petPortraitComponentList))
		{
			return;
		}
		PetPortraitComponent petPortraitComponent = null;
		for (int i = 0; i < _petPortraitComponentList.Count; i++)
		{
			petPortraitComponent = _petPortraitComponentList[i];
			if (!(petPortraitComponent == null))
			{
				petPortraitComponent.SetSelection(IsPlacePet(petPortraitComponent.PetIndex));
			}
		}
	}

	private void AddSleepPetIndex(int petIndex)
	{
		if (_sleepPetIndexList == null)
		{
			_sleepPetIndexList = new List<int>();
			_sleepPetIndexList.Clear();
		}
		if (!IsSleepPet(petIndex))
		{
			_sleepPetIndexList.Add(petIndex);
		}
	}

	private void OpenConfirmPopupPlacePet()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_CONFIRM_ARRANGE");
		PayShopMessage3ButtonPopup.Open(text, stringDirect, delegate
		{
			CompletePlacePet();
		});
	}

	public void OpenConfirmPopupClose()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_CANCEL_ARRANGE");
		PayShopMessage3ButtonPopup.Open(text, stringDirect, delegate
		{
			Close();
		});
	}

	private void RefreshNewNotice()
	{
		if (_selectGrade == CreatureGrade.None)
		{
			_newMarkGradeDic.Clear();
		}
		else
		{
			_newMarkGradeDic.Remove(_selectGrade);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petPortraitComponentList); i++)
		{
			PetPortraitComponent petPortraitComponent = _petPortraitComponentList[i];
			if (petPortraitComponent == null || petPortraitComponent.CreatureData == null)
			{
				continue;
			}
			CreatureGrade creatureGrade = petPortraitComponent.CreatureData.CreatureGrade;
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.PetManager.IsNew(petPortraitComponent.PetIndex);
			if (flag && !_newMarkGradeDic.ContainsKey(creatureGrade))
			{
				_newMarkGradeDic.Add(creatureGrade, flag);
				if (_selectGrade != CreatureGrade.None)
				{
					break;
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_petGradeTabComponentList); j++)
		{
			PetGradeTabComponent petGradeTabComponent = _petGradeTabComponentList[j];
			if (!(petGradeTabComponent == null) && (_selectGrade == CreatureGrade.None || petGradeTabComponent.GradeType == _selectGrade))
			{
				bool flag2 = _newMarkGradeDic.ContainsKey(petGradeTabComponent.GradeType) && _newMarkGradeDic[petGradeTabComponent.GradeType];
				petGradeTabComponent.SetNew(flag2);
			}
		}
	}

	private PetInfo GetUserPetInfo(int petIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_userPetInfoList))
		{
			return null;
		}
		return _userPetInfoList.Find((PetInfo petInfo) => petInfo.PetIndex == petIndex);
	}

	private bool IsSleepPet(int petIndex)
	{
		if (_sleepPetIndexList == null)
		{
			return false;
		}
		return _sleepPetIndexList.Find((int index) => index == petIndex) > 0;
	}

	private bool IsPlacePet(int petIndex)
	{
		List<PlacePetInfo> list = _placePetInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return false;
		}
		PlacePetInfo placePetInfo = null;
		for (int i = 0; i < list.Count; i++)
		{
			placePetInfo = list[i];
			if (placePetInfo != null && placePetInfo.PetIndex == petIndex)
			{
				return true;
			}
		}
		return false;
	}

	private void CompletePlacePet()
	{
		if (_iListener != null)
		{
			_iListener.CompletePlacePet(CheckIsRequest, _placePetInfoDic.Values.ToList());
		}
	}

	public void OnClickClose()
	{
		if (!CheckIsRequest)
		{
			Close();
		}
		else
		{
			OpenConfirmPopupClose();
		}
	}

	public void OnClickPlacePet()
	{
		if (!CheckIsRequest)
		{
			Close();
		}
		else
		{
			OpenConfirmPopupPlacePet();
		}
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		if (comp == null || comp.PetInfo == null)
		{
			return;
		}
		int petIndex = comp.PetInfo.PetIndex;
		List<PlacePetInfo> list = _placePetInfoDic.Values.ToList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list) && list.Count > 0)
		{
			bool flag = true;
			PlacePetInfo placePetInfo = null;
			for (int i = 0; i < list.Count; i++)
			{
				placePetInfo = list[i];
				if (placePetInfo != null && placePetInfo.PetIndex == petIndex)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (IsSleepPet(petIndex))
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("PET_POPUP_SLEEPING_DISPLAY"));
					return;
				}
				AddPlacePet(petIndex);
			}
			else
			{
				RemovePlacedPet(petIndex);
			}
		}
		SetPlacePetList();
		RefreshPetListSelectState();
		comp.SetNew(show: false);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemoveAddedPet(petIndex);
		RefreshNewNotice();
		if (_iPetHouseProvider != null)
		{
			_iPetHouseProvider.CurrentPetsState();
		}
	}

	void PetGradeTabComponent.IListener.OnClickTab(CreatureGrade grade)
	{
		if (_selectGrade != grade)
		{
			_selectGrade = grade;
			SetPetList();
		}
	}

	void IUserObserver<PetHouseManager.ChangedPetInfo>.OnChanged(PetHouseManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null)
		{
			if (changedInfo.EPetActionType == EPetActionType.Sleep)
			{
				AddSleepPetIndex(changedInfo.PetIndex);
			}
			else if (IsSleepPet(changedInfo.PetIndex))
			{
				_sleepPetIndexList.Remove(changedInfo.PetIndex);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}

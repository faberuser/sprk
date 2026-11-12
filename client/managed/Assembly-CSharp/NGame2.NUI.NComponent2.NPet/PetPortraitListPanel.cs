using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetPortraitListPanel : MonoBehaviour, IPetPanel
{
	public UIScrollGrid PetListGrid;

	public UIButton TotalViewButton;

	private List<PetPortraitComponent> _petCompList = new List<PetPortraitComponent>();

	private int _mainPetIndex;

	private IDataProvider _dataProvider;

	private PetPortraitComponent.IListener _listener;

	public void Open(IDataProvider dataProvider, PetPortraitComponent.IListener listener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_dataProvider = dataProvider;
		_listener = listener;
		Init();
	}

	private void Init()
	{
		SetPetList();
	}

	private void SetPetList()
	{
		ClearPetList();
		List<PetInfo> petInfoList = _dataProvider.GetPetInfoList();
		_mainPetIndex = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex;
		PetPortraitComponent petPortraitComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petInfoList); i++)
		{
			PetInfo petInfo = petInfoList[i];
			if (petInfo == null)
			{
				continue;
			}
			CreatureData creatureData = petInfo.CreatureData;
			if (creatureData != null)
			{
				bool mainPet = _mainPetIndex == petInfo.PetIndex;
				bool sleep = PetHelper.CheckIsSleep(petInfo);
				PetPortraitComponent petPortraitComponent2 = UIHelper.AddGridItem(PetListGrid, PetPortraitComponent.SmallPrefabAuto);
				petPortraitComponent2.Open(petInfo, _listener);
				petPortraitComponent2.SetMainPet(mainPet);
				petPortraitComponent2.SetSleep(sleep);
				petPortraitComponent2.SetNew(GlobalAutoBehaviour<AccountManager>.instance.PetManager.IsNew(creatureData.Index));
				if (_dataProvider.GetSelectedPetInfo() == petInfo)
				{
					petPortraitComponent = petPortraitComponent2;
					petPortraitComponent2.SetSelection(select: true);
					petPortraitComponent2.SetNew(show: false);
				}
				_petCompList.Add(petPortraitComponent2);
			}
		}
		UIHelper.RepositionGrid(PetListGrid);
		if (petPortraitComponent != null)
		{
			MoveList(petPortraitComponent);
		}
	}

	public void SetSelectionComp(PetInfo oldPet, PetInfo seletPet)
	{
		List<PetInfo> petInfoList = _dataProvider.GetPetInfoList();
		if (oldPet != seletPet && petInfoList.Contains(oldPet))
		{
			int index = _petCompList.FindIndex((PetPortraitComponent value) => value.PetInfo == oldPet);
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_petCompList, index))
			{
				_petCompList[index].SetSelection(select: false);
			}
		}
		if (petInfoList.Contains(seletPet))
		{
			int index2 = _petCompList.FindIndex((PetPortraitComponent value) => value.PetInfo == seletPet);
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_petCompList, index2))
			{
				_petCompList[index2].SetSelection(select: true);
				_petCompList[index2].SetNew(show: false);
				MoveList(_petCompList[index2]);
			}
		}
	}

	public void SetExclamationComp(bool onOff)
	{
		if (!onOff)
		{
			_petCompList.ForEach(delegate(PetPortraitComponent e)
			{
				e.SetExclamationMark(show: false);
			});
			return;
		}
		PetInfo petInfo = null;
		PetPortraitComponent petPortraitComponent = null;
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_petCompList); num++)
		{
			petPortraitComponent = _petCompList[num];
			if (!(petPortraitComponent == null))
			{
				petInfo = petPortraitComponent.PetInfo;
				if (petInfo != null)
				{
					petPortraitComponent.SetExclamationMark(PetHelper.CheckPossiblePetAwaken(petInfo));
				}
			}
		}
	}

	public void SetTotalViewButton(bool onOff)
	{
		UIHelper.EnableGameObject(TotalViewButton, onOff);
	}

	public void Refresh(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo == null || _dataProvider == null)
		{
			return;
		}
		PetPortraitComponent petPortraitComponent = null;
		if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInfoChanged)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petCompList); i++)
			{
				petPortraitComponent = _petCompList[i];
				if (petPortraitComponent.PetIndex == changedInfo.PetIndex)
				{
					PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(changedInfo.PetIndex);
					petPortraitComponent.ResetPortrait(petInfo, null);
					petPortraitComponent.SetExclamationMark(PetHelper.CheckPossiblePetAwaken(petInfo));
					break;
				}
			}
		}
		else if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.CreateMainAvatarPet)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_petCompList); j++)
			{
				petPortraitComponent = _petCompList[j];
				if (petPortraitComponent.PetIndex == changedInfo.PetIndex)
				{
					_mainPetIndex = changedInfo.PetIndex;
					petPortraitComponent.SetMainPet(show: true);
					break;
				}
			}
		}
		else
		{
			if (changedInfo.EChangedPetInfoType != NGame2.NAccount.UserPetManager.EChangedPetInfoType.ChangeMainAvatarPet)
			{
				return;
			}
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_petCompList); k++)
			{
				petPortraitComponent = _petCompList[k];
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
		}
	}

	private void MoveList(PetPortraitComponent petComp, float scrollStrength = 13f)
	{
		int num = __Extension_Method_ICollection__.COUNT(_petCompList);
		if (num <= 5)
		{
			return;
		}
		int num2 = _petCompList.IndexOf(petComp);
		if (num2 != -1)
		{
			Transform cachedTransform = PetListGrid.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = PetListGrid.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num2 <= 1)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_petCompList[0].transform.position) - vector;
				vector2.y += PetListGrid.Grid.cellHeight / 2f;
			}
			else if (num2 >= num - 2)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_petCompList[num - 1].transform.position) - vector3;
				vector2.y -= PetListGrid.Grid.cellHeight / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(petComp.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!PetListGrid.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!PetListGrid.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(PetListGrid.ScrollView.panel.cachedGameObject, pos, scrollStrength).onFinished = delegate
			{
				PetListGrid.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		SetTotalViewButton(onOff: true);
		Reset();
	}

	public void Reset()
	{
		_listener = null;
		_dataProvider = null;
		_mainPetIndex = 0;
		ClearPetList();
	}

	private void ClearPetList()
	{
		FactoryBase.ReleaseAndClearList(_petCompList);
	}
}

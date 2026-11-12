using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetTotalViewPanel : MonoBehaviour, IPetPanel, PetGradeTabComponent.IListener, PetPortraitComponent.IListener
{
	public interface IListener
	{
		void OnClick(PetPortraitComponent comp);
	}

	public UIScrollGrid PetScrollGrid;

	public UISimpleGrid GradeGrid;

	private List<PetGradeTabComponent> _gradeComponentList = new List<PetGradeTabComponent>();

	private List<PetPortraitComponent> _petPortraitComponentList = new List<PetPortraitComponent>();

	private IDataProvider _dataProvider;

	private IListener _listener;

	private CreatureGrade _curToggleType;

	private Dictionary<CreatureGrade, bool> _newMarkGradeDic = new Dictionary<CreatureGrade, bool>();

	public void Open(IDataProvider dataProvider, IListener listener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_dataProvider = dataProvider;
		_listener = listener;
		_curToggleType = CreatureGrade.None;
		Init();
	}

	private void Init()
	{
		SetPetList();
		SetGradeList();
	}

	private void SetPetList(CreatureGrade selectGrade = CreatureGrade.None)
	{
		ClearPetList();
		List<PetInfo> petInfoList = _dataProvider.GetPetInfoList();
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petInfoList); i++)
		{
			PetInfo petInfo = petInfoList[i];
			if (petInfo == null)
			{
				continue;
			}
			CreatureData creatureData = petInfo.CreatureData;
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
				bool mainPet = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex == petInfo.PetIndex;
				bool sleep = PetHelper.CheckIsSleep(petInfo);
				PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(PetScrollGrid, PetPortraitComponent.SmallPrefabAuto);
				petPortraitComponent.Open(petInfo, this);
				petPortraitComponent.SetNew(flag);
				petPortraitComponent.SetMainPet(mainPet);
				petPortraitComponent.SetSleep(sleep);
				if (selectedPetInfo == petInfo)
				{
					petPortraitComponent.SetSelection(select: true);
					petPortraitComponent.SetNew(show: false);
				}
				_petPortraitComponentList.Add(petPortraitComponent);
			}
		}
		UIHelper.ResetGridPosition(PetScrollGrid);
	}

	private void SetGradeList()
	{
		CreatureGrade[] obj = Enum.GetValues(typeof(CreatureGrade)) as CreatureGrade[];
		bool flag = false;
		CreatureGrade[] array = obj;
		foreach (CreatureGrade creatureGrade in array)
		{
			PetGradeTabComponent petGradeTabComponent = UIHelper.AddGridItem(GradeGrid, PetGradeTabComponent.PrefabAuto);
			petGradeTabComponent.Init(creatureGrade, this);
			petGradeTabComponent.SelectToggle(creatureGrade == _curToggleType);
			flag = _newMarkGradeDic.ContainsKey(creatureGrade) && _newMarkGradeDic[creatureGrade];
			petGradeTabComponent.SetNew(flag);
			_gradeComponentList.Add(petGradeTabComponent);
		}
		UIHelper.RepositionGrid(GradeGrid);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_dataProvider = null;
		_listener = null;
		if (_newMarkGradeDic != null)
		{
			_newMarkGradeDic.Clear();
		}
		ClearPetGradeList();
		ClearPetList();
	}

	private void ClearPetGradeList()
	{
		FactoryBase.ReleaseAndClearList(_gradeComponentList);
	}

	private void ClearPetList()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitComponentList);
	}

	void PetGradeTabComponent.IListener.OnClickTab(CreatureGrade grade)
	{
		if (_curToggleType != grade)
		{
			_curToggleType = grade;
			SetPetList(grade);
		}
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		if (_listener != null)
		{
			_listener.OnClick(comp);
		}
		Close();
	}
}

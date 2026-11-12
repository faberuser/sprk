using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.ClassBuff;

public class ClassBuffMain : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/ClassBuff/ClassBuffMain";

	public List<UIToggle> CreatureTypeCategoryToggleList = new List<UIToggle>();

	public ClassBuffMainInfo MainInfo;

	public ClassBuffMainInfoList MainInfoList;

	private CreatureTagType _currentCreatureTypeCategory;

	public IEnumerator Open(CreatureTagType type = CreatureTagType.Knight)
	{
		base.Open();
		Init(type);
		yield return null;
	}

	private void Init(CreatureTagType type)
	{
		_currentCreatureTypeCategory = type;
		SetCreatureCategoryToggleByCreatureType(_currentCreatureTypeCategory);
		ClassBuffWindowManager classBuffWindowManager = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
		if (classBuffWindowManager != null)
		{
			if (MainInfo != null)
			{
				MainInfo.Open(classBuffWindowManager);
			}
			if (MainInfoList != null)
			{
				MainInfoList.Open(classBuffWindowManager, type);
			}
			SetClassBuffMainInfo();
		}
	}

	public void SetClassBuffMainInfo()
	{
		SetClassBuffInfoList();
		SetClassBuffInfo();
	}

	private void SetClassBuffInfoList()
	{
		if (!(MainInfoList == null))
		{
			MainInfoList.SetClassBuffInfoList(_currentCreatureTypeCategory);
		}
	}

	private void SetClassBuffInfo()
	{
		if (!(MainInfo == null))
		{
			MainInfo.SetClassBuffInfo(_currentCreatureTypeCategory);
		}
	}

	private CreatureTagType GetCreatureTypeByCateogryToggle(UIToggle categoryToggle)
	{
		if (categoryToggle == null)
		{
			return CreatureTagType.None;
		}
		CreatureTypeSelector component = categoryToggle.gameObject.GetComponent<CreatureTypeSelector>();
		if (component == null)
		{
			return CreatureTagType.None;
		}
		return component.CreatureType;
	}

	private UIToggle GetSelectCreatureCategoryToggle()
	{
		foreach (UIToggle creatureTypeCategoryToggle in CreatureTypeCategoryToggleList)
		{
			if (creatureTypeCategoryToggle.value)
			{
				return creatureTypeCategoryToggle;
			}
		}
		return null;
	}

	private void SetCreatureCategoryToggleByCreatureType(CreatureTagType creatureType)
	{
		foreach (UIToggle creatureTypeCategoryToggle in CreatureTypeCategoryToggleList)
		{
			if (creatureType == GetCreatureTypeByCateogryToggle(creatureTypeCategoryToggle))
			{
				creatureTypeCategoryToggle.Set(state: true);
				break;
			}
		}
	}

	public void OnClickCreatureTypeCategory()
	{
		UIToggle selectCreatureCategoryToggle = GetSelectCreatureCategoryToggle();
		if (!(selectCreatureCategoryToggle == null))
		{
			CreatureTagType creatureTypeByCateogryToggle = GetCreatureTypeByCateogryToggle(selectCreatureCategoryToggle);
			if (creatureTypeByCateogryToggle != CreatureTagType.None && creatureTypeByCateogryToggle != CreatureTagType.End && _currentCreatureTypeCategory != creatureTypeByCateogryToggle)
			{
				_currentCreatureTypeCategory = creatureTypeByCateogryToggle;
				SetClassBuffMainInfo();
			}
		}
	}
}

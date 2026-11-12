using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.ClassBuff;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.ClassBuff;

public class ClassBuffMainInfo : MonoBehaviour
{
	public interface IListener
	{
		void Reset(CreatureTagType typeCategory);
	}

	public UIBundleTexSelector ClassTexturSelector;

	public UIButton ResetButton;

	public UIScrollGrid ClassBuffStatComponentScrollGrid;

	private List<ClassBuffStatComponent> _classBuffStatComponentList = new List<ClassBuffStatComponent>();

	private CreatureTagType _currentCreatureTypeCategory;

	private IListener _iListener;

	public void Open(IListener iListener)
	{
		Init(iListener);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
	}

	public void SetClassBuffInfo(CreatureTagType type)
	{
		_currentCreatureTypeCategory = type;
		SetClassInfo();
		SetClassStatInfo();
	}

	private void SetClassInfo()
	{
		if (!(ClassTexturSelector == null))
		{
			ClassTexturSelector.SelectEnum(_currentCreatureTypeCategory);
			SetResetButtonStatus();
		}
	}

	public void SetClassStatInfo()
	{
		FactoryBase.ReleaseAndClearList(_classBuffStatComponentList);
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return;
		}
		Dictionary<int, ClassBuffInfo> classBuffInfoDicByCreatureType = classBuffManager.GetClassBuffInfoDicByCreatureType(_currentCreatureTypeCategory);
		if (classBuffInfoDicByCreatureType == null)
		{
			return;
		}
		ClassBuffStatComponent classBuffStatComponent = null;
		ClassBuffData classBuffData = null;
		foreach (ClassBuffInfo value in classBuffInfoDicByCreatureType.Values)
		{
			if (value == null)
			{
				continue;
			}
			classBuffData = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(_currentCreatureTypeCategory, value.ClassBuffIndex, value.ClassBuffLevel);
			if (classBuffData != null && GameRoot.Instance.SkillDataContainer.GetData(classBuffData.SkillIndex) != null)
			{
				classBuffStatComponent = UIHelper.AddGridItem(ClassBuffStatComponentScrollGrid, ClassBuffStatComponent.PrefabAuto);
				if (!(classBuffStatComponent == null))
				{
					string classBuffDesc = GetClassBuffDesc(classBuffData);
					classBuffStatComponent.Open(classBuffDesc);
					_classBuffStatComponentList.Add(classBuffStatComponent);
				}
			}
		}
		UIHelper.RepositionGrid(ClassBuffStatComponentScrollGrid);
	}

	private void SetResetButtonStatus()
	{
		if (!(ResetButton == null))
		{
			bool flag = !SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper.CheckIsInit(_currentCreatureTypeCategory);
			UIHelper.EnableButton(ResetButton, flag, flag);
		}
	}

	public void OnClickReset()
	{
		if (_iListener != null)
		{
			_iListener.Reset(_currentCreatureTypeCategory);
		}
	}

	public static string GetClassBuffDesc(ClassBuffData classBuffData)
	{
		if (classBuffData == null)
		{
			return string.Empty;
		}
		string statValue = ClassBuffDataContainer.GetStatValue(classBuffData.SkillIndex, classBuffData.StatUsePercentage);
		if (string.IsNullOrEmpty(statValue))
		{
			return string.Empty;
		}
		return LocalizationManager.GetStringDirect(classBuffData.BuffOptionTitleDesc, statValue);
	}
}

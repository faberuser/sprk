using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.ClassBuff;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.ClassBuff;

public class ClassBuffMainInfoList : MonoBehaviour, ClassBuffComponent.IListener
{
	public interface IListener
	{
		void OpenLevelUpPopup(ClassBuffData data);
	}

	public UILabel ClassLabel;

	public UILabel ClassBuffPointLabel;

	public UILabel RemainClassBuffDescriptionLabel;

	public UIWidget IconParentWidget;

	public UIScrollGrid ClassBuffComponentScrollGrid;

	private IconProxy _iconProxy;

	private List<ClassBuffComponent> _classBuffComponentList = new List<ClassBuffComponent>();

	private IListener _iListener;

	public void Open(IListener iListener, CreatureTagType creatureTypeCategory)
	{
		Init(iListener, creatureTypeCategory);
	}

	private void Init(IListener iListener, CreatureTagType creatureTypeCategory)
	{
		_iListener = iListener;
		SetClassBuffInfo(creatureTypeCategory);
	}

	public void SetClassBuffInfoList(CreatureTagType creatureTypeCategory)
	{
		SetClassBuffInfo(creatureTypeCategory);
		FactoryBase.ReleaseAndClearList(_classBuffComponentList);
		AddClassBuffComponent(creatureTypeCategory);
		UIHelper.RepositionGrid(ClassBuffComponentScrollGrid);
	}

	private void SetClassBuffInfo(CreatureTagType creatureTypeCategory)
	{
		SetClassInfo(creatureTypeCategory);
		SetClassBuffIcon(creatureTypeCategory);
		SetRemainClassBuffDescription(creatureTypeCategory);
		SetClassBuffPointInfo(creatureTypeCategory);
	}

	private void SetClassInfo(CreatureTagType creatureTypeCategory)
	{
		if (!(ClassLabel == null))
		{
			UIHelper.SetLabelText(ClassLabel, creatureTypeCategory);
		}
	}

	private void SetClassBuffIcon(CreatureTagType creatureTypeCategory)
	{
		SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper?.SetClassBuffIcon(ref IconParentWidget, ref _iconProxy, creatureTypeCategory);
	}

	private void SetRemainClassBuffDescription(CreatureTagType creatureTypeCategory)
	{
		if (!(RemainClassBuffDescriptionLabel == null))
		{
			ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
			if (classBuffWrapper != null)
			{
				string text = classBuffWrapper.LocalizeClassBuffPointName(creatureTypeCategory);
				string stringDirect = LocalizationManager.GetStringDirect(classBuffWrapper.LocalizeClassBuffPointRemain, text);
				UIHelper.SetLabelText(RemainClassBuffDescriptionLabel, stringDirect);
			}
		}
	}

	private void SetClassBuffPointInfo(CreatureTagType creatureTypeCategory)
	{
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager != null)
		{
			long classBuffPointByCreatureType = classBuffManager.GetClassBuffPointByCreatureType(creatureTypeCategory);
			UIHelper.SetLabelText(ClassBuffPointLabel, Vespa_Long_Extension_Method.ToCommaString(classBuffPointByCreatureType));
		}
	}

	private void AddClassBuffComponent(CreatureTagType creatureTypeCategory)
	{
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper == null)
		{
			return;
		}
		foreach (ClassBuffData currentClassBuffData in classBuffWrapper.GetCurrentClassBuffDataList(creatureTypeCategory))
		{
			if (currentClassBuffData != null)
			{
				CreateClassBuffComponent(currentClassBuffData);
			}
		}
	}

	private void CreateClassBuffComponent(ClassBuffData data)
	{
		if (data != null)
		{
			ClassBuffComponent classBuffComponent = UIHelper.AddGridItem(ClassBuffComponentScrollGrid, ClassBuffComponent.PrefabAuto);
			if (!(classBuffComponent == null))
			{
				classBuffComponent.Open(this, data);
				_classBuffComponentList.Add(classBuffComponent);
			}
		}
	}

	public void PlayLevelUpEffect(int levelUpIndex)
	{
		foreach (ClassBuffComponent classBuffComponent in _classBuffComponentList)
		{
			if (!(classBuffComponent == null) && classBuffComponent.ClassBuffIndex == levelUpIndex)
			{
				classBuffComponent.PlayLevelUpEffect();
				break;
			}
		}
	}

	void ClassBuffComponent.IListener.OpenLevelUpPopup(ClassBuffData data)
	{
		if (_iListener != null)
		{
			_iListener.OpenLevelUpPopup(data);
		}
	}
}

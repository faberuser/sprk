using NGame2.NUI.NComponent2.ExtendMenu;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow.Mission;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ExtendMenuPanel : MonoBehaviour, ExtendMenuRoot.IListener
{
	public interface IListener
	{
		void SelectExtendMenu(int mainIndex, int subIndex);
	}

	public UIScrollGrid ExtendMenuScrollGrid;

	public ExtendMenuRoot ExtendMenuRoot;

	private IListener _iListener;

	public void Init(IListener iListener)
	{
		_iListener = iListener;
		InitExtendMenu();
	}

	private void InitExtendMenu()
	{
		if (!(ExtendMenuRoot == null) || !(ExtendMenuRoot.ExtendMenu == null))
		{
			ExtendMenuRoot.Init(this);
			ExtendMenuRoot.Apply();
			InitMenu();
			RepositionGrid();
		}
	}

	private void InitMenu()
	{
		Transform transform = ExtendMenuRoot.ExtendMenu.transform;
		if (transform == null)
		{
			return;
		}
		ExtendMenu[] componentsInChildren = transform.GetComponentsInChildren<ExtendMenu>(includeInactive: true);
		ExtendMenu extendMenu = null;
		MissionMain.EQuestType eQuestType = MissionMain.EQuestType.Daily;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			extendMenu = componentsInChildren[i];
			if (!(extendMenu == null))
			{
				eQuestType = (MissionMain.EQuestType)extendMenu.Index;
				if (eQuestType == MissionMain.EQuestType.Sub)
				{
					InitSubQuestMenu(extendMenu.transform);
				}
				extendMenu.LabelText = LocalizationManager.GetStringDirect(eQuestType.ToString().ToUpper() + "_MISSION");
				extendMenu.SetLabel();
			}
		}
	}

	private void InitSubQuestMenu(Transform parentTransform)
	{
		Transform transform = null;
		ExtendMenuComponent extendMenuComponent = null;
		for (int i = 0; i < parentTransform.childCount; i++)
		{
			transform = parentTransform.GetChild(i);
			if (!(transform == null))
			{
				extendMenuComponent = transform.GetComponent<ExtendMenuComponent>();
				if (!(extendMenuComponent == null))
				{
					extendMenuComponent.LabelText = SceneAutoBehaviour<MissionManagement>.instance.LocalizeSubQuestGroup(extendMenuComponent.Index);
					extendMenuComponent.SetLabel();
				}
			}
		}
	}

	private void RepositionGrid()
	{
		if (!(ExtendMenuScrollGrid == null))
		{
			ExtendMenuScrollGrid.RepositionGrid();
		}
	}

	public void SetNewExtendMenu(int mainIndex, int subIndex, bool isNew)
	{
		if (!(ExtendMenuRoot == null))
		{
			ExtendMenuRoot.SetNew(mainIndex, subIndex, isNew);
		}
	}

	public void SetEventOrBoosterExtendMenu(int mainIndex, bool isEvent, bool isBooster)
	{
		if (!(ExtendMenuRoot == null))
		{
			ExtendMenuRoot.SetEventOrBooster(mainIndex, isEvent, isBooster);
		}
	}

	void ExtendMenuRoot.IListener.SelectExtendMenu(int mainIndex, int subIndex)
	{
		if (_iListener != null)
		{
			_iListener.SelectExtendMenu(mainIndex, subIndex);
		}
	}
}

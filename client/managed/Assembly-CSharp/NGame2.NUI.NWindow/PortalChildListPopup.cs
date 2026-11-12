using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NodeFunctionType;
using NGame2.NUI.NWindow.NodeFunctionType.Condition;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PortalChildListPopup : BaseWindow, PortalComponent2.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PortalChildListPopup";

	public const string PrefabDungeonPath = "Resources/Prefab2/Window/MessagePopup/PortalDungeonListPopup";

	private const int UPPERDUNGEON_CHAPTERINDEX = 100;

	private const int UPPERDUNGEON_DUNGEONINDEX = 2;

	private const int CONQUESTDUNGEON_DUNGEONINDEX = 3;

	protected Action<int, int> _onClickSelectable;

	protected Action _onClickDisabled;

	protected List<PortalComponent2> _componentList = new List<PortalComponent2>();

	public UIScrollGrid ComponentGrid;

	public UILabel Label_Title;

	public int OriginChapterIndex;

	public int OriginDungeonIndex;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public static void OpenDungeonListPopup(StaminaType staminaType, Action<int, int> onClickSelectable, Action onClickDisabled)
	{
		int dungeonIdx = 0;
		switch (staminaType)
		{
		case StaminaType.HideoutKey:
			dungeonIdx = 2;
			break;
		case StaminaType.ConquestKey:
			dungeonIdx = 3;
			break;
		}
		OpenDungeonListPopup(100, dungeonIdx, onClickSelectable, onClickDisabled);
	}

	public static void Open(int ChapterIdx, int DungeonIdx, Action<int, int> onClickSelectable, Action onClickDisabled)
	{
		WindowPrefab<PortalChildListPopup> windowPrefab = new WindowPrefab<PortalChildListPopup>("Resources/Prefab2/Window/MessagePopup/PortalChildListPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(ChapterIdx, DungeonIdx, onClickSelectable, onClickDisabled, PortalComponent2.PrefabAuto);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenDungeonListPopup(int ChapterIdx, int DungeonIdx, Action<int, int> onClickSelectable, Action onClickDisabled)
	{
		WindowPrefab<PortalChildListPopup> windowPrefab = new WindowPrefab<PortalChildListPopup>("Resources/Prefab2/Window/MessagePopup/PortalDungeonListPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(ChapterIdx, DungeonIdx, onClickSelectable, onClickDisabled, PortalComponent2.PrefabAutoDungeonNameTag);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(int ChapterIdx, int DungeonIdx, Action<int, int> onClickSelectable, Action onClickDisabled, PrefabAuto<PortalComponent2> Component)
	{
		base.Open();
		_onClickDisabled = onClickDisabled;
		_onClickSelectable = onClickSelectable;
		foreach (PortalData allDatum in GameRoot.Instance.PortalDataContainer.GetAllData())
		{
			if ((allDatum.Hidden && !Helper.CheckDropBonusEventNode(allDatum.ChapterIndex, allDatum.DungeonIndex)) || !allDatum.hasParent() || allDatum.ParentNode[0] != ChapterIdx || allDatum.ParentNode[1] != DungeonIdx)
			{
				continue;
			}
			PortalComponent2 portalComponent = UIHelper.AddGridItem(ComponentGrid, Component);
			if (!String_Extension_Method.IsNullOrEmpty(allDatum.AdventureNoticeCode))
			{
				AdventureNoticeData data = GameRoot.Instance.AdventureNoticeDataContainer.GetData(allDatum.AdventureNoticeCode);
				AdventureNoticeComponent adventureNoticeComponent = portalComponent as AdventureNoticeComponent;
				if (adventureNoticeComponent == null)
				{
					continue;
				}
				adventureNoticeComponent.Open(this, allDatum.ChapterIndex, allDatum.DungeonIndex, allDatum.Name, allDatum.ImagePath, data);
			}
			else
			{
				portalComponent.Open(this, allDatum.ChapterIndex, allDatum.DungeonIndex, allDatum.Name, allDatum.ImagePath);
			}
			_componentList.Add(portalComponent);
		}
		UIHelper.RepositionGrid(ComponentGrid);
		ComponentGrid.ResetScroll();
		if (Component == PortalComponent2.PrefabAutoDungeonNameTag)
		{
			PortalData data2 = GameRoot.Instance.PortalDataContainer.GetData(ChapterIdx, DungeonIdx);
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(data2.Name));
			return;
		}
		PortalData data3 = GameRoot.Instance.PortalDataContainer.GetData(ChapterIdx, DungeonIdx);
		if (data3 != null && !String_Extension_Method.IsNullOrEmpty(data3.TitleDescKey))
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(data3.TitleDescKey));
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
		OriginChapterIndex = 0;
		OriginDungeonIndex = 0;
	}

	public override void Close()
	{
		base.Close();
		if (OriginChapterIndex != 0 && OriginDungeonIndex != 0)
		{
			NGame2.NAccount.UserCampaignManager.ChapterIndex = OriginChapterIndex;
			NGame2.NAccount.UserCampaignManager.DungeonIndex = OriginDungeonIndex;
		}
	}

	public bool CanEnter(int chpaterIndex)
	{
		IList<PortalData> allData = GameRoot.Instance.PortalDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			PortalData portalData = allData[i];
			if (portalData.ChapterIndex == chpaterIndex && portalData.HasChild)
			{
				if (__Extension_Method_ICollection__.IsNullOrEmpty(portalData.Condition))
				{
					return true;
				}
				string valueString = ArgParser.GetValueString(portalData.Condition, 0);
				ICondition condition = SingletonAuto<SwitchingContainer>.instance.GetCondition(valueString);
				if (condition == null)
				{
					return true;
				}
				return !condition.EnterConditionCheck(portalData.Condition);
			}
		}
		return true;
	}

	void PortalComponent2.Listener.SelectPortal(PortalComponent2 component)
	{
		if (component.Selectable())
		{
			int chapterIndex = component.ChapterIndex;
			int num = component.DungeonIndex;
			PortalData data = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, num);
			if (data.hasParent() && data.ParentNode[0] == 100 && data.ParentNode[1] == 2)
			{
				num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLatelyHighLevelDungeonIndex_Chapter(component.ChapterIndex);
			}
			if (CanEnter(component.ChapterIndex))
			{
				Vespa_System_Action_Extension_Method.fire(_onClickSelectable, chapterIndex, num);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(_onClickDisabled);
			}
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(_onClickDisabled);
		}
	}

	void PortalComponent2.Listener.SelectButton(PortalComponent2 component)
	{
	}

	void PortalComponent2.Listener.SelectEnableContentsShortCut(PortalComponent2 component)
	{
	}

	void PortalComponent2.Listener.SelectDisableContentsShortCut(string disableText)
	{
	}

	void PortalComponent2.Listener.StartDrag()
	{
	}

	void PortalComponent2.Listener.OnDrag()
	{
	}

	void PortalComponent2.Listener.EndDrag()
	{
	}

	bool PortalComponent2.Listener.CanOpenShorCutBtn()
	{
		return false;
	}
}

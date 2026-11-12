using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishScenarioListWindow : FrameControlWindow
{
	public static string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentDungeonListWindow";

	public UIScrollGrid ScrollGrid;

	private List<PunishScenarioListComponent> _listComponent = new List<PunishScenarioListComponent>();

	private PunishmentGroupData _scenarioData;

	private PunishScenarioListComponent.IListener _listener;

	public void Open(PunishmentGroupData scenarioData, PunishScenarioListComponent.IListener listener)
	{
		_scenarioData = scenarioData;
		_listener = listener;
		Open(Set, null);
	}

	private void Set()
	{
		SetScenarioComp();
	}

	private void SetScenarioComp()
	{
		ReleaseScenarioComp();
		if (_scenarioData == null)
		{
			return;
		}
		PunishScenarioListComponent punishScenarioListComponent = null;
		int num = 0;
		int chapterIndex = _scenarioData.ChapterIndex;
		IList<EventDungeonData> datasByChapterIndex = GameRoot.Instance.EventDungeonDataContainer.GetDatasByChapterIndex(chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datasByChapterIndex); i++)
		{
			EventDungeonData eventDungeonData = datasByChapterIndex[i];
			if (eventDungeonData == null)
			{
				continue;
			}
			string notOpenText = "";
			bool flag = UIHelper.CheckEventDungeonOpenCondition(eventDungeonData, out notOpenText);
			if (flag || eventDungeonData.CloseType != EEventDungeonCloseType.Hide)
			{
				PunishScenarioListComponent punishScenarioListComponent2 = UIHelper.AddGridItem(ScrollGrid, PunishScenarioListComponent.PreabAuto);
				if (punishScenarioListComponent2 == null)
				{
					break;
				}
				punishScenarioListComponent2.Open(_listener, eventDungeonData);
				_listComponent.Add(punishScenarioListComponent2);
				if (flag && num < eventDungeonData.DungeonIndex)
				{
					num = eventDungeonData.DungeonIndex;
					punishScenarioListComponent = punishScenarioListComponent2;
				}
			}
		}
		ScrollGrid.RepositionGrid();
		if ((bool)punishScenarioListComponent && num != 1)
		{
			ScrollGrid.ScrollTo(punishScenarioListComponent, 0f, 0f);
		}
		UIHelper.RepositionGrid(ScrollGrid);
	}

	private void ReleaseScenarioComp()
	{
		FactoryBase.ReleaseAndClearList(_listComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_scenarioData = null;
		_listener = null;
		ReleaseScenarioComp();
	}
}

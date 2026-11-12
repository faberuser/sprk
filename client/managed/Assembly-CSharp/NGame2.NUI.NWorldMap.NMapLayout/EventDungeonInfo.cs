using NShared.NWorldMap.NEventSubData;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class EventDungeonInfo : LinkFactoryComponent, InitOnCreate
{
	public static PrefabAuto<EventDungeonInfo> PrefabAuto = new PrefabAuto<EventDungeonInfo>("Prefab2/WorldMap/EventDungeonInfo");

	public EventDungeon EventDungeonData { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(EventDungeon eventDungeon)
	{
		EventDungeonData = eventDungeon;
	}
}

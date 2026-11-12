using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PunishScenarioListComponent : EventDungeonComponent
{
	public new interface IListener
	{
		void OnClickEnterDungeon(int chapterIndex, int dungeonIndex);
	}

	public static string ParefabPath2 = "Resources/Prefab2/Component/Raid/PunishmentScenarioListComponent";

	public static PrefabAuto<PunishScenarioListComponent> PreabAuto = new PrefabAuto<PunishScenarioListComponent>(ParefabPath2);

	private new IListener _listener;

	public void Open(IListener listener, EventDungeonData eventDungeonData)
	{
		Open((Listener)null, eventDungeonData);
		_listener = listener;
	}

	public override void OnClickEnter()
	{
		if (_listener != null)
		{
			int chapterIndex = GetChapterIndex();
			int dungeonIndex = GetDungeonIndex();
			_listener.OnClickEnterDungeon(chapterIndex, dungeonIndex);
		}
	}
}

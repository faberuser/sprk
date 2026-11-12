using NShared;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public interface IEventDungeonCondition
{
	bool Check(EventDungeonData dungeonData, string[] values, out string labelText);
}

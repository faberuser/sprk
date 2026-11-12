using NGame2.NBattleContext;

namespace NGame2.NUI.NWindow;

public interface IDungeonRemainTimer
{
	bool IsShowAtRemainTimeZero();

	int GetRemainTime(EnterDungeonData enterDungeon);
}

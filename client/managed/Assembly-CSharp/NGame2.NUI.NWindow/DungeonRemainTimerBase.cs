using NGame2.NBattleContext;

namespace NGame2.NUI.NWindow;

public class DungeonRemainTimerBase : IDungeonRemainTimer
{
	private IDungeonRemainTimer _parentTimer;

	private bool _showAtRemainTimeZero;

	public DungeonRemainTimerBase(IDungeonRemainTimer parent)
	{
		_parentTimer = parent;
	}

	public void SetShowAtRemainZero(bool show)
	{
		_showAtRemainTimeZero = show;
	}

	protected int GetParentRemainTime(EnterDungeonData enterDungeon)
	{
		if (_parentTimer == null)
		{
			return -1;
		}
		return _parentTimer.GetRemainTime(enterDungeon);
	}

	int IDungeonRemainTimer.GetRemainTime(EnterDungeonData enterDungeon)
	{
		return GetRemainTimeDerived(enterDungeon);
	}

	public virtual int GetRemainTimeDerived(EnterDungeonData enterDungeon)
	{
		return -1;
	}

	bool IDungeonRemainTimer.IsShowAtRemainTimeZero()
	{
		return _showAtRemainTimeZero;
	}
}

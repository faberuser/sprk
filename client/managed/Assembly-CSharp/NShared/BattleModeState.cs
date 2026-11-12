namespace NShared;

public abstract class BattleModeState<TState> : IBattleModeState where TState : IBattleModeState
{
	protected BattleModeUpdater _updater;

	private BattleModeState<TState> _nextState;

	IBattleModeState IBattleModeState.GetNextState()
	{
		return _nextState;
	}

	void IBattleModeState.Init()
	{
		OnInit();
	}

	void IBattleModeState.Update()
	{
		OnUpdate();
	}

	public void SetUpdater(BattleModeUpdater updater)
	{
		_updater = updater;
	}

	public void EndState(BattleModeState<TState> nextState)
	{
		_nextState = nextState;
		if (_nextState != null)
		{
			_nextState.SetUpdater(_updater);
		}
	}

	public virtual void OnInit()
	{
	}

	public virtual void OnUpdate()
	{
	}
}

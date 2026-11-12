namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class StateUpdater
{
	private State _state;

	public void SetState(State state)
	{
		_state = state;
		if (_state != null)
		{
			_state.OnStart();
		}
	}

	public void OnUpdate()
	{
		if (_state != null)
		{
			_state.OnUpdate();
			if (_state.IsEnd())
			{
				State nextState = _state.GetNextState();
				SetState(nextState);
			}
		}
	}

	public void Close()
	{
		_state = null;
	}

	public bool IsEnded()
	{
		return _state == null;
	}
}

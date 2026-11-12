namespace NGame2.NBattleField.NManager.NCampaign;

public class CampaignStateUpdater
{
	private StateBase.IListener _listener;

	private StateBase _state;

	public CampaignStateUpdater(StateBase.IListener listener)
	{
		_listener = listener;
	}

	public void SetState(StateBase state)
	{
		if (!Vespa_Common_Class_Extension_Method.EQUALS(_state, state))
		{
			if (_state != null)
			{
				_ = _state.GetType().Name;
			}
			_ = state?.GetType().Name;
			if (_state != null)
			{
				_state.OnEnd();
			}
			_state = state;
			if (_state != null)
			{
				_state.OnStart(_listener);
			}
		}
	}

	public void Update()
	{
		if (_state != null)
		{
			_state.ChainUpdate();
			StateBase stateBase = _state.PopNext();
			if (stateBase != null)
			{
				SetState(stateBase);
			}
		}
	}
}

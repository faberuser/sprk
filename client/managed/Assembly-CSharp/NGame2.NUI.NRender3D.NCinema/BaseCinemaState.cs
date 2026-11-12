using NVespa;

namespace NGame2.NUI.NRender3D.NCinema;

public abstract class BaseCinemaState : ICinemaState
{
	protected ICinemaOwner _owner;

	public abstract ECinemaState State { get; }

	protected Cinema _cinema
	{
		get
		{
			if (_owner == null)
			{
				return null;
			}
			return _owner.Cinema;
		}
		set
		{
			if (_owner != null)
			{
				_owner.Cinema = value;
			}
		}
	}

	public virtual bool OnStart(ICinemaOwner owner)
	{
		if (owner == null)
		{
			return false;
		}
		_owner = owner;
		return true;
	}

	public abstract void ChainUpdate();

	public abstract void NextState();

	public virtual void OnEnd()
	{
	}

	protected void SetNextState(ICinemaState state)
	{
		if (state == null)
		{
			DebugError("[NextState] NextState is Null");
		}
		else if (_owner != null)
		{
			_owner.SetState(state);
		}
		else
		{
			DebugError("[NextState] Owner is Null");
		}
	}

	protected void DebugError(string message)
	{
		message = $"[{State.ToString()}] Error : {message} ";
		if (_owner == null)
		{
			VespaDebug.LogError(message);
		}
		else
		{
			_owner.NoticeError(message);
		}
	}
}

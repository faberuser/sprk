namespace NGame2.NUI.NWindow.NBattle.NAnim;

public abstract class State
{
	protected bool _isEnd;

	public abstract State GetNextState();

	public abstract void OnStart();

	public virtual void OnUpdate()
	{
	}

	public virtual bool IsEnd()
	{
		return _isEnd;
	}

	protected void SetEnd(bool isEnd)
	{
		_isEnd = isEnd;
	}
}

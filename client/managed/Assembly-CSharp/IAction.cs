public abstract class IAction
{
	public DummyClient DummyClient { get; private set; }

	public void SetDummyClient(DummyClient dummyClient)
	{
		DummyClient = dummyClient;
	}

	public virtual void OnBegin()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnEnd()
	{
	}

	public virtual void OnGUI()
	{
	}
}

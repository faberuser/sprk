public sealed class SprkLobbyAccessoryLease
{
	private object owner;

	public bool Pending { get; private set; }

	public void Bind(object value)
	{
		owner = value;
		Pending = false;
	}

	public void Cancel()
	{
		Pending = false;
	}

	public bool Matches(object value)
	{
		if (value != null)
		{
			return owner == value;
		}
		return false;
	}

	public bool Queue(object value)
	{
		if (!Matches(value))
		{
			return false;
		}
		Pending = true;
		return true;
	}

	public bool Take(object value, bool ready)
	{
		if (!Matches(value) || !Pending || !ready)
		{
			return false;
		}
		Pending = false;
		return true;
	}
}

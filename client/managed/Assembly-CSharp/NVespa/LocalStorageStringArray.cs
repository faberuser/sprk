namespace NVespa;

public class LocalStorageStringArray
{
	private NVespa.LocalStorageGenericArray<string> _inner;

	public string this[long index]
	{
		get
		{
			return _inner[index];
		}
		set
		{
			_inner[index] = value;
		}
	}

	public int Count
	{
		get
		{
			if (_inner != null)
			{
				return _inner.Count;
			}
			return 0;
		}
	}

	public LocalStorageStringArray(string i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<string>(i_default, i_keyPrefix, SprkTestPrefs.GetString, SprkTestPrefs.SetString, EqualValue);
	}

	private bool EqualValue(string lhs, string rhs)
	{
		return lhs.Equals(rhs);
	}
}

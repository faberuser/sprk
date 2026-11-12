namespace NVespa;

public class LocalStorageIntArray
{
	private NVespa.LocalStorageGenericArray<int> _inner;

	public int this[long index]
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

	public LocalStorageIntArray(int i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<int>(i_default, i_keyPrefix, SprkTestPrefs.GetInt, SprkTestPrefs.SetInt, EqualValue);
	}

	private bool EqualValue(int lhs, int rhs)
	{
		return lhs == rhs;
	}
}

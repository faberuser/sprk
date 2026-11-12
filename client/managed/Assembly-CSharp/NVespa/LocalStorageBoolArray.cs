namespace NVespa;

public class LocalStorageBoolArray
{
	private NVespa.LocalStorageGenericArray<bool> _inner;

	public bool this[long index]
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

	public LocalStorageBoolArray(bool i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<bool>(i_default, i_keyPrefix, GetLocalValue, SetLocalValue, EqualValue);
	}

	private bool GetLocalValue(string key, bool defaultValue)
	{
		return SprkTestPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
	}

	private void SetLocalValue(string key, bool value)
	{
		SprkTestPrefs.SetInt(key, value ? 1 : 0);
	}

	private bool EqualValue(bool lhs, bool rhs)
	{
		return lhs == rhs;
	}
}

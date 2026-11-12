namespace NVespa;

public class LocalAccountStringArray
{
	private long _accountId = long.MinValue;

	private string _keyPrefix;

	private string _default;

	private LocalStorageStringArray _value;

	public string this[long index]
	{
		get
		{
			Init();
			return _value[index];
		}
		set
		{
			Init();
			_value[index] = value;
		}
	}

	public int Count
	{
		get
		{
			if (_value != null)
			{
				return _value.Count;
			}
			return 0;
		}
	}

	public LocalAccountStringArray(string i_default, string i_keyPrefix)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageStringArray(_default, _accountId + _keyPrefix);
		}
	}
}

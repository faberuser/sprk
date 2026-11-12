namespace NVespa;

public class LocalAccountIntArray
{
	private long _accountId = long.MinValue;

	private string _keyPrefix;

	private int _default;

	private LocalStorageIntArray _value;

	public int this[long index]
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

	public LocalAccountIntArray(int i_default, string i_keyPrefix)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
	}

	public void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageIntArray(_default, _accountId + _keyPrefix);
		}
	}
}

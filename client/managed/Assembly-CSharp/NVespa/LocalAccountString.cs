namespace NVespa;

public class LocalAccountString
{
	private long _accountId = long.MinValue;

	private string _key;

	private string _default;

	private LocalStorageString _value;

	public string Value
	{
		get
		{
			Init();
			return _value.Value;
		}
		set
		{
			Init();
			_value.Value = value;
		}
	}

	public LocalAccountString(string i_key)
	{
		_key = i_key;
		_default = string.Empty;
	}

	public LocalAccountString(string i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	public void Remove()
	{
		Init();
		_value.Remove();
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageString(_default, _accountId + _key);
		}
	}
}

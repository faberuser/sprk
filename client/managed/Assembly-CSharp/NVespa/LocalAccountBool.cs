namespace NVespa;

public class LocalAccountBool
{
	private long _accountId = long.MinValue;

	private string _key;

	private bool _default;

	private LocalStorageBool _value;

	public bool Value
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

	public LocalAccountBool(bool i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageBool(_default, _accountId + _key);
		}
	}
}

namespace NVespa;

public class LocalAccountBoolArray
{
	private long _accountId = long.MinValue;

	private string _keyPrefix;

	private bool _default;

	private LocalStorageBoolArray _value;

	public bool this[long index]
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

	public LocalAccountBoolArray(bool i_default, string i_keyPrefix)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageBoolArray(_default, _accountId + _keyPrefix);
		}
	}
}

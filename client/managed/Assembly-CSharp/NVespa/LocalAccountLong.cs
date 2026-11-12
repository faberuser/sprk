namespace NVespa;

public class LocalAccountLong
{
	private long _accountId = long.MinValue;

	private string _key;

	private long _default;

	private LocalStorageLong _value;

	public long Value
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

	public LocalAccountLong(long i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageLong(_default, _accountId + _key);
		}
	}
}

namespace NVespa;

public class LocalAccountEnum<T> where T : struct
{
	private long _accountId = long.MinValue;

	private string _key;

	private T _default;

	private LocalStorageEnum<T> _value;

	public T Value
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

	public LocalAccountEnum(string i_key)
	{
		_key = i_key;
		_default = default(T);
	}

	public LocalAccountEnum(T i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageEnum<T>(_default, _accountId + _key);
		}
	}
}

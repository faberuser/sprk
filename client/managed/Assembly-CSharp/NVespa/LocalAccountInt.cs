namespace NVespa;

public class LocalAccountInt
{
	private long _accountId = long.MinValue;

	private string _key;

	private int _default;

	private LocalStorageInt _value;

	public int Value
	{
		get
		{
			Init();
			return _value.Value;
		}
		set
		{
			Init();
			_value.Value = checked((int)value);
		}
	}

	public LocalAccountInt(int i_default, string i_key)
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
			_value = new LocalStorageInt(_default, _accountId + _key);
		}
	}
}

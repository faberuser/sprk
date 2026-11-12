namespace NVespa;

public class LocalAccountFloat
{
	private long _accountId = long.MinValue;

	private string _key;

	private float _default;

	private LocalStorageFloat _value;

	public float Value
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

	public LocalAccountFloat(float i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageFloat(_default, _accountId + _key);
		}
	}
}

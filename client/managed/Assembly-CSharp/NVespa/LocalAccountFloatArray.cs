namespace NVespa;

public class LocalAccountFloatArray
{
	private long _accountId = long.MinValue;

	private string _keyPrefix;

	private float _default;

	private LocalStorageFloatArray _value;

	public float this[long index]
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

	public LocalAccountFloatArray(float i_default, string i_keyPrefix)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageFloatArray(_default, _accountId + _keyPrefix);
		}
	}
}

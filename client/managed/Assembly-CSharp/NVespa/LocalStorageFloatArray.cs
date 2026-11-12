namespace NVespa;

public class LocalStorageFloatArray
{
	private NVespa.LocalStorageGenericArray<float> _inner;

	public float this[long index]
	{
		get
		{
			return _inner[index];
		}
		set
		{
			_inner[index] = value;
		}
	}

	public LocalStorageFloatArray(float i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<float>(i_default, i_keyPrefix, SprkTestPrefs.GetFloat, SprkTestPrefs.SetFloat, EqualValue);
	}

	private bool EqualValue(float lhs, float rhs)
	{
		return lhs == rhs;
	}
}

using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;

namespace NGame2.NUI.NRender3D.NCinema;

public class NodeTreeKeyword
{
	private StringKey _key;

	private long _uid;

	private string _str;

	private bool IsValid()
	{
		if (_key == null)
		{
			return false;
		}
		if (_uid == 0L && String_Extension_Method.IsNullOrEmpty(_str))
		{
			return false;
		}
		return true;
	}

	public NodeTreeKeyword(StringKey key, long uid)
	{
		_key = key;
		_uid = uid;
	}

	public NodeTreeKeyword(StringKey key, string str)
	{
		_key = key;
		_str = str;
	}

	public void RuntimeAddKeyword(BuildChildRootRuntime runtime)
	{
		if (IsValid())
		{
			if (_uid != 0L)
			{
				runtime.AddValue(_key, _uid);
			}
			else
			{
				runtime.AddValue(_key, _str);
			}
		}
	}

	public bool IsEqual(long uid)
	{
		if (_uid == 0L)
		{
			return false;
		}
		return _uid.Equals(uid);
	}

	public bool IsEqual(string str)
	{
		if (String_Extension_Method.IsNullOrEmpty(_str))
		{
			return false;
		}
		return _str.Equals(str);
	}

	public override string ToString()
	{
		if (_uid != 0L)
		{
			return $"String Key : {_key.ToString()} UID : {_uid.ToString()}";
		}
		return string.Format("String Key {0} Str : ", _key.ToString(), _str);
	}
}

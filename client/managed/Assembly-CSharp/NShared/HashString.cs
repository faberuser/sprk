using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HashString
{
	[Key(0)]
	public string _value;

	[Key(1)]
	public int Length => _value.Length;

	public HashString()
	{
	}

	private HashString(string value)
	{
		_value = HashStringContainer.GetHashString(value);
	}

	public string GetValue()
	{
		return _value;
	}

	public override string ToString()
	{
		return _value;
	}

	public bool Equals(HashString obj)
	{
		if ((object)obj != null)
		{
			return _value == obj._value;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is HashString hashString)
		{
			return _value == hashString._value;
		}
		if (obj is string text)
		{
			return _value == text;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public static bool operator ==(HashString a, HashString b)
	{
		return Equals(a, b);
	}

	public static bool operator !=(HashString a, HashString b)
	{
		return !Equals(a, b);
	}

	public static bool Equals(HashString a, HashString b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	public string[] Split(params char[] seperator)
	{
		return _value.Split(seperator);
	}

	public bool Contains(string value)
	{
		return _value.Contains(value);
	}

	public int IndexOf(char value)
	{
		return _value.IndexOf(value);
	}

	public string Substring(int startIndex)
	{
		return _value.Substring(startIndex);
	}

	public bool StartsWith(string value)
	{
		return _value.StartsWith(value);
	}

	public static implicit operator string(HashString sh)
	{
		if (!(sh != null))
		{
			return null;
		}
		return sh._value;
	}

	public static implicit operator HashString(string v)
	{
		if (v == null)
		{
			return null;
		}
		return new HashString(v);
	}
}

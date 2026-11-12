using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct LanguagePatternKey
{
	[Key(0)]
	public string ServerName;

	[Key(1)]
	public int Index;

	public LanguagePatternKey(string serverName, int index)
	{
		ServerName = serverName;
		Index = index;
	}

	public bool Equals(LanguagePatternKey other)
	{
		if (ServerName == other.ServerName)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ServerName, Index);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ServerName}, {Index})";
	}
}

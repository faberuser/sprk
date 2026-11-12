using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct HelpListDataKey
{
	[Key(0)]
	public string Code;

	[Key(1)]
	public int Page;

	public HelpListDataKey(string code, int page)
	{
		Code = code;
		Page = page;
	}

	public bool Equals(HelpListDataKey other)
	{
		if (Code == other.Code)
		{
			return Page == other.Page;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Code, Page);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Code}, {Page})";
	}
}

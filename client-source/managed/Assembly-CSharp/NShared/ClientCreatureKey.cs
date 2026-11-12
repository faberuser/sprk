using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ClientCreatureKey
{
	[Key(0)]
	public string Code;

	[Key(1)]
	public int CostumeIndex;

	public ClientCreatureKey(string code, int costumeIndex)
	{
		Code = code;
		CostumeIndex = costumeIndex;
	}

	public bool Equals(ClientCreatureKey other)
	{
		if (Code == other.Code)
		{
			return CostumeIndex == other.CostumeIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Code, CostumeIndex);
	}

	public override string ToString()
	{
		return $"Code={Code} CostumeIndex={CostumeIndex}";
	}

	public string GetStringKey()
	{
		if (CostumeIndex != 0)
		{
			return Code + CostumeIndex;
		}
		return Code;
	}
}

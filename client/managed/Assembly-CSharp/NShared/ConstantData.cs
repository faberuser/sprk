using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class ConstantData : IDataKeySelector<string>
{
	[Key(0)]
	public string Key { get; set; }

	[Key(1)]
	public string Value { get; set; }

	string IDataKeySelector<string>.Key => Key;

	public override string ToString()
	{
		return "[" + Key + "] = " + Value;
	}
}

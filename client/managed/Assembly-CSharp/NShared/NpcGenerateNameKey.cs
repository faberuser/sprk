namespace NShared;

public class NpcGenerateNameKey : Tuple<string, string>
{
	public string Name1 => Item1;

	public string Name2 => Item2;

	public NpcGenerateNameKey()
		: base((string)null, (string)null)
	{
	}

	public NpcGenerateNameKey(string name1, string name2)
		: base(name1, name2)
	{
	}
}

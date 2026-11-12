namespace NShared;

public class ClassBuffKey : Tuple<int, int>
{
	public ClassBuffKey()
		: base(0, 0)
	{
	}

	public ClassBuffKey(int tagType, int classBuffIndex)
		: base(tagType, classBuffIndex)
	{
	}
}

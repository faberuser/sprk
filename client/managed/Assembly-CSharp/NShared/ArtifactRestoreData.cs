using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ArtifactRestoreData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string MaterialItemCode { get; set; }

	[Key(2)]
	public int MaterialItemCount { get; set; }

	[Key(3)]
	public string ArtifactItemGroup { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}

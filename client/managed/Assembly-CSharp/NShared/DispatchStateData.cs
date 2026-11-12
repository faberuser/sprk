using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DispatchStateData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public DispatchState DispatchState { get; set; }

	[Key(2)]
	public string StateDescKey { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return $"Index={Index.ToString()}";
	}
}

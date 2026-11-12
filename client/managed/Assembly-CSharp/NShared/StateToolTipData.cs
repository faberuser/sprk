using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StateToolTipData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string StateType { get; set; }

	[Key(2)]
	public string StateName { get; set; }

	[Key(3)]
	public string StateDesc { get; set; }

	[Key(4)]
	public string IconPath { get; set; }
}

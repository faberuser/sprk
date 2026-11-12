using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class CinemaWindowData : IDataKeySelector<string>
{
	[Key(0)]
	public string Name { get; set; }

	[Key(1)]
	public bool IsOpenPlayOnce { get; set; }

	[Key(2)]
	public string OpenNodeTreePack { get; set; }

	[Key(3)]
	public string OpenNodeTreeName { get; set; }

	[Key(4)]
	public bool IsClosePlayOnce { get; set; }

	[Key(5)]
	public string CloseNodeTreePack { get; set; }

	[Key(6)]
	public string CloseNodeTreeName { get; set; }

	[Key(7)]
	public bool IsOwnerWindow { get; set; }

	string IDataKeySelector<string>.Key => Name;
}

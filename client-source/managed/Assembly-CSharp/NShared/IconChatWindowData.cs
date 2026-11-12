using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class IconChatWindowData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Order { get; set; }

	[Key(2)]
	public RankPanelType UIPanelType { get; set; }

	[Key(3)]
	public RankViewType ViewType { get; set; }

	[Key(4)]
	public string IconFile { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return Index.ToString();
	}
}

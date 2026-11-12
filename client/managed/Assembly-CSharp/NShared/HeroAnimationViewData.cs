using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class HeroAnimationViewData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int HeroIndex { get; set; }

	[Key(2)]
	public string Animation { get; set; }

	[Key(3)]
	public string AnimationText { get; set; }

	[Key(4)]
	public string IconType { get; set; }

	[Key(5)]
	public string VoiceCode { get; set; }

	[Key(6)]
	public bool SkipLegendCostume { get; set; }
}

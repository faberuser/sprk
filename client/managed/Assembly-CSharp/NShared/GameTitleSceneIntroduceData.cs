using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GameTitleSceneIntroduceData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public GameIntroduceType Type { get; set; }

	[Key(2)]
	public bool Show { get; set; }

	[Key(3)]
	public string HeroTitleStringKey { get; set; }

	[Key(4)]
	public string HeroNameStringKey { get; set; }

	[Key(5)]
	public string HeroDescStringKey1 { get; set; }

	[Key(6)]
	public string HeroDescStringKey2 { get; set; }

	[Key(7)]
	public string HeroPopupImage1 { get; set; }

	[Key(8)]
	public string HeroPopupImage2 { get; set; }

	[Key(9)]
	public string WorldTitleStringKey { get; set; }

	[Key(10)]
	public string WorldDescStringKey1 { get; set; }

	[Key(11)]
	public string WorldDescStringKey2 { get; set; }

	[Key(12)]
	public string WorldPopupImage1 { get; set; }

	[Key(13)]
	public string WorldPopupImage2 { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "GameTitleSceneIntroduceData Index=" + Index;
	}
}

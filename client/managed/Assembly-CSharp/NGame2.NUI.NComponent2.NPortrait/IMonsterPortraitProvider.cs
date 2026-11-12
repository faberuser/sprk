using NShared;

namespace NGame2.NUI.NComponent2.NPortrait;

public interface IMonsterPortraitProvider
{
	MonsterHeroInfo MonsterHeroInfo { get; }

	string PreviewBg { get; }
}

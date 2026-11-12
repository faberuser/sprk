using NShared;

namespace NGame2.NUI.NWindow;

public enum DragonRaidDifficulty
{
	[DragonRaidDifficulty(DragonRaidType = RaidType.DragonNormalSingle)]
	Normal,
	[DragonRaidDifficulty(DragonRaidType = RaidType.DragonHardSingle)]
	Hard
}

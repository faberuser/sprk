using System.Collections.Generic;

namespace NShared;

[BattlePacket]
public class BattleResultNot
{
	public bool Win { get; set; }

	public int Star { get; set; }

	public bool Dropped { get; set; }

	public Dictionary<int, int> KillCount { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public int MaxWaveIndex { get; set; }
}

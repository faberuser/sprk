using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class None : PhaseBase
{
	public None(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.None, listener)
	{
	}
}

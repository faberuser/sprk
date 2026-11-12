using NShared;

namespace NGame2.NUI.NComponent2;

public interface IArchiveHeroSubPanel
{
	void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null);

	void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null);
}

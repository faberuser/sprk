using System.Collections.Generic;
using NGame2.NTutorial;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class HeroComparer : Comparer<CachedHeroInfo>
{
	public new static HeroComparer Default = new HeroComparer();

	public override int Compare(CachedHeroInfo l, CachedHeroInfo r)
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return l.HeroIndex.CompareTo(r.HeroIndex);
		}
		int num = -l.BookmarkPriority.CompareTo(r.BookmarkPriority);
		if (num != 0)
		{
			return num;
		}
		num = -l.Transcended.CompareTo(r.Transcended);
		if (num != 0)
		{
			return num;
		}
		num = -l.Level.CompareTo(r.Level);
		if (num != 0)
		{
			return num;
		}
		num = -l.Star.CompareTo(r.Star);
		if (num != 0)
		{
			return num;
		}
		return l.HeroIndex.CompareTo(r.HeroIndex);
	}
}

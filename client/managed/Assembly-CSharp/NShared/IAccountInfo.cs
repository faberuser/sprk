using System.Collections.Generic;

namespace NShared;

public interface IAccountInfo
{
	IEnumerable<CachedHeroInfo> GetHeroInfos();
}

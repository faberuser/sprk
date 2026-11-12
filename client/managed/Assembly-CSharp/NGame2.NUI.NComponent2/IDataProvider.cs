using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NComponent2;

public interface IDataProvider
{
	CachedHeroInfo GetSelectedHeroInfo();

	CreatureData GetSelectedHeroData();

	List<CreatureData> GetHeroDataList();

	SkillAttrType GetRecommandedType();
}

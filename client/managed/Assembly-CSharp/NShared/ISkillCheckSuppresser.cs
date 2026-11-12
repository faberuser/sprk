using System.Collections.Generic;

namespace NShared;

public interface ISkillCheckSuppresser : ISnapshotObject
{
	IEnumerable<int> GetSkillIndexList();

	SkillCheckSuppressType GetSuppressType();
}

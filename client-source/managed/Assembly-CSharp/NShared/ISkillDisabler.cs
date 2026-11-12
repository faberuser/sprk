using System.Collections.Generic;

namespace NShared;

public interface ISkillDisabler : ISnapshotObject
{
	SkillDisablerType GetDisablerType();

	IEnumerable<int> GetSkillIndexList();

	void AddSkillIndex(int toSkillIndex);

	bool IsDisableAutoUse();
}

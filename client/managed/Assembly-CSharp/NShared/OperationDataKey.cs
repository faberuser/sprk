using NShared.NJit;

namespace NShared;

[JitKey]
public struct OperationDataKey
{
	public int SkillIndex;

	public int OperationNo;

	public OperationDataKey(int skill, int no)
	{
		SkillIndex = skill;
		OperationNo = no;
	}

	public bool Equals(OperationDataKey other)
	{
		if (SkillIndex == other.SkillIndex)
		{
			return OperationNo == other.OperationNo;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SkillIndex, OperationNo);
	}
}

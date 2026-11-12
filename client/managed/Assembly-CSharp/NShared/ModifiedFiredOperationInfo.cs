using System;

namespace NShared;

public class ModifiedFiredOperationInfo
{
	public bool IsValid;

	public FiredOperationInfo FiredOperationInfo;

	public int FixedDelayMs;

	public int DistanceDelayMs;

	public int DistanceFixedDelayMs;

	public void GetConditionValues(Action<string[]> callback)
	{
		if (FiredOperationInfo == null)
		{
			return;
		}
		BattleCreature firedCreature = FiredOperationInfo.FiredCreature;
		if (firedCreature == null)
		{
			return;
		}
		SkillData skillData = FiredOperationInfo.SkillData;
		if (skillData == null)
		{
			return;
		}
		OperationData operationData = FiredOperationInfo.OperationData;
		if (operationData == null)
		{
			return;
		}
		int index = skillData.Index;
		int operationNo = operationData.OperationNo;
		string[] groupConditionValues = operationData.GroupConditionValues;
		int num = ((groupConditionValues != null) ? groupConditionValues.Length : 0);
		using ArrayPool<string>.AutoArrayPoolReleaser autoArrayPoolReleaser = ArrayPool<string>.AllocAuto(num);
		for (int i = 0; i < num; i++)
		{
			string modifiedString = firedCreature.SkillController.ModifierContainer.GetModifiedString(index, operationNo, OperationFieldType.GroupConditionValue, i, groupConditionValues[i]);
			autoArrayPoolReleaser[i] = modifiedString;
		}
		callback?.Invoke(autoArrayPoolReleaser);
	}
}

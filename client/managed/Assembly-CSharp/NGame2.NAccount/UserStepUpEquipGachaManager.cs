using NShared;

namespace NGame2.NAccount;

public class UserStepUpEquipGachaManager : NShared.UserStepUpEquipGachaManager
{
	public void Apply(StepUpEquipGachaInfo[] infos)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			foreach (StepUpEquipGachaInfo info in infos)
			{
				Apply(info);
			}
		}
	}

	public void Apply(StepUpEquipGachaInfo info)
	{
		if (info == null)
		{
			return;
		}
		StepUpEquipGachaInfo stepUpEquipGachaInfo = Get(info.GachaIndex);
		if (stepUpEquipGachaInfo != null)
		{
			if (IsChanged(info, stepUpEquipGachaInfo))
			{
				ChangeInfo(info, stepUpEquipGachaInfo);
			}
		}
		else
		{
			OnAddData(info.GachaIndex, info);
		}
	}

	private bool IsChanged(StepUpEquipGachaInfo newInfo, StepUpEquipGachaInfo oldInfo)
	{
		if (newInfo.CompletedCount != oldInfo.CompletedCount)
		{
			return true;
		}
		if (newInfo.CompletedStep != oldInfo.CompletedStep)
		{
			return true;
		}
		if (newInfo.ProgressTime != oldInfo.ProgressTime)
		{
			return true;
		}
		return false;
	}

	private void ChangeInfo(StepUpEquipGachaInfo newInfo, StepUpEquipGachaInfo oldInfo)
	{
		oldInfo.CompletedCount = newInfo.CompletedCount;
		oldInfo.CompletedStep = newInfo.CompletedStep;
		oldInfo.ProgressTime = newInfo.ProgressTime;
	}
}

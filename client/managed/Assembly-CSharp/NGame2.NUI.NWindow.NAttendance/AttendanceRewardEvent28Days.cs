using NNguiExtension;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardEvent28Days : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardEvent28Days";

	public UILabelSelector LabelSelector;

	protected override void SetupCustom()
	{
		if (!(LabelSelector == null))
		{
			string text = ((base.AttendanceData != null) ? base.AttendanceData.Keyword.ToString() : "");
			bool enable = LabelSelector.Select(text);
			UIHelper.EnableGameObject(LabelSelector, enable);
		}
	}
}

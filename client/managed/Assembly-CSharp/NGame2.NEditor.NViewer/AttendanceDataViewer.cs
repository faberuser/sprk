using NGame2.NContentsInfo;
using NGame2.NUI3.NAttendance;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NEditor.NViewer;

public class AttendanceDataViewer : BaseGenericViewer<AttendanceData>
{
	protected override AttendanceData DrawValue(int index, AttendanceData value)
	{
		if (value != null)
		{
			base.DrawValue(index, value);
			if (GUILayout.Button("Open Window"))
			{
				AttendanceKeywordGroup attendanceGroup = AttendanceKeywordGroup.Helper.CreateByAnyKeyword(value.Keyword);
				AttendanceWindow attendanceWindow = SingletonAuto<AttendanceManager>.instance.GetAttendanceWindow(attendanceGroup);
				if (attendanceWindow != null)
				{
					attendanceWindow.OpenAttendance(attendanceGroup, openedFromReservePopup: false);
				}
			}
		}
		return value;
	}
}

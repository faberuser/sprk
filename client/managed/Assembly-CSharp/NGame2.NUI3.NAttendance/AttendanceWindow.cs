using System;
using NGame2.NContentsInfo;
using NGame2.NUI.NWindow;

namespace NGame2.NUI3.NAttendance;

public abstract class AttendanceWindow : BaseWindow
{
	public abstract void OpenAttendance(AttendanceKeywordGroup attendanceGroup, bool openedFromReservePopup, Action onComplete = null);

	public abstract void RefreshAttendance(AttendanceKeywordGroup attendanceGroup);

	public abstract void Setup_for_AttendanceIntegration();

	public abstract void ShowPurchaseButton(bool isShow);
}

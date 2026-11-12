using System;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NLogin;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EtcMenuWindow : BaseWindow
{
	public interface Listener
	{
		void OnClickNotice();

		void OnClickCalendar();

		void OnClickGuide();

		void OnClickPlayRecord();

		void OpenClassBuffMain();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Lobby/EtcMenuWindow";

	public UIGrid grid;

	public GameObject GoogleRoot;

	public GameObject Calendar_Alert;

	public Transform ClassBuffTranform;

	public UILabel Attendance_Label;

	private Listener _listener;

	public override void ReOpen()
	{
		base.ReOpen();
		Setup();
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		Setup();
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void RepositionGrid()
	{
		if (grid != null)
		{
			grid.Reposition();
		}
	}

	public void Setup()
	{
		SetupGoogleButton();
		SetupAttendanceLabel();
		SetEnableClassBuff();
		RepositionGrid();
	}

	private void SetupGoogleButton()
	{
		UIHelper.EnableGameObject(GoogleRoot, enable: false);
	}

	public void SetupAttendanceLabel()
	{
		if (!(Attendance_Label == null))
		{
			string localizeText_DefaultDailyAttendance = AttendanceFacade.GetLocalizeText_DefaultDailyAttendance(AttendanceFacade.GetAttendance_DefaultDailyAttendance().KeywordMain);
			UIHelper.SetLabelText(Attendance_Label, localizeText_DefaultDailyAttendance);
		}
	}

	private void SetEnableClassBuff()
	{
		bool enable = SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.ClassBuff);
		UIHelper.EnableGameObject(ClassBuffTranform, enable);
	}

	public void OnClickNotice()
	{
		if (_listener != null)
		{
			_listener.OnClickNotice();
		}
	}

	public void OnClickCalendar()
	{
		if (_listener != null)
		{
			_listener.OnClickCalendar();
			UIHelper.EnableGameObject(Calendar_Alert, enable: false);
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().UpdateEtcMenuAlert(enable: false);
		}
	}

	public void OnClickProcessAttendance()
	{
	}

	public void OnClickGuide()
	{
		if (_listener != null)
		{
			_listener.OnClickGuide();
		}
		Close();
	}

	public void OnClickGoogle()
	{
	}

	public void OnClickWebEvent()
	{
	}

	public void OnClickPlayRecord()
	{
		if (_listener != null)
		{
			_listener.OnClickPlayRecord();
		}
	}

	public void OnClickIGameOption()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenGameOptionWindow(GameOptionTab.Game);
		_ = (object)null;
		_ = 58;
		_ = 1;
		_ = 0;
		Array.Empty<object>();
	}

	public void OnClickMiniGame()
	{
		LoginMiniGame.OpenPopup(openAtPatchDownload: false);
	}

	public void OnClickOpenClassBuffMain()
	{
		if (_listener != null)
		{
			_listener.OpenClassBuffMain();
			Close();
		}
	}

	public void OnNewEventCalendar(int eventCalendarVersion)
	{
		if (SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.ETCMenu, ContentsStatusSubType.Calendar) == ContentsStatus.On)
		{
			int eventCalendarVersion2 = LoginData.EventCalendarVersion;
			if (eventCalendarVersion != eventCalendarVersion2)
			{
				UIHelper.EnableGameObject(Calendar_Alert, enable: true);
				SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().UpdateEtcMenuAlert(enable: true);
			}
		}
	}
}

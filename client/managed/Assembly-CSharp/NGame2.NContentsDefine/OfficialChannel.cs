using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;

namespace NGame2.NContentsDefine;

public class OfficialChannel
{
	public static int GetPatchNoteIndex()
	{
		GetPatchNoteInfo(out var patchNoteIndex, out var endTime);
		if (patchNoteIndex <= 0)
		{
			return 0;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out var o_dateTime))
		{
			return 0;
		}
		if (Common.GetUTCNow() > o_dateTime)
		{
			return 0;
		}
		return patchNoteIndex;
	}

	private static void GetPatchNoteInfo(out int patchNoteIndex, out string endTime)
	{
		if (!AccountManager.IsLogin())
		{
			patchNoteIndex = 0;
			endTime = string.Empty;
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.PatchNote != null && GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index > 0)
		{
			patchNoteIndex = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index;
			endTime = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.EndTime;
		}
		else if (LoginData.LastPatchNoteIndex <= 0)
		{
			patchNoteIndex = 0;
			endTime = string.Empty;
		}
		else
		{
			patchNoteIndex = LoginData.LastPatchNoteIndex;
			endTime = LoginData.LastPatchNoteEndTime;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NGame2.NLobby;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NNotice;

public class LobbyNoticePopup : BaseWindow, WebViewComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Notice/LobbyNoticePopup";

	public static LocalStorageString DisabledLobbyNoticeIndex = new LocalStorageString("", "DisabledLobbyNoticeIndex");

	public UITexture NoticeTexture;

	public UILabel MessageLabel;

	public GameObject DisableCheckedObject;

	public UIBundleLoader BundleLoader;

	public WebViewComponent WebView;

	private LobbyNotice _lobbyNotice;

	private LobbyNoticeLocalInfo _localInfo;

	public void Open(LobbyNotice lobbyNotice)
	{
		if (lobbyNotice == null)
		{
			Debug.Log("LobbyNotice is null!");
			return;
		}
		SystemLanguage currentLanguage = SingletonAuto<LocalizationManager>.instance.Language;
		_lobbyNotice = lobbyNotice;
		_localInfo = lobbyNotice.LocalInfo.FirstOrDefault((LobbyNoticeLocalInfo e) => string.Equals(e.Language, Vespa_String_Extension_Method.GetEnumString(currentLanguage), StringComparison.InvariantCultureIgnoreCase));
		if (_localInfo != null)
		{
			UIHelper.EnableGameObject(DisableCheckedObject, enable: false);
			Open(delegate
			{
				UIHelper.SetLabelText(MessageLabel, _localInfo.Message);
				ShowNoticePopup();
			}, null);
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (WebView != null)
		{
			WebView.Close();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			SingletonUpdater<PlainAssetManager>.instance.DestroyUnused(unusedAll: true);
		}
		if (UIHelper.IsEnabledGameObjet(DisableCheckedObject))
		{
			MakeDisableToday();
		}
	}

	public void OnClickDisableToday()
	{
		bool flag = !UIHelper.IsEnabledGameObjet(DisableCheckedObject);
		UIHelper.EnableGameObject(DisableCheckedObject, flag);
		if (flag)
		{
			Close();
		}
	}

	public void OnTouchImage()
	{
		if (_lobbyNotice == null)
		{
			return;
		}
		LobbyNoticeTaskInfo[] taskInfo = _lobbyNotice.TaskInfo;
		if (__Extension_Method_ICollection__.COUNT(taskInfo) == 0 || NoticeTexture == null)
		{
			return;
		}
		Vector3[] worldCorners = NoticeTexture.worldCorners;
		if (__Extension_Method_ICollection__.COUNT(worldCorners) < 2)
		{
			return;
		}
		int num = int.MinValue;
		LobbyNoticeTaskInfo[] array = taskInfo;
		for (int i = 0; i < array.Length; i++)
		{
			num = System.Math.Max(array[i].Position, num);
		}
		float y = worldCorners[0].y;
		float y2 = worldCorners[1].y;
		float y3 = UICamera.lastWorldPosition.y;
		float num2 = y2 - y;
		if (num2 < 0.001f)
		{
			return;
		}
		float num3 = Mathf.Clamp01((y2 - y3) / num2);
		int num4 = ((num < 0) ? 1 : (num + 1));
		int clickedPosition = (int)(num3 * (float)num4);
		LobbyNoticeTaskInfo lobbyNoticeTaskInfo = taskInfo.FirstOrDefault((LobbyNoticeTaskInfo e) => e.Position == clickedPosition);
		if (lobbyNoticeTaskInfo == null)
		{
			return;
		}
		string taskName = lobbyNoticeTaskInfo.TaskName;
		if (!string.IsNullOrEmpty(taskName))
		{
			string[] taskValues = null;
			if (lobbyNoticeTaskInfo.TaskValue != null)
			{
				taskValues = ((!lobbyNoticeTaskInfo.TaskValue.StartsWith("{")) ? lobbyNoticeTaskInfo.TaskValue.Split(new char[1] { ',' }) : new string[1] { lobbyNoticeTaskInfo.TaskValue });
			}
			SceneAutoBehaviour<ActivityManager>.instance.Run(taskName, taskValues);
			SingletonUpdater<Executer>.instance.DelayRun(0.5f, this, Close);
		}
	}

	private void ShowNoticePopup()
	{
		if (WebView == null)
		{
			return;
		}
		string imageUrl = _localInfo.ImageUrl;
		LobbyNoticeTaskInfo[] taskInfo = _lobbyNotice.TaskInfo;
		if (String_Extension_Method.IsNullOrEmpty(imageUrl))
		{
			VespaDebug.LogError("url is Null");
		}
		else if (new string[3] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(imageUrl).ToLower()))
		{
			string hyperlink = string.Empty;
			LobbyNoticeTaskInfo[] array = taskInfo;
			foreach (LobbyNoticeTaskInfo lobbyNoticeTaskInfo in array)
			{
				hyperlink = ((!String_Extension_Method.IsNullOrEmpty(lobbyNoticeTaskInfo.TaskName) && !(lobbyNoticeTaskInfo.TaskName == "None")) ? ((!(lobbyNoticeTaskInfo.TaskName == "OpenUrl")) ? ("kingsraid://" + lobbyNoticeTaskInfo.TaskName + "?value=" + lobbyNoticeTaskInfo.TaskValue) : lobbyNoticeTaskInfo.TaskValue) : "kingsraid://CloseAllPopup?value=1");
			}
			WebView.OpenUrl(MakeHtmlContent(imageUrl, hyperlink), this);
		}
		else
		{
			WebView.OpenUrl(imageUrl, this);
		}
	}

	private string MakeHtmlContent(string imgUrl, string hyperlink)
	{
		return "<html style='margin: 0;'><head><meta name='viewport' content='width=device-width, initial-scale = 1.0'></head><body style='margin: 0; height: 980px; overflow: hidden;'><a href= '" + hyperlink + "'><img src='" + imgUrl + "' style='width:100%; position: fixed; top:0;'></a></body></html>";
	}

	private void MakeDisableToday()
	{
		string value = DisabledLobbyNoticeIndex.Value;
		if (string.IsNullOrEmpty(value))
		{
			DisabledLobbyNoticeIndex.Value = _lobbyNotice.Index.ToString() ?? "";
			return;
		}
		bool flag = false;
		string[] array = value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], _lobbyNotice.Index.ToString()))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			LocalStorageString disabledLobbyNoticeIndex = DisabledLobbyNoticeIndex;
			disabledLobbyNoticeIndex.Value = disabledLobbyNoticeIndex.Value + "," + _lobbyNotice.Index;
		}
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		Close();
	}

	public static int[] GetDisabledLobbyNoticeIndices()
	{
		List<int> list = new List<int>();
		string value = DisabledLobbyNoticeIndex.Value;
		if (!string.IsNullOrEmpty(value))
		{
			string[] array = value.Split(',');
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
			{
				int result = 0;
				if (int.TryParse(array[i], out result) && result > 0)
				{
					list.Add(result);
				}
			}
		}
		return list.ToArray();
	}

	public static void ClearDisabledLobbyNoticeIndices()
	{
		DisabledLobbyNoticeIndex.Value = string.Empty;
	}
}

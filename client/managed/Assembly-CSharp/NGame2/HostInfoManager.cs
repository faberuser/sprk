using NGame2.NLogin;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2;

public class HostInfoManager : GlobalAutoBehaviour<HostInfoManager>
{
	private string textHostIP;

	public string ServerName { get; set; }

	public string HostIP { get; set; }

	public void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("Host Info", this, DrawGui);
	}

	public void DrawGui()
	{
		GUILayout.Label($"Server Name : {LoginData.LastServerName}");
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Host IP : ");
			textHostIP = HostIP;
			textHostIP = GUILayout.TextField(textHostIP);
		}
		if (SingletonUpdater<LoginManager>.instance.VersionInfo != null)
		{
			GUILayout.Label("Platform : " + SingletonUpdater<LoginManager>.instance.VersionInfo.Platform);
			GUILayout.Label("MinVersion : " + SingletonUpdater<LoginManager>.instance.VersionInfo.MinVersion);
			GUILayout.Label("PatchVersionUrl : " + SingletonUpdater<LoginManager>.instance.VersionInfo.PatchVersionUrl);
			GUILayout.Label("CdnProxyUrl : " + SingletonUpdater<LoginManager>.instance.VersionInfo.CdnProxyUrl);
			GUILayout.Label("CrashReport : " + SingletonUpdater<LoginManager>.instance.VersionInfo.CrashReport);
			GUILayout.Label("OffBackgroundDownload : " + SingletonUpdater<LoginManager>.instance.VersionInfo.OffBackgroundDownload);
			GUILayout.Label("DownloadFileWriteSleepTimeMS : " + SingletonUpdater<LoginManager>.instance.VersionInfo.DownloadFileWriteSleepTimeMS);
			GUILayout.Label("IsETC1TextureCdn : " + SingletonUpdater<LoginManager>.instance.VersionInfo.IsETC1TextureCdn);
			GUILayout.Label("LegacyLoadingBundlePack : " + SingletonUpdater<LoginManager>.instance.VersionInfo.LegacyLoadingBundlePack);
			GUILayout.Label("LegacyLoadingBundlePack_x86 : " + SingletonUpdater<LoginManager>.instance.VersionInfo.LegacyLoadingBundlePack_x86);
		}
	}
}

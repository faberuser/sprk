using System;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;

namespace NGameAsset.NPatch;

/// <summary>
/// SPRK: the game looks for new assets (LastBuildVersion.txt) only while logging in, and
/// nothing sends a running game back to login, so a player who keeps the game open or
/// resumes it (Android keeps it alive in the background) stays on old assets after a new
/// version is published. From the lobby, this checks the asset service when the game
/// comes back after a pause and every few minutes; when a newer version is active it
/// offers to return to the title, which downloads it (the same path as changing Texture).
/// </summary>
public static class AssetVersionWatcher
{
	private const float CheckInterval = 300f;

	// A gap this long between lobby frames means the game was paused or in the background.
	private const float ResumeGap = 30f;

	private static UnityWebRequest request;

	private static float nextCheck = CheckInterval;

	private static float lastTick;

	private static int offeredVersion;

	private static int pendingVersion;

	/// <summary>Called every lobby frame (LobbyManagement.ChainUpdate).</summary>
	public static void Tick()
	{
		float now = Time.realtimeSinceStartup;
		bool resumed = lastTick > 0f && now - lastTick > ResumeGap;
		lastTick = now;
		if (resumed)
		{
			// Ask again after a resume, even for a version the player declined.
			offeredVersion = 0;
		}
		if (request != null)
		{
			if (request.isDone)
			{
				Finish();
			}
			return;
		}
		if (pendingVersion > 0)
		{
			TryOffer();
			return;
		}
		if (!resumed && now < nextCheck)
		{
			return;
		}
		nextCheck = now + CheckInterval;
		string url = VersionUrl();
		if (url == null)
		{
			return;
		}
		request = UnityWebRequest.Get(url);
		request.timeout = 15;
		request.SendWebRequest();
	}

	private static string VersionUrl()
	{
		if (!SingletonUpdater<LoginManager>.isValidInstance || SingletonUpdater<LoginManager>.instance.VersionInfo == null)
		{
			return null;
		}
		string cdn = SingletonUpdater<LoginManager>.instance.VersionInfo.CdnProxyUrl;
		// Only SPRK's asset service (docs/game-assets.md) publishes new versions.
		if (string.IsNullOrEmpty(cdn) || cdn.IndexOf("/updates/assets/", StringComparison.Ordinal) < 0)
		{
			return null;
		}
		return cdn.CombineUrl("LastBuildVersion.txt").AppendRandomNumber("?");
	}

	private static void Finish()
	{
		UnityWebRequest done = request;
		request = null;
		try
		{
			if (done.result != UnityWebRequest.Result.Success || !GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
			{
				return;
			}
			string text = (done.downloadHandler.text ?? string.Empty).Trim();
			int dot = text.IndexOf('.');
			if (!int.TryParse((dot >= 0) ? text.Substring(0, dot) : text, out var remote))
			{
				return;
			}
			int current = GlobalAutoBehaviour<PatchDownloader>.instance.GetPatchVersion();
			if (current > 0 && remote > current && remote != offeredVersion)
			{
				pendingVersion = remote;
				TryOffer();
			}
		}
		finally
		{
			done.Dispose();
		}
	}

	private static void TryOffer()
	{
		if (GlobalAutoBehaviour<LoadingManager>.isValidInstance && GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			return;
		}
		offeredVersion = pendingVersion;
		pendingVersion = 0;
		MessagePopup.Open(SprkText.Get("New Game Data"), SprkText.Get("New game data is available. Return to the title screen to download it now?"), delegate
		{
			AccountManager.Logout();
			SingletonUpdater<LoginManager>.instance.ReturnToLogin();
		}, delegate
		{
		});
	}
}

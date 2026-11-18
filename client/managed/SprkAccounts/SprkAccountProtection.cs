using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Protection for the saved-login file (refresh tokens).
///
/// Windows: DPAPI, bound to the Windows user, as before.
/// Android: DPAPI does not exist. The file lives in the app's internal storage
/// (<see cref="StoreDirectory"/>), which only this app can read, and is kept as is
/// there; the Android Keystore would be the next hardening step.
/// The platform-specific calls sit in separate methods so an interpreter (HybridCLR on
/// Android) never prepares the DPAPI path on Android.
/// </summary>
public static class SprkAccountProtection
{
	private static bool UsesDpapi => Application.platform != RuntimePlatform.Android;

	public static byte[] Protect(byte[] plain)
	{
		return UsesDpapi ? DpapiProtect(plain) : (byte[])plain.Clone();
	}

	public static byte[] Unprotect(byte[] stored)
	{
		return UsesDpapi ? DpapiUnprotect(stored) : stored;
	}

	/// <summary>Root for the account store: the player data folder on Windows, the
	/// app-private internal files directory on Android (persistentDataPath is external
	/// storage there, readable by other apps on older Android versions).</summary>
	public static string StoreDirectory(string windowsDefault)
	{
		if (UsesDpapi)
		{
			return windowsDefault;
		}
		using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
		using AndroidJavaObject dir = activity.Call<AndroidJavaObject>("getFilesDir");
		return dir.Call<string>("getAbsolutePath");
	}

	private static byte[] DpapiProtect(byte[] plain)
	{
		return ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
	}

	private static byte[] DpapiUnprotect(byte[] stored)
	{
		return ProtectedData.Unprotect(stored, null, DataProtectionScope.CurrentUser);
	}
}

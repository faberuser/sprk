using NVespa.NSingleton;
using UnityEngine;

public class CustomTabsLauncher : GlobalAutoBehaviour<CustomTabsLauncher>
{
	private class CustomTabsCallback : AndroidJavaProxy
	{
		public CustomTabsCallback()
			: base("com.masang.customtab.CustomTabsManager$UnityCallback")
		{
		}

		public void onCustomTabsClosed()
		{
			Debug.Log("Custom Tabs was closed.");
			CustomTabsLauncher.OnCustomTabsClosed?.Invoke();
		}
	}

	public static event CustomTabsClosedDelegate OnCustomTabsClosed;

	public void LaunchCustomTab(string url)
	{
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}
}

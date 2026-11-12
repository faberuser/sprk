using System;
using Microsoft.Win32;
using UnityEngine;

public class GameRegistryManager : MonoBehaviour
{
	private void Awake()
	{
		RegisterGameInstallationPath();
	}

	public static void RegisterGameInstallationPath()
	{
		try
		{
			string text = Application.streamingAssetsPath.Replace('/', '\\') + "\\kingsraid.exe";
			string text2 = "kingsraid";
			string subkey = "Software\\Classes\\" + text2;
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(subkey);
			registryKey.SetValue("", "URL:" + text2);
			registryKey.SetValue("URL Protocol", "");
			registryKey.CreateSubKey("DefaultIcon").SetValue("", text);
			registryKey.CreateSubKey("shell\\open\\command").SetValue("", text + " \"%1\"");
		}
		catch (Exception ex)
		{
			Debug.LogError("\ufffd\ufffd\ufffd\ufffd \ufffd\ufffdθ\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd\ufffdƮ\ufffd\ufffd\ufffd\ufffd \ufffd\ufffd\ufffd\ufffdϴ\ufffd \ufffd\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd \ufffd\u07fb\ufffd\ufffd\u07fd\ufffd\ufffdϴ\ufffd: " + ex.Message);
		}
	}
}

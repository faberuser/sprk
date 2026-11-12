using System;
using System.Runtime.InteropServices;
using System.Text;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NDevice;

public class DeviceHelper
{
	public static DeviceBuildInfo DeviceBuildEmpty = new DeviceBuildInfo();

	private static string ScriptingBackend = string.Empty;

	public static string GetCpuFamily()
	{
		return "mono";
	}

	public static bool IsCpuFamily_x86()
	{
		return Vespa_Common_Class_Extension_Method.EQUALS(GetCpuFamily(), "x86");
	}

	public static string GetScriptingBackend()
	{
		if (!String_Extension_Method.IsNullOrEmpty(ScriptingBackend))
		{
			return ScriptingBackend;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Mono ");
		stringBuilder.Append("(.NET 4.6) ");
		stringBuilder.Append("(C# 7.3 or newer) ");
		ScriptingBackend = stringBuilder.ToString();
		return ScriptingBackend;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetUserDefaultGeoName([Out] char[] geoName, int geoNameCount);

	public static string GetCountryCode()
	{
		try
		{
			char[] array = new char[10];
			if (GetUserDefaultGeoName(array, array.Length) > 0)
			{
				return new string(array).TrimEnd('\0');
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
		return "US";
	}

	public static bool IsDeviceRooted()
	{
		return false;
	}

	public static bool IsModInstalled()
	{
		return false;
	}

	public static EVideoSpecType GetCurrentVideoSpec()
	{
		return EVideoSpecType.High;
	}

	public static string GetSigningKey()
	{
		return "UNITY";
	}

	public static DeviceBuildInfo GetBuildInfo()
	{
		return DeviceBuildEmpty;
	}

	public static string GetKRConnectorPreference()
	{
		return null;
	}

	public static void SendCSEmail(string mailTo, string mailTitle, string message)
	{
		bool flag = false;
		try
		{
			if (!flag)
			{
				string text = EscapeURL(mailTitle);
				string text2 = EscapeURL(message);
				Application.OpenURL("mailto:" + mailTo + "?subject=" + text + "&body=" + text2);
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}

	private static string EscapeURL(string url)
	{
		return WWW.EscapeURL(url).Replace("+", "%20");
	}

	public static int GetAndroidSDKInt()
	{
		return -1;
	}

	public static bool IsHardwardAcceratedCodec()
	{
		return false;
	}

	public static bool CheckNativeClassExists(string className)
	{
		return true;
	}
}

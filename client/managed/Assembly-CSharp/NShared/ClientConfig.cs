using System;
using System.Globalization;

namespace NShared;

[JsonSerializable]
public class ClientConfig
{
	public string Version { get; set; }

	public string TightVersion => GetTightVersion();

	public string Name { get; set; }

	public ClientBuildType Build { get; set; }

	public string BuildStep { get; set; }

	public string Platform { get; set; }

	public string BundleIdentifier { get; set; }

	public string ProductName { get; set; }

	public string QueryHost { get; set; }

	public string ForcedServerName { get; set; }

	public string Define { get; set; }

	public string GcmRegisterId { get; set; }

	public bool ShowEditorGui { get; set; }

	public string BattleVersion { get; set; }

	public string ExpireDate { get; set; }

	public bool EditorDefault { get; set; }

	public bool ShowClientConfig { get; set; }

	public string ForceBuildVersion { get; set; }

	public bool IsForceBuildVersion => !string.IsNullOrEmpty(ForceBuildVersion);

	public bool IsOBB { get; set; }

	public bool IsPTS { get; set; }

	public string SecretKey { get; set; }

	public string[] GradleProfile { get; set; }

	public bool GetRemainExpireSecond(out int remainSecond)
	{
		remainSecond = 0;
		if (!string.IsNullOrEmpty(ExpireDate) && DateTime.TryParseExact(ExpireDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			remainSecond = (int)(result - DateTime.UtcNow).TotalSeconds;
			return true;
		}
		return false;
	}

	public void SetVersion(int majorVersion, int minorVersion, int patchVersion)
	{
		Version = $"{majorVersion:D2}.{minorVersion:D2}.{patchVersion:D3}";
	}

	public void SetVersion(string version)
	{
		ParseVersion(version, out var majorVersion, out var minorVersion, out var patchVersion);
		SetVersion(majorVersion, minorVersion, patchVersion);
	}

	private static int GetVersionToken(string versionString, int index)
	{
		string[] array = versionString.Split(new char[1] { '.' });
		if (index < array.Length)
		{
			int result = 0;
			int.TryParse(array[index], out result);
			return result;
		}
		return 0;
	}

	public static void ParseVersion(string versionString, out int majorVersion, out int minorVersion, out int patchVersion)
	{
		majorVersion = GetVersionToken(versionString, 0);
		minorVersion = GetVersionToken(versionString, 1);
		patchVersion = GetVersionToken(versionString, 2);
	}

	public string GetTightVersion()
	{
		ParseVersion(Version, out var majorVersion, out var minorVersion, out var patchVersion);
		return GetTightVersion(majorVersion, minorVersion, patchVersion);
	}

	public long GetNormalizedVersion()
	{
		return GetNormalizedVersion(Version);
	}

	public static string GetTightVersion(int majorVersion, int minorVersion, int patchVersion)
	{
		return $"{majorVersion:D}.{minorVersion:D}.{patchVersion:D}";
	}

	public static long GetNormalizedVersion(int majorVersion, int minorVersion, int patchVersion)
	{
		return (long)majorVersion * 1000000L + minorVersion * 1000 + patchVersion;
	}

	public static long GetNormalizedVersion(string version)
	{
		ParseVersion(version, out var majorVersion, out var minorVersion, out var patchVersion);
		return GetNormalizedVersion(majorVersion, minorVersion, patchVersion);
	}
}

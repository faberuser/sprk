using System.Collections.Generic;

namespace NShared.NContentsDefine;

[JsonSerializable]
public class OfficialChannel : ContentsDefine.IDefine
{
	[JsonSerializable]
	public class Channel
	{
		public string comment { get; set; }

		public bool IsEnable { get; set; }

		public string Url { get; set; }

		public string Icon { get; set; }

		public string Name_Local { get; set; }

		public bool IsNewVisible { get; set; }
	}

	[JsonSerializable]
	public class Setting
	{
		public string comment { get; set; }

		public bool IsDefault { get; set; }

		public bool ShowLobbyButton { get; set; }

		public bool NewMarkInLobbyButton { get; set; }

		public bool OpenInAppBrowser { get; set; }

		public List<Channel> Channels { get; set; }

		public static Setting CreateDefault(bool isDefault = false)
		{
			Setting setting = new Setting();
			setting.IsDefault = isDefault;
			setting.ShowLobbyButton = true;
			setting.Channels = new List<Channel>();
			for (int i = 0; i < 3; i++)
			{
				setting.Channels.Add(new Channel
				{
					IsEnable = true,
					Url = "url",
					Icon = "icon",
					Name_Local = "localization_code",
					IsNewVisible = (i == 0)
				});
			}
			return setting;
		}
	}

	public Dictionary<string, Setting> Settings { get; set; }

	public OfficialChannel()
	{
		Settings = new Dictionary<string, Setting>();
		Settings.Add("America", Setting.CreateDefault(isDefault: true));
		Settings.Add("Korea", Setting.CreateDefault());
		Settings.Add("Japan", Setting.CreateDefault());
		Settings.Add("Europe", Setting.CreateDefault());
		Settings.Add("Asia", Setting.CreateDefault());
		Settings.Add("TW,HK,MO", Setting.CreateDefault());
	}

	public Setting GetSetting(string serverName, out string selectedServer)
	{
		selectedServer = "(null)";
		if (Settings == null || Settings.Count == 0)
		{
			return new Setting();
		}
		if (Settings.TryGetValue(serverName, out var value))
		{
			selectedServer = serverName;
			return value;
		}
		foreach (KeyValuePair<string, Setting> setting in Settings)
		{
			if (value == null)
			{
				value = setting.Value;
				selectedServer = setting.Key;
			}
			if (setting.Value.IsDefault)
			{
				value = setting.Value;
				selectedServer = setting.Key;
				break;
			}
		}
		return value;
	}
}

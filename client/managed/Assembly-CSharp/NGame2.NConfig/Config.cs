using NGame2.NLogin;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NConfig;

public class Config : SingletonAuto<Config>
{
	public ClientConfig ClientConfig { get; private set; }

	public GlobalClientConfig GlobalClientConfig { get; private set; }

	public Config()
	{
		LoadConfigFromResource();
	}

	private void LoadConfigFromResource()
	{
		TextAsset textAsset = VespaResource.Load<TextAsset>("Config/Config");
		if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
		{
			VespaDebug.LogError("Failed to open config file.");
			return;
		}
		SetClientConfig(JsonMarshaler.Parse<ClientConfig>(textAsset.text));
		TextAsset textAsset2 = VespaResource.Load<TextAsset>("Config/GlobalConfig");
		if (textAsset2 != null && !string.IsNullOrEmpty(textAsset2.text))
		{
			GlobalClientConfig = JsonMarshaler.Parse<GlobalClientConfig>(textAsset2.text);
		}
	}

	public void SetClientConfig(ClientConfig clientConfig)
	{
		ClientConfig = clientConfig;
		OverwriteVersion();
		ClientConfig.SetVersion(ClientConfig.Version);
	}

	private void OverwriteVersion()
	{
		if (!String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.BinaryVersionNumber.Value))
		{
			string version = ClientConfig.Version;
			string value = SingletonUpdater<LoginManager>.instance.BinaryVersionNumber.Value;
			if (version != value)
			{
				ClientConfig.Version = value;
			}
		}
	}
}

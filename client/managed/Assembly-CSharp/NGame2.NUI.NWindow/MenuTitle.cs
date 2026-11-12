using System;
using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;

namespace NGame2.NUI.NWindow;

public class MenuTitle : BaseWindow, IInspectorGuiDrawer
{
	public enum EIconType
	{
		Shop,
		SoulShop,
		BookShop,
		GoblinShop,
		Prison,
		Arena,
		Temple,
		Square
	}

	[Serializable]
	public class TitleData
	{
		public string Name;

		public LocalizeLinkData Text = new LocalizeLinkData();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EntryMenu/MenuTitle";

	public UILabel TextLabel;

	public UISpriteSelector IconSelector;

	public UITexture TextureGuildLogo;

	public void Open(string EntryNPCCode)
	{
		Open(delegate
		{
			Init(EntryNPCCode);
		}, null);
	}

	public void Init(string EntryNPCCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(EntryNPCCode))
		{
			InitNpcTitle(null);
			return;
		}
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(EntryNPCCode);
		if (data == null || String_Extension_Method.IsNullOrEmpty(data.MenuIconType))
		{
			InitNpcTitle(null);
		}
		else
		{
			InitNpcTitle(data);
		}
	}

	private void InitGuildTitle(string titleText, string iconKey)
	{
		UIHelper.EnableGameObject(IconSelector, enable: false);
		UIHelper.EnableGameObject(TextureGuildLogo, enable: true);
		int num = 1;
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L)
		{
			num = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo;
			if (num <= 0 || num > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIcon"))
			{
				num = 1;
			}
			titleText = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name;
		}
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(num);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureGuildLogo, guildLogoIcon);
		UIHelper.SetLabelText(TextLabel, titleText);
	}

	private void InitNormalTitle(string titleText, string iconKey)
	{
		UIHelper.EnableGameObject(IconSelector, enable: true);
		UIHelper.EnableGameObject(TextureGuildLogo, enable: false);
		if (IconSelector != null)
		{
			IconSelector.Select(iconKey);
		}
		UIHelper.SetLabelText(TextLabel, titleText);
	}

	private void InitNpcTitle(NPCData npcData)
	{
		string titleText;
		string text;
		if (npcData == null)
		{
			titleText = "Empty";
			text = string.Empty;
		}
		else
		{
			titleText = LocalizationManager.GetStringDirect(npcData.MenuTitleKey);
			text = npcData.MenuIconType;
		}
		if (string.Equals(text, "Guild", StringComparison.OrdinalIgnoreCase))
		{
			InitGuildTitle(titleText, text);
		}
		else
		{
			InitNormalTitle(titleText, text);
		}
	}
}

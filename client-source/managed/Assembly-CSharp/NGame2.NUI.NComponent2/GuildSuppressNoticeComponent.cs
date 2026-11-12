using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressNoticeComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressNoticeComponent";

	public static PrefabAuto<GuildSuppressNoticeComponent> PrefabAuto = new PrefabAuto<GuildSuppressNoticeComponent>("Prefab2/Component/GuildSuppress/GuildSuppressNoticeComponent");

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open()
	{
	}

	public void OnClick()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_SUPPRESS_ICON_MESSAGE"));
	}
}

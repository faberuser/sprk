using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildRaidLevelComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickLevel(int index);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/GuildRaid/GuildRaidLevelComponent";

	public static PrefabAuto<GuildRaidLevelComponent> PrefabAuto = new PrefabAuto<GuildRaidLevelComponent>("Resources/Prefab2/Component/GuildRaid/GuildRaidLevelComponent");

	public UILabel NameLabel;

	public UISprite SelectedSprite;

	private Listener _Listener;

	public int Index { get; private set; }

	public void SetData(int index, Listener listener)
	{
		Index = index;
		_Listener = listener;
		UIHelper.SetLabelText(NameLabel, LocalizationManager.GetString(GuildRaid.GUILDRAID_SECTOR_NAME, index));
	}

	public void OnClickLevel()
	{
		if (_Listener != null)
		{
			_Listener.OnClickLevel(Index);
		}
	}

	public void ShowSelectImage(bool isShow)
	{
		UIHelper.EnableGameObject(SelectedSprite, isShow);
	}
}

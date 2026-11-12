using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressBanComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressBanComponent";

	public static PrefabAuto<GuildSuppressBanComponent> PrefabAuto = new PrefabAuto<GuildSuppressBanComponent>("Prefab2/Component/GuildSuppress/GuildSuppressBanComponent");

	public UIWidget RootWidget;

	public UIGrid Grid;

	public UILabel Label_session;

	public UISprite Sprite_Cursor;

	private List<GuildSuppressBanSubComponent> _subComponentList = new List<GuildSuppressBanSubComponent>();

	public GuildSuppressSessionData GuildSuppressSessionData;

	public int currentSessionIndex;

	public int index;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_subComponentList);
	}

	public void Open(GuildSuppressSessionData guildSuppressSessionData, int _currentSessionIndex, int _index)
	{
		GuildSuppressSessionData = guildSuppressSessionData;
		currentSessionIndex = _currentSessionIndex;
		index = _index;
		int[] suppressDungeonIndex = guildSuppressSessionData.SuppressDungeonIndex;
		foreach (int dungeonIndex in suppressDungeonIndex)
		{
			GuildSuppressBanSubComponent guildSuppressBanSubComponent = GuildSuppressBanSubComponent.PrefabAuto.Factory.Create(Grid.transform);
			guildSuppressBanSubComponent.Open(dungeonIndex);
			_subComponentList.Add(guildSuppressBanSubComponent);
		}
		Grid.Reposition();
		if (Label_session != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_SESSION_NUMBER", index);
			UIHelper.SetLabelText(Label_session, stringDirect);
		}
		SetState();
	}

	public void SetState()
	{
		if (!(RootWidget == null))
		{
			bool grayScale = GuildSuppressSessionData.SessionIndex < currentSessionIndex;
			UIHelper.SetWidgetGrayScale(RootWidget, grayScale);
			bool enable = GuildSuppressSessionData.SessionIndex == currentSessionIndex;
			UIHelper.EnableGameObject(Sprite_Cursor, enable);
		}
	}
}

using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildArenaMemberDeckComponent : GuildArenaMemberComponent
{
	public new const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaMemberDeckComponent";

	public new static PrefabAuto<GuildArenaMemberDeckComponent> PrefabAuto = new PrefabAuto<GuildArenaMemberDeckComponent>("Prefab2/Component/GuildArena/GuildArenaMemberDeckComponent");

	public UISprite SelectedSprite;

	protected override void InitLifes()
	{
		if (_memberInfo.DefeatDeck != null)
		{
			for (int i = 0; i < 5; i++)
			{
				UIHelper.EnableGameObject(Sprite_Lifes[i].transform.parent.gameObject, enable: false);
				UIHelper.EnableGameObject(Sprite_Lifes[i], enable: false);
			}
			for (int j = 0; j < _memberInfo.DeckCount; j++)
			{
				int num = ((!Vespa_Generic_IList_Extension_Method.IsValidIndex(_memberInfo.DefeatDeck, j)) ? 1 : _memberInfo.DefeatDeck[j]);
				UIHelper.EnableGameObject(Sprite_Lifes[j].transform.parent.gameObject, enable: true);
				UIHelper.EnableGameObject(Sprite_Lifes[j], num != 1);
			}
			Grid_Life.Reposition();
		}
	}

	public void ShowSelectImage(bool isShow)
	{
		UIHelper.EnableGameObject(SelectedSprite, isShow);
	}
}

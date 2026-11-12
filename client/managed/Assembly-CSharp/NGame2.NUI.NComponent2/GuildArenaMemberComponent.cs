using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildArenaMemberComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickDeckInfo(GuildArenaMemberDeckInfo deckInfo);

		void OnClickDeckAttack(GuildArenaMemberDeckInfo deckInfo);
	}

	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaMemberComponent";

	public static PrefabAuto<GuildArenaMemberComponent> PrefabAuto = new PrefabAuto<GuildArenaMemberComponent>("Prefab2/Component/GuildArena/GuildArenaMemberComponent");

	private Listener _listener;

	public UIBundleLoader HeroBundleTex;

	public UILabel Label_Level;

	public UILabel Label_Nick;

	public UIButton Button_Info;

	public UIButton Button_Attack;

	public UISprite[] Sprite_Lifes;

	public UIGrid Grid_Life;

	public GameObject Enemy_Sprite_BG;

	protected GuildArenaMemberDeckInfo _memberInfo;

	private TeamType _teamType;

	public GuildArenaMemberDeckInfo MemberDeckInfo => _memberInfo;

	public void Open(Listener listener, TeamType teamType, GuildArenaMemberDeckInfo memberInfo)
	{
		_listener = listener;
		_teamType = teamType;
		_memberInfo = memberInfo;
		Refresh();
		InitAvatar();
		InitButtons();
		InitLabels();
		InitLifes();
		InitBG();
	}

	public void Refresh()
	{
		UISprite[] sprite_Lifes = Sprite_Lifes;
		foreach (UISprite obj in sprite_Lifes)
		{
			UIHelper.EnableGameObject(obj.transform.parent.gameObject, enable: false);
			UIHelper.EnableGameObject(obj, enable: false);
		}
	}

	private void InitAvatar()
	{
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(_memberInfo.AvatarHeroIndex);
		string codeName = avatarInfo.GetCreatureData().CodeName;
		int star = avatarInfo.Star;
		int costumeIndex = avatarInfo.CostumeIndex;
		string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(codeName, star, costumeIndex);
		UIHelper.SetBundleTexture(HeroBundleTex, battleHeroPortraitPath);
	}

	private void InitButtons()
	{
		UIHelper.EnableGameObject(Button_Info, _teamType == TeamType.Left);
		UIHelper.EnableGameObject(Button_Attack, _teamType == TeamType.Right);
	}

	private void InitLabels()
	{
		UIHelper.SetLabelText(Label_Level, string.Format("{0} {1}", "Lv", _memberInfo.TeamLevel));
		UIHelper.SetLabelText(Label_Nick, _memberInfo.Nick);
	}

	protected virtual void InitLifes()
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
				int num = (Vespa_Generic_IList_Extension_Method.IsValidIndex(_memberInfo.DefeatDeck, j) ? _memberInfo.DefeatDeck[j] : 0);
				UIHelper.EnableGameObject(Sprite_Lifes[j].transform.parent.gameObject, enable: true);
				UIHelper.EnableGameObject(Sprite_Lifes[j], num != 1);
			}
			Grid_Life.Reposition();
		}
	}

	private void InitBG()
	{
		UIHelper.EnableGameObject(Enemy_Sprite_BG, _teamType == TeamType.Right);
	}

	public void OnClickDeckInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickDeckInfo(_memberInfo);
		}
	}

	public void OnClickDeckAttack()
	{
		if (_listener != null)
		{
			_listener.OnClickDeckAttack(_memberInfo);
		}
	}
}

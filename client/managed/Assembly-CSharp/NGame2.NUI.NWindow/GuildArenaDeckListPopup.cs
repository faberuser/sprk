using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildArenaDeckListPopup : BaseWindow, HeroPortraitComponent.Listener, GuildArenaDeckComponent.IListener
{
	public interface Listener
	{
		bool HasPrevMember(GuildArenaMemberDeckInfo deckInfo, TeamType _teamType);

		bool HasNextMember(GuildArenaMemberDeckInfo deckInfo, TeamType _teamType);

		void OnClickPrevDeck(GuildArenaDeckListPopup caller, GuildArenaMemberDeckInfo deckInfo, TeamType _teamType);

		void OnClickNextDeck(GuildArenaDeckListPopup caller, GuildArenaMemberDeckInfo deckInfo, TeamType _teamType);
	}

	private Listener _listener;

	public UILabel Label_Name;

	public UIButton Button_Prev;

	public UIButton Button_Next;

	public UIScrollGrid DeckListGrid;

	private List<GuildArenaDeckComponent> _deckList = new List<GuildArenaDeckComponent>();

	private TeamType _teamType;

	private GuildArenaMemberDeckInfo _memberInfo;

	private GuildArenaDeckInfo[] _deckInfos;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaDeckListPopup";

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Open(Listener listener, GuildArenaMemberDeckInfo memberInfo, GuildArenaDeckInfo[] deckInfos, TeamType teamType, Action onOpen = null)
	{
		_listener = listener;
		_teamType = teamType;
		_memberInfo = memberInfo;
		_deckInfos = deckInfos;
		base.Open(delegate
		{
			Init();
		});
	}

	public override void Close()
	{
		base.Close();
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_deckList);
	}

	public void Init()
	{
		Clear();
		InitDeckList();
		InitLabels();
		InitPrevNextButton();
	}

	public void InitDeckList()
	{
		int deckCount = _memberInfo.DeckCount;
		FactoryBase.ReleaseAndClearList(_deckList);
		for (int i = 0; i < deckCount; i++)
		{
			CreateDeckItem(i, _deckInfos[i]);
		}
		UIHelper.RepositionGrid(DeckListGrid);
	}

	private void CreateDeckItem(int deckIndex, GuildArenaDeckInfo deckInfo)
	{
		GuildArenaDeckComponent guildArenaDeckComponent = UIHelper.AddGridItem(DeckListGrid, GuildArenaDeckComponent.PrefabAuto2);
		guildArenaDeckComponent.Init(deckIndex + 1, 3, this, _teamType, GuildArenaDeckComponent.ComponentType.OnAttack);
		guildArenaDeckComponent.SetDeckInfo(deckInfo);
		_deckList.Add(guildArenaDeckComponent);
	}

	public void InitLabels()
	{
		UIHelper.SetLabelText(Label_Name, _memberInfo.Nick);
	}

	public void InitPrevNextButton()
	{
		if (_listener != null)
		{
			bool enable = _listener.HasPrevMember(_memberInfo, _teamType);
			bool enable2 = _listener.HasNextMember(_memberInfo, _teamType);
			UIHelper.EnableButton(Button_Prev, enable, grayscaleIfDisable: true);
			UIHelper.EnableButton(Button_Next, enable2, grayscaleIfDisable: true);
		}
	}

	public void OnClickPrevDeck()
	{
		if (_listener != null)
		{
			_listener.OnClickPrevDeck(this, _memberInfo, _teamType);
		}
	}

	public void OnClickNextDeck()
	{
		if (_listener != null)
		{
			_listener.OnClickNextDeck(this, _memberInfo, _teamType);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void GuildArenaDeckComponent.IListener.OnClickDeckHeroPortrait(HeroPortraitComponent component)
	{
	}

	void GuildArenaDeckComponent.IListener.OnClickStartBattle(GuildArenaDeckInfo deckInfo, CachedHeroInfo[] heroInfos)
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaBattlePartySetting(deckInfo, _memberInfo, heroInfos);
	}

	void GuildArenaDeckComponent.IListener.OnClickDeck(GuildArenaDeckComponent component)
	{
	}

	void GuildArenaDeckComponent.IListener.OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName)
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaDeckDetailPopup(heroInfos, titleName);
	}
}

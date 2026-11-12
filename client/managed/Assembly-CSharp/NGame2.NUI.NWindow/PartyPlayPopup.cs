using NGame2.NBattleContext;
using NShared;

namespace NGame2.NUI.NWindow;

public class PartyPlayPopup : BaseWindow
{
	public interface Listener
	{
		void OnClick_PartySearch(EnterDungeonData enterDungeonInfo);

		void OnClick_PartyCreate(EnterDungeonData enterDungeonInfo);

		void OnClick_PartyList();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PartyDungeon/PartyPlayPopup";

	private ChapterDungeonInfo _chapterDungeonInfo;

	private EnterDungeonData _enterDungeonInfo;

	private Listener _listener;

	public void Open(EnterDungeonData enterDungeonInfo, Listener listener)
	{
		_listener = listener;
		_enterDungeonInfo = enterDungeonInfo;
		base.Open();
	}

	public void OnClickPartySearch()
	{
		if (_listener != null)
		{
			_listener.OnClick_PartySearch(_enterDungeonInfo);
		}
		Close();
	}

	public void OnClickPartyCreate()
	{
		if (_listener != null)
		{
			_listener.OnClick_PartyCreate(_enterDungeonInfo);
		}
		Close();
	}

	public void OnClickPartyList()
	{
		if (_listener != null)
		{
			_listener.OnClick_PartyList();
		}
		Close();
	}

	public void OnClickClose()
	{
		Close();
	}
}

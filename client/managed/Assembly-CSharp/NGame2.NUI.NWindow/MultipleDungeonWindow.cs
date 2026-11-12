using System.Collections.Generic;
using NGame2.NUI.NComponent2.NMultipleDungeon;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class MultipleDungeonWindow : BaseWindow, MultipleDungeonComponent.Listener, LobbyTopmostMenu.ICustomStaminaObserver
{
	public interface Listener
	{
		void EnterDungeon(UnderPrisonData underPrisonData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MultipleDungeon/MultipleDungeonWindow";

	public UIScrollGrid DungeonGrid;

	private List<MultipleDungeonComponent> _dungeonList = new List<MultipleDungeonComponent>();

	private Listener _listener;

	public StaminaType StaminaType => StaminaType.None;

	public void Open(Listener listener)
	{
		Open(delegate
		{
			_listener = listener;
			SetDungeonList();
		}, null);
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_dungeonList);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}

	public void SetDungeonList()
	{
		Clear();
		IList<UnderPrisonData> allData = GameRoot.Instance.UnderPrisonDataContainer.GetAllData();
		UnderPrisonData underPrisonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonData = allData[i];
			CreateDungeon(underPrisonData);
		}
		UIHelper.RepositionGrid(DungeonGrid);
	}

	private void CreateDungeon(UnderPrisonData underPrisonData)
	{
		if (underPrisonData != null)
		{
			MultipleDungeonComponent multipleDungeonComponent = UIHelper.AddGridItem(DungeonGrid, MultipleDungeonComponent.PrefabAuto);
			if (!(multipleDungeonComponent == null))
			{
				multipleDungeonComponent.Open(this, underPrisonData);
				_dungeonList.Add(multipleDungeonComponent);
			}
		}
	}

	public override void OnSystemBackButton()
	{
		Close();
	}

	void MultipleDungeonComponent.Listener.EnterDungeon(UnderPrisonData underPrisonData)
	{
		if (_listener != null)
		{
			_listener.EnterDungeon(underPrisonData);
		}
	}
}

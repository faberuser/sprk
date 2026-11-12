using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class GuildArenaMatchResult : BaseWindow
{
	public interface Listener
	{
		void OnArena(GuildArenaMatchResult caller);

		void OnStatistics(GuildArenaMatchResult caller);

		void OnEffectEnd(GuildArenaMatchResult caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/GuildArenaMatchResult";

	public GameObject WinObject;

	public GameObject LoseObject;

	public GameObject ButtonRoot;

	public float EffectDuration = 3f;

	public UIButton LobbyButton;

	public UIButton StatisticsButton;

	public Transform EffectRoot;

	public UIGame3DWidget EffectWin;

	public UIGame3DWidget EffectLose;

	public UILabel Label_Score;

	public UILabel Label_ScoreDiff;

	public UISimpleGrid SimpleGrid;

	private Listener _listener;

	private float _startTime;

	private bool _effectEnded;

	private float _openedTime;

	private bool _win = true;

	private int _mySessionScore;

	private int _addSessionScroe;

	private List<ItemIconComponent> _listItemComponent = new List<ItemIconComponent>();

	public bool Win => _win;

	public void Open(Listener listener, int win, int mySessionScore, int addSessionScore, ItemResultInfo[] itemResultInfos)
	{
		_listener = listener;
		_win = win == 1;
		_mySessionScore = mySessionScore;
		_addSessionScroe = addSessionScore;
		Open(delegate
		{
			Init();
			SetItemComponent(itemResultInfos);
		}, null);
	}

	public void Init()
	{
		_startTime = Time.time;
		_effectEnded = false;
		_openedTime = Time.time;
		UIHelper.EnableGameObject(WinObject, _win);
		UIHelper.EnableGameObject(LoseObject, !_win);
		UIHelper.EnableGameObject(ButtonRoot, enable: false);
		UIHelper.SetLabelText(Label_Score, _mySessionScore);
		UIHelper.SetLabelText(Label_ScoreDiff, $"(+{_addSessionScroe})");
		(_win ? EffectWin : EffectLose).GetComponent<UIGame3DWidget>().PlayNow();
		PlaySound(_win);
	}

	private void RelaseItemIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_listItemComponent);
	}

	private void SetItemComponent(ItemResultInfo[] itemResults)
	{
		RelaseItemIconComponent();
		if (SimpleGrid == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemResults); i++)
		{
			ItemResultInfo itemResultInfo = itemResults[i];
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemResultInfo.ItemIndex);
			if (data != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(SimpleGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(data);
					int addCount = itemResultInfo.AddCount;
					Color? color = null;
					int num = addCount;
					itemIconComponent.SetItemCount(num, color);
				}
			}
		}
		UIHelper.RepositionGrid(SimpleGrid);
	}

	private void PlaySound(bool win)
	{
		if (win)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Campaign.Victory",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.VictoryBGM", isEvent: true);
		}
		else
		{
			SoundStock stock2 = new SoundStock
			{
				Code = "UI.Campaign.Lose",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock2, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.LoseBGM", isEvent: true);
		}
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void UpdateDerived()
	{
		UpdateEffectEnd();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		RelaseItemIconComponent();
	}

	private void UpdateEffectEnd()
	{
		if (!_effectEnded && Time.time - _startTime >= EffectDuration)
		{
			_effectEnded = true;
			if (_listener != null)
			{
				_listener.OnEffectEnd(this);
			}
			UIHelper.EnableGameObject(ButtonRoot, enable: true);
		}
	}

	public void OnClickArena()
	{
		if (_effectEnded && _listener != null)
		{
			_listener.OnArena(this);
		}
	}

	public void OnClickStatistics()
	{
		if (_effectEnded && _listener != null)
		{
			_listener.OnStatistics(this);
		}
	}
}

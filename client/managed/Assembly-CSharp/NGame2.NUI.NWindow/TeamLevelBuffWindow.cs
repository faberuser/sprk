using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TeamLevelBuffWindow : BaseWindow, TeamLevelBuffComponent.IListener, IUserObserver<TeamLevelBuff>, IUserObserver
{
	public interface IListener
	{
		void RequestReinforceTeamLevelBuff(TeamLevelReinforceData reinforceData, Action onComplete);
	}

	public static string PrefabPath = "Prefab2/Window/GameOption/TeamBuffWindow";

	public UIGrid BuffListGrid;

	public UIScrollView BuffListScrillView;

	private Dictionary<int, TeamLevelBuffComponent> _buffCompDic = new Dictionary<int, TeamLevelBuffComponent>();

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			GenerateComponents();
		});
	}

	public override void Reset()
	{
		FactoryBase.ReleaseAndClearList(_buffCompDic);
		if (GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.ObserverManager.RemoveObserver(this);
		}
	}

	public override void UpdateDerived()
	{
		foreach (KeyValuePair<int, TeamLevelBuffComponent> item in _buffCompDic)
		{
			item.Value.RefreshPhuraseButton();
		}
	}

	private void GenerateComponents()
	{
		_buffCompDic.Clear();
		GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.ObserverManager.AddObserver(this);
		foreach (int openAbleBuffTeamLevel in GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetOpenAbleBuffTeamLevelList())
		{
			TeamLevelBuffComponent teamLevelBuffComponent = TeamLevelBuffComponent.PrefabAuto.Factory.Create(BuffListGrid.transform);
			if (!(teamLevelBuffComponent == null))
			{
				teamLevelBuffComponent.Open(this, openAbleBuffTeamLevel);
				_buffCompDic.Add(openAbleBuffTeamLevel, teamLevelBuffComponent);
			}
		}
		BuffListGrid.Reposition();
		BuffListScrillView.ResetPosition();
	}

	public void OnClickExitButton()
	{
		Reset();
		Close();
	}

	public void OnClickOpenTeamLevelTotalBuffPopup()
	{
		TeamLevelTotalBuffPopup.OpenPopup();
	}

	void TeamLevelBuffComponent.IListener.ReinforceTeamLevelBuff(TeamLevelReinforceData reinforceData, Action onComplete)
	{
		if (_listener != null)
		{
			_listener.RequestReinforceTeamLevelBuff(reinforceData, onComplete);
		}
	}

	void IUserObserver<TeamLevelBuff>.OnChanged(TeamLevelBuff changedInfo)
	{
		int openBuffLevel = changedInfo.OpenBuffLevel;
		if (_buffCompDic.ContainsKey(openBuffLevel))
		{
			TeamLevelBuffComponent teamLevelBuffComponent = _buffCompDic[openBuffLevel];
			if (teamLevelBuffComponent != null)
			{
				teamLevelBuffComponent.Refresh();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}

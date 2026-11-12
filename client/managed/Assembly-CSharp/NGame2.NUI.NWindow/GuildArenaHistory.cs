using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.GetGuildArenaMemberRecord;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaHistory : BaseWindow
{
	public enum EToggle
	{
		None,
		MyHistory,
		GuildAttack,
		GuildDefense
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaHistory";

	public UIScrollGrid Grid;

	public UIToggle ToggleMyHistory;

	public UIToggle ToggleGuildAttack;

	public UIToggle ToggleGuildDefense;

	public UILabel Label_Desc;

	public GameObject NotExistRoot;

	private List<GuildArenaHistoryComponent> _componentList;

	private EToggle _toggle;

	private WorldBossInfo _worldBossInfo;

	private DateTime _endLocalTime;

	private string _endDayOfWeek;

	private GuildArenaRecordType _currentRecordType;

	private GuildArenaMemberRecordInfo[] _memberInfos;

	public new void Open()
	{
		base.Open();
		InitToggle();
	}

	private void InitToggle()
	{
		_toggle = EToggle.MyHistory;
		ToggleMyHistory.Set(state: true);
		ToggleGuildAttack.Set(state: false);
		ToggleGuildDefense.Set(state: false);
		ShowMyHistory();
	}

	public void Setup(GuildArenaMemberRecordInfo[] memberInfos)
	{
		if (_componentList == null)
		{
			_componentList = new List<GuildArenaHistoryComponent>();
		}
		ClearList();
		if (memberInfos == null || __Extension_Method_ICollection__.COUNT(memberInfos) <= 0)
		{
			UIHelper.EnableGameObject(NotExistRoot, enable: true);
			return;
		}
		UIHelper.EnableGameObject(NotExistRoot, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(memberInfos); i++)
		{
			GuildArenaMemberRecordInfo recordInfo = memberInfos[i];
			GuildArenaHistoryComponent guildArenaHistoryComponent = UIHelper.AddGridItem(Grid, GuildArenaHistoryComponent.PrefabAuto);
			guildArenaHistoryComponent.Init(recordInfo, _currentRecordType);
			_componentList.Add(guildArenaHistoryComponent);
		}
		UIHelper.RepositionGrid(Grid);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
	}

	public void OnClickClose()
	{
		Close();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleMyHistory.value)
		{
			eToggle = EToggle.MyHistory;
		}
		else if (ToggleGuildAttack.value)
		{
			eToggle = EToggle.GuildAttack;
		}
		else if (ToggleGuildDefense.value)
		{
			eToggle = EToggle.GuildDefense;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			if (_toggle == EToggle.MyHistory)
			{
				ShowMyHistory();
			}
			else if (_toggle == EToggle.GuildAttack)
			{
				ShowAttack();
			}
			else if (_toggle == EToggle.GuildDefense)
			{
				ShowDefense();
			}
		}
	}

	private void RequestGuildArenaHistory(GuildArenaRecordType recordType, Action<NShared.GetGuildArenaMemberRecord.Response> onComplete)
	{
		GuildArenaManager guildArenaManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager;
		GuildArenaRankerInfo myGuildInfo = guildArenaManager.GetMyGuildInfo();
		NShared.GetGuildArenaMemberRecord.Request request = new NShared.GetGuildArenaMemberRecord.Request
		{
			SessionIndex = guildArenaManager.GetGuildArenaSessionInfo().SessionIndex,
			ServerGroup = ((myGuildInfo != null) ? myGuildInfo.ServerGroup : string.Empty),
			GuildId = (myGuildInfo?.GuildId ?? 0),
			RecordType = recordType
		};
		_currentRecordType = recordType;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, response);
			}
		});
	}

	private void ShowMyHistory()
	{
		Debug.Log("ShowMy");
		RequestGuildArenaHistory(GuildArenaRecordType.MyRecord, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			Setup(response.MemberRecords);
		});
	}

	private void ShowAttack()
	{
		Debug.Log("ShowAttack");
		RequestGuildArenaHistory(GuildArenaRecordType.MyTeam, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			Setup(response.MemberRecords);
		});
	}

	private void ShowDefense()
	{
		Debug.Log("ShowDefense");
		RequestGuildArenaHistory(GuildArenaRecordType.EnemyTeam, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			Setup(response.MemberRecords);
		});
	}

	private void SetRewardDesc(string msg)
	{
		UIHelper.SetLabelText(Label_Desc, msg);
	}
}

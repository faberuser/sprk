using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildSuppressBanList : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressBanList";

	private GuildSuppressBanInfo GuildSuppressBanInfo;

	private List<GuildSuppressBanComponent> _banComponentList = new List<GuildSuppressBanComponent>();

	public UIGrid Grid;

	public UIScrollView ScrollView;

	public UIPanel Panel;

	public UILabel Label_Title;

	public UILabel Label_Desc;

	private int SeasonIndex;

	private int CurrentSessionIndex;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_banComponentList);
	}

	public void Open(GuildSuppressBanInfo _GuildSuppressBanInfo)
	{
		Open(null, delegate
		{
			SetScrollViewToCurrentSession();
		});
		Init(_GuildSuppressBanInfo);
	}

	public void Init(GuildSuppressBanInfo _GuildSuppressBanInfo)
	{
		GuildSuppressBanInfo = _GuildSuppressBanInfo;
		SetBanList();
		SetLabelText();
		ScrollView.ResetPosition();
	}

	public void SetBanList()
	{
		SeasonIndex = GuildSuppressBanInfo.SeasonIndex;
		CurrentSessionIndex = GuildSuppressBanInfo.SessionIndex;
		List<GuildSuppressSessionData> sessionList = GameRoot.Instance.GuildSuppressSessionDataContainer.GetSessionList(SeasonIndex);
		for (int i = 0; i < sessionList.Count; i++)
		{
			GuildSuppressBanComponent guildSuppressBanComponent = GuildSuppressBanComponent.PrefabAuto.Factory.Create(Grid.transform);
			guildSuppressBanComponent.Open(sessionList[i], GuildSuppressBanInfo.SessionIndex, i + 1);
			_banComponentList.Add(guildSuppressBanComponent);
		}
		Grid.Reposition();
	}

	public void SetLabelText()
	{
		if (Label_Title != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_BAN_LIST_TITLE", SeasonIndex);
			UIHelper.SetLabelText(Label_Title, stringDirect);
		}
		if (Label_Desc != null)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_BAN_LIST_DESC", SeasonIndex);
			UIHelper.SetLabelText(Label_Desc, stringDirect2);
		}
	}

	public void SetScrollViewToCurrentSession()
	{
		if (CurrentSessionIndex <= 0)
		{
			return;
		}
		GuildSuppressBanComponent guildSuppressBanComponent = null;
		foreach (GuildSuppressBanComponent banComponent in _banComponentList)
		{
			if (banComponent.GuildSuppressSessionData.SessionIndex == CurrentSessionIndex)
			{
				guildSuppressBanComponent = banComponent;
				break;
			}
		}
		if (Panel == null || guildSuppressBanComponent == null)
		{
			return;
		}
		Transform cachedTransform = Panel.cachedTransform;
		_ = Panel.worldCorners;
		Vector3 zero = Vector3.zero;
		if (guildSuppressBanComponent.index == 1)
		{
			return;
		}
		if (guildSuppressBanComponent.index == _banComponentList.Count)
		{
			float num = (float)_banComponentList.Count * Grid.cellHeight - Grid.cellHeight - Panel.height / 2f;
			zero.y = 0f - num;
		}
		else
		{
			for (int i = 0; i < guildSuppressBanComponent.index; i++)
			{
				zero -= new Vector3(0f, Grid.cellHeight, 0f);
			}
			float num2 = (float)_banComponentList.Count * Grid.cellHeight - Grid.cellHeight - Panel.height / 2f;
			if (Mathf.Abs(zero.y) > Mathf.Abs(num2))
			{
				zero.y = 0f - num2;
			}
		}
		if (!ScrollView.canMoveHorizontally)
		{
			zero.x = 0f;
		}
		if (!ScrollView.canMoveVertically)
		{
			zero.y = 0f;
		}
		zero.z = 0f;
		Vector3 relative = cachedTransform.localPosition - zero;
		ScrollView.MoveRelative(relative);
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}

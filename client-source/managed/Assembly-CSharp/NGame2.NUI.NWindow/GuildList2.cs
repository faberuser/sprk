using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildList2 : BaseWindow, GuildInfoComponent.Listener
{
	public interface Listener
	{
		void OnClickJoinGuild(GuildInfo info);

		void OnClickCancelRequest(GuildInfo info, Action onFail);

		void OnClickGetGuildList();

		void OnClickSearchGuild(string name);

		bool OnCloseRequest(GuildList2 closeThis);

		void ReOpen();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildList2";

	public UIToggle Toggle_List;

	public UIToggle Toggle_JoinRequest;

	public UIWidget Widget_Refresh;

	public UIWidget Widget_Request;

	public UIWidget Widget_Search;

	public UILabel Label_SearchGuildName;

	public UIButton Button_Search;

	public UIButton Button_Refresh;

	public UIScrollGrid GuildInfoGrid2;

	public UILabel Label_InputName;

	public UILabel Label_NoGuild;

	public UILabel Label_RequestCount;

	public UIInput Input_GuildName;

	public UILabel Label_NoGuildList;

	private Dictionary<long, GuildInfoComponent> _DictGuilds = new Dictionary<long, GuildInfoComponent>();

	private Dictionary<long, GuildInfoComponent> _DictJoinRequestedGuilds = new Dictionary<long, GuildInfoComponent>();

	private GuildInfoComponent _SearchedGuild;

	private Listener _listener;

	private float _SearchDelayTime;

	private bool _SearchDelay;

	private float _GetGuildListDelayTime;

	private bool _GetGuildListDelay;

	private bool _UseSearchResult;

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_SearchDelay)
		{
			_SearchDelayTime += Time.deltaTime;
			if (_SearchDelayTime > 2f)
			{
				UIHelper.EnableButton(Button_Search, enable: true);
				_SearchDelay = false;
			}
		}
		if (_GetGuildListDelay)
		{
			_GetGuildListDelayTime += Time.deltaTime;
			if (_GetGuildListDelayTime > 2f)
			{
				UIHelper.EnableButton(Button_Refresh, enable: true);
				_GetGuildListDelay = false;
			}
		}
	}

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		_UseSearchResult = false;
		Toggle_List.Set(state: true);
		Toggle_JoinRequest.Set(state: false);
		Input_GuildName.characterLimit = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNameLength");
		Reset();
		GuildInfoGrid2.Grid.sorting = UIGrid.Sorting.Custom;
		GuildInfoGrid2.Grid.onCustomSort = (Transform x, Transform y) => SortBase<GuildInfoComponent>(x, y, SortGuildInfo);
		SetupGuildList();
		UIHelper.EnableGameObject(Widget_Refresh, Toggle_List.value);
		UIHelper.EnableGameObject(Widget_Search, Toggle_List.value);
		UIHelper.EnableGameObject(Widget_Request, Toggle_JoinRequest.value);
	}

	public void SetGuildListSearch(bool bSearch)
	{
		_UseSearchResult = bSearch;
	}

	public void RefreshListOfCurTab()
	{
		if (Toggle_List.value)
		{
			RefreshGuildList();
		}
		if (Toggle_JoinRequest.value)
		{
			RefreshJoinRequestedGuildList();
		}
	}

	public void RefreshGuildList()
	{
		ResetGuildList();
		SetupGuildList();
	}

	public void RefreshJoinRequestedGuildList()
	{
		ResetJoinRequestedGuildList();
		SetupJoinRequestedGuildList();
	}

	public void SetSearchDelay()
	{
		_SearchDelay = true;
		_SearchDelayTime = 0f;
		UIHelper.EnableButton(Button_Search, enable: false);
	}

	public void SetGetGuildListDelay()
	{
		_GetGuildListDelay = true;
		_GetGuildListDelayTime = 0f;
		UIHelper.EnableButton(Button_Refresh, enable: false);
	}

	public void OnChangeToggle()
	{
		if (Toggle_List.value || Toggle_JoinRequest.value)
		{
			Reset();
			if (Toggle_List.value)
			{
				SetupGuildList();
			}
			if (Toggle_JoinRequest.value)
			{
				SetupJoinRequestedGuildList();
			}
			UIHelper.EnableGameObject(Widget_Refresh, Toggle_List.value);
			UIHelper.EnableGameObject(Widget_Search, Toggle_List.value);
			UIHelper.EnableGameObject(Widget_Request, Toggle_JoinRequest.value);
		}
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public override void Reset()
	{
		base.Reset();
		ResetJoinRequestedGuildList();
		ResetRequestedGuild();
		ResetSearchedGuild();
		UIHelper.SetLabelText(Label_SearchGuildName, string.Empty);
	}

	private void ResetRequestedGuild()
	{
		FactoryBase.ReleaseAndClearList(_DictGuilds);
	}

	private void ResetJoinRequestedGuildList()
	{
		FactoryBase.ReleaseAndClearList(_DictJoinRequestedGuilds);
	}

	private void ResetSearchedGuild()
	{
		if (_SearchedGuild != null)
		{
			FactoryBase.ReleaseComponent(_SearchedGuild);
			_SearchedGuild = null;
		}
	}

	private void ResetGuildList()
	{
		ResetSearchedGuild();
		ResetRequestedGuild();
	}

	public void DisplayGuildNotFound()
	{
		UIHelper.EnableGameObject(Label_InputName, enable: false);
		UIHelper.EnableGameObject(Label_NoGuild, enable: true);
		UIHelper.EnableGameObject(Label_NoGuildList, enable: false);
	}

	private void SetupRequestedGuild()
	{
		_UseSearchResult = false;
		if (GlobalAutoBehaviour<GuildManager>.instance.RequestedGuildManager == null)
		{
			return;
		}
		IEnumerator<GuildInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.RequestedGuildManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.Get(enumerator.Current.Id) == null && !_DictGuilds.ContainsKey(enumerator.Current.Id))
			{
				GuildInfoComponent value = CreateGuildInfo(enumerator.Current, GuildInfoComponent.EShowMode.Join);
				_DictGuilds.Add(enumerator.Current.Id, value);
			}
		}
		UIHelper.EnableGameObject(Label_InputName, enable: false);
		UIHelper.EnableGameObject(Label_NoGuild, enable: false);
		UIHelper.EnableGameObject(Label_NoGuildList, _DictGuilds.Count == 0);
		UIHelper.RepositionGrid(GuildInfoGrid2);
	}

	private void SetupSearchedGuild()
	{
		_UseSearchResult = true;
		if (_SearchDelay)
		{
			Button_Search.enabled = false;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo != null)
		{
			GuildInfoComponent searchedGuild = CreateGuildInfo(GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo, GuildInfoComponent.EShowMode.Join);
			_SearchedGuild = searchedGuild;
		}
		UIHelper.EnableGameObject(Label_InputName, GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo == null);
		UIHelper.EnableGameObject(Label_NoGuildList, enable: false);
		UIHelper.EnableGameObject(Label_NoGuild, enable: false);
		UIHelper.RepositionGrid(GuildInfoGrid2);
	}

	private void SetupGuildList()
	{
		if (_UseSearchResult)
		{
			SetupSearchedGuild();
		}
		else
		{
			SetupRequestedGuild();
		}
	}

	private void SetupJoinRequestedGuildList()
	{
		SetRequestCountLabel();
		IEnumerator<GuildInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!_DictJoinRequestedGuilds.ContainsKey(enumerator.Current.Id))
			{
				GuildInfoComponent value = CreateGuildInfo(enumerator.Current, GuildInfoComponent.EShowMode.CancelJoin);
				_DictJoinRequestedGuilds.Add(enumerator.Current.Id, value);
			}
		}
		UIHelper.RepositionGrid(GuildInfoGrid2);
	}

	private void SetRequestCountLabel()
	{
		int num = GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.Count();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxJoinGuildRequest");
		UIHelper.SetLabelText(Label_RequestCount, num + " / " + valueInt);
	}

	private GuildInfoComponent CreateGuildInfo(GuildInfo info, GuildInfoComponent.EShowMode ShowMode)
	{
		GuildInfoComponent guildInfoComponent = UIHelper.AddGridItem(GuildInfoGrid2, GuildInfoComponent.PrefabAuto);
		guildInfoComponent.Open(info, this, ShowMode);
		return guildInfoComponent;
	}

	public void GetGuildList()
	{
		if (_listener != null)
		{
			_listener.OnClickGetGuildList();
		}
	}

	public void SearchGuild()
	{
		if (_listener != null)
		{
			ResetGuildList();
			_listener.OnClickSearchGuild(Label_SearchGuildName.text);
		}
	}

	void GuildInfoComponent.Listener.OnClickJoinGuild(GuildInfo info)
	{
		if (_listener != null)
		{
			_listener.OnClickJoinGuild(info);
		}
	}

	void GuildInfoComponent.Listener.OnClickCancelRequest(GuildInfo info)
	{
		if (_listener == null)
		{
			return;
		}
		_listener.OnClickCancelRequest(info, delegate
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.Remove(info.Id);
			}
			RefreshJoinRequestedGuildList();
		});
	}

	private int SortGuildInfo(GuildInfoComponent component1, GuildInfoComponent component2)
	{
		GuildInfo guildInfo = component1.GuildInfo;
		GuildInfo guildInfo2 = component2.GuildInfo;
		if (guildInfo == null || guildInfo2 == null)
		{
			if (guildInfo != null)
			{
				return -1;
			}
			if (guildInfo2 != null)
			{
				return 1;
			}
			return 0;
		}
		return -guildInfo.CurMember.CompareTo(guildInfo2.CurMember);
	}

	private int SortBase<T>(Transform a, Transform b, Comparison<T> SortFunc)
	{
		T component = a.GetComponent<T>();
		T component2 = b.GetComponent<T>();
		if (component == null || component2 == null)
		{
			if (component != null)
			{
				return -1;
			}
			if (component2 != null)
			{
				return 1;
			}
			return 0;
		}
		return SortFunc(component, component2);
	}

	public void OnChangeGuildName()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNameLength");
		string text = Input_GuildName.value;
		if (text.Length > valueInt)
		{
			text = text.Substring(0, valueInt);
		}
		Input_GuildName.value = text;
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (_listener != null)
		{
			_listener.ReOpen();
		}
	}
}

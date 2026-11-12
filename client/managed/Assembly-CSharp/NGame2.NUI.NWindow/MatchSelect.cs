using System;
using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NOrdeal;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchSelect : BaseWindow
{
	public interface Listener
	{
		void OnEnterPvp(MatchArenaType matchArenaType);
	}

	[Serializable]
	public class ButtonGroup
	{
		public MatchArenaType ArenaType;

		public UIButton Button;

		public UIWidget WidgetLocked;

		public UIWidget WidgetLockedBg;

		public GameObject Enable;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchSelect";

	public UIScrollGrid ScrollView;

	public List<ButtonGroup> PvpButtonGroups;

	public List<Transform> Sprite_Slot;

	public UIGrid Grid;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		InitBtn();
		UIHelper.RepositionGrid(ScrollView);
	}

	public void InitBtn()
	{
		InitBanPickButton();
		InitArenaButtonStatus();
		InitOrdealArena();
		SprkArenaPractice.AddCard(this);
	}

	private void InitBanPickButton()
	{
		EnablePvpButtonButton(MatchArenaType.BanPick, enable: false);
		ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.BanPick);
		if (contentsStatus == ContentsStatus.Disable || contentsStatus == ContentsStatus.Hide)
		{
			return;
		}
		SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(MatchArena.MatchSeasonInfo seasonInfo)
		{
			if (seasonInfo != null)
			{
				EnablePvpButtonButton(MatchArenaType.BanPick, enable: true);
				if (SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex.HasValue)
				{
					int value = SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex.Value;
					ButtonGroup buttonGroup = PvpButtonGroups[value];
					if (buttonGroup != null && buttonGroup.Button != null)
					{
						OnClickPvpButton(buttonGroup.Button);
					}
					SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = null;
				}
			}
		});
	}

	private void InitArenaButtonStatus()
	{
		MatchArenaType arenaType = MatchArenaType.Normal;
		EnablePvpButtonButton(arenaType, enable: false);
		if (!SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Arena))
		{
			return;
		}
		SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.Normal, delegate(MatchArena.MatchSeasonInfo seasonInfo)
		{
			if (seasonInfo != null)
			{
				EnablePvpButtonButton(arenaType, enable: true);
			}
		});
	}

	private void InitOrdealArena()
	{
		EnablePvpButtonButton(MatchArenaType.Ordeal, enable: false);
		if (!OrdealHelper.CheckOpenCondition())
		{
			return;
		}
		SingletonAuto<MatchArena>.instance.RequestOrdealSeasonInfo(delegate(MatchArena.MatchSeasonInfo seasonInfo)
		{
			if (seasonInfo != null)
			{
				EnablePvpButtonButton(MatchArenaType.Ordeal, enable: true);
			}
		});
	}

	private ButtonGroup GetButtonGroup(MatchArenaType arenaType)
	{
		return PvpButtonGroups.Find((ButtonGroup x) => x.ArenaType == arenaType);
	}

	private void EnablePvpButtonButton(MatchArenaType arenaType, bool enable)
	{
		ButtonGroup buttonGroup = GetButtonGroup(arenaType);
		if (buttonGroup == null)
		{
			VespaDebug.LogError("Button group of type=" + arenaType.ToString() + " not exist!");
			return;
		}
		UIHelper.EnableGameObject(buttonGroup.Button, enable);
		UIHelper.EnableGameObject(buttonGroup.WidgetLocked, !enable);
		UIHelper.EnableGameObject(buttonGroup.Enable, enable);
		UIHelper.SetWidgetGrayScale(buttonGroup.WidgetLockedBg, !enable);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			Close();
		}
	}

	public void OnClickPvpButton(UIButton button)
	{
		if (_listener != null)
		{
			ButtonGroup buttonGroup = PvpButtonGroups.Find((ButtonGroup x) => x.Button == button);
			if (buttonGroup != null)
			{
				_listener.OnEnterPvp(buttonGroup.ArenaType);
			}
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void SetArena(int[] nslotList)
	{
		SprkArenaPractice.SetSlots(this, nslotList);
	}
}

using System;
using System.Collections;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NTask.NCustomYieldInstruction;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class NewRaid : BaseWindow
{
	public interface Listener
	{
		void OnRaidList(NewRaid caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/NewRaid";

	public const int RaidMonsterUid = 1;

	public const string Command_ShowNewRaidUi = "NewRaidCinema/ShowNewRaidUi";

	public Transform InfoRoot;

	public UILabel NameLabel;

	public UILabel NamePrefixLabel;

	public UILabel RemainTimeLabel;

	public UITexture CinemaTexture;

	public float CinemaStartDelay;

	public string CinemaNodeTreeName;

	public int CinemaTestHeroIndex;

	private Listener _listener;

	private NShared.RaidInfo _raidInfo;

	private RaidData _raidData;

	private RenderTargetCinema _cinema;

	private Holder _receiverHolder = new Holder();

	public NShared.RaidInfo RaidInfo => _raidInfo;

	public void Open(Listener listener, NShared.RaidInfo raidInfo, Action onComplete)
	{
		if (raidInfo == null)
		{
			VespaDebug.LogError("RaidData info is null");
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_listener = listener;
		_raidInfo = raidInfo;
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(_raidInfo.RaidIndex, _raidInfo.RaidLevel);
		if (_raidData == null)
		{
			VespaDebug.LogError("RaidData not found : index= " + _raidInfo.RaidIndex + " + level=" + _raidInfo.RaidLevel);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (CinemaTexture == null)
		{
			VespaDebug.LogError("CinemaTexture is null");
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		base.Open();
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		SetupRaidInfo();
		VespaCoroutine.Start(this, coOpen(onComplete));
	}

	public override void Reset()
	{
		base.Reset();
		VespaCoroutine.StopAll(this);
		_receiverHolder.Clear();
		if (CinemaTexture != null)
		{
			CinemaTexture.mainTexture = null;
		}
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
	}

	private IEnumerator coOpen(Action onComplete)
	{
		UIHelper.EnableGameObject(CinemaTexture, enable: false);
		UIHelper.EnableGameObject(InfoRoot, enable: false);
		if (CinemaStartDelay > 0f)
		{
			yield return new NVespa.NTask.NCustomYieldInstruction.WaitForSeconds(CinemaStartDelay);
		}
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		UIHelper.EnableGameObject(CinemaTexture, enable: true);
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(_raidData.ChapterIndex, _raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out var _, out var _, out var _) == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = _raidData.NodeTreePath;
		stock.NodeTreeName = _raidData.NodeTreeCode;
		_cinema = RenderTargetCinema.CreateInstance(stock);
		if (_cinema == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		_cinema.StartNodeTree(OnEndCinema);
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private void OnEndCinema()
	{
		ShowNewRaidUi();
	}

	private void ShowNewRaidUi()
	{
		UIHelper.EnableGameObject(InfoRoot, enable: true);
	}

	private void SetupRaidInfo()
	{
		CampaignWaveData bossWaveData;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(_raidData.ChapterIndex, _raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
		if (bossCreatureData != null)
		{
			GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, bossWaveData.BossVisualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var creatureNamePrefix);
			UIHelper.SetLabelText(NameLabel, creatureName);
			UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
		}
		else
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(_raidData.Name));
			UIHelper.SetLabelText(NamePrefixLabel, "");
		}
	}

	public void OnClickRaidList()
	{
		if (_listener != null)
		{
			_listener.OnRaidList(this);
		}
	}

	public void OnClickClose()
	{
		VespaCoroutine.Start(coClose(this));
	}

	public void OnClickCinema()
	{
		OnEndCinema();
	}

	public override void OnSystemBackButton()
	{
		if (UIHelper.IsEnabledGameObjet(InfoRoot))
		{
			VespaCoroutine.Start(coClose(this));
		}
	}

	private static IEnumerator coClose(NewRaid This)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			This.Close();
		}
		sceneChanger.Close();
	}

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && message.Command == "NewRaidCinema/ShowNewRaidUi")
		{
			ShowNewRaidUi();
		}
	}
}

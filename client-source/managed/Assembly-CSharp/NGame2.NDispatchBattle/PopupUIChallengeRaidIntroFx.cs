using System;
using System.Collections;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDispatchBattle;

public class PopupUIChallengeRaidIntroFx : BaseWindow
{
	public const string PREFAB_PATH = "Resources/Prefab2/Window/Raid/PopupUIChallengeRaidIntroFx";

	public UIWidget FxRoot;

	public UITexture CinemaTexture;

	public string NodeTreeCode = "";

	public string NodeTreePath = "";

	public bool IsForceInEditor;

	private Action _onClose;

	private bool _isBlocked = true;

	private RenderTargetCinema _cinema;

	private RaidData _raidData;

	private Coroutine _coCheckingTimerOver;

	public static void OpenGlobal(int challengeRaidIndex, Action onClose)
	{
		WindowPrefab<PopupUIChallengeRaidIntroFx> windowPrefab = new WindowPrefab<PopupUIChallengeRaidIntroFx>("Resources/Prefab2/Window/Raid/PopupUIChallengeRaidIntroFx", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null && !(windowPrefab.instance == null))
		{
			windowPrefab.instance.Open(challengeRaidIndex, onClose);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public static void CloseGlobal()
	{
		PopupUIDispatchBattleFx popupUIDispatchBattleFx = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<PopupUIDispatchBattleFx>();
		if (popupUIDispatchBattleFx != null)
		{
			popupUIDispatchBattleFx.Close();
		}
	}

	public override void OnClosing()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.OnClosing();
	}

	public override void Reset()
	{
		base.Reset();
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

	public void Open(int challengeRaidIndex, Action onClose)
	{
		base.Open();
		_onClose = onClose;
		_isBlocked = true;
		if (!UpdateData(challengeRaidIndex))
		{
			Close();
		}
	}

	public void OnClick()
	{
		if (!_isBlocked)
		{
			Close();
		}
	}

	private void OnEnable()
	{
		if (IsForceInEditor)
		{
			IsForceInEditor = false;
		}
		VespaCoroutine.Start(this, CoProcessingPlayFx());
		_coCheckingTimerOver = VespaCoroutine.Start(this, CoCheckingTimeOver());
	}

	private bool UpdateData(int challengeRaidIndex)
	{
		ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		if (data == null)
		{
			return false;
		}
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(data.RaidIndex, 1);
		if (_raidData == null)
		{
			return false;
		}
		if (!IsForceInEditor)
		{
			NodeTreePath = _raidData.NodeTreePath;
			NodeTreeCode = _raidData.NodeTreeCode;
		}
		return true;
	}

	private IEnumerator CoProcessingPlayFx()
	{
		yield return new WaitUntil(() => _raidData != null);
		VespaCoroutine.Stop(_coCheckingTimerOver);
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = NodeTreePath;
		stock.NodeTreeName = NodeTreeCode;
		_cinema = RenderTargetCinema.CreateInstance(stock);
		if (_cinema == null)
		{
			Close();
			yield break;
		}
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		if (!_cinema.StartNodeTree(OnEnd))
		{
			Close();
			yield break;
		}
		if (CinemaTexture != null && _cinema != null)
		{
			CinemaTexture.mainTexture = _cinema.RenderTarget;
			CinemaTexture.alpha = 1f;
		}
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
	}

	private void OnEnd()
	{
		_isBlocked = false;
		VespaCoroutine.Start(this, CoProcessingClosing());
	}

	private IEnumerator CoProcessingClosing()
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		_cinema.Close();
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Close();
	}

	private IEnumerator CoCheckingTimeOver()
	{
		yield return new WaitForSeconds(5f);
		Close();
	}
}

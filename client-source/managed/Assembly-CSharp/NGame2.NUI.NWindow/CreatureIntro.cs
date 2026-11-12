using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGame2.NTutorial;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow;

public class CreatureIntro : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/CreatureIntro";

	public static string KeyDefaultFirstView = "KeyCreatureIntro_";

	public static LocalAccountBool LocalValueFirstView;

	public UITexture CinemaTexture;

	private CreatureData _creatureData;

	private Action _completeAction;

	private Action _closeAction;

	private RenderTargetCinema _cinema;

	public override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_completeAction = null;
			_closeAction = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		VespaCoroutine.StopAll(this);
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

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (SingletonUpdater<TutorialManager>.instance.GetRunningState().AutoType == TutorialRunningState.TutorialAutoType.All)
		{
			OnSystemBackButton();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public override void OnSystemBackButton()
	{
		VespaCoroutine.Start(CoStartEndCinema());
	}

	public void Open(CreatureData creatureData, Action completeAction, Action closeAction)
	{
		if (IsOpen())
		{
			Close();
		}
		base.Open();
		if (CinemaTexture == null)
		{
			Vespa_System_Action_Extension_Method.fire(completeAction);
			return;
		}
		if (creatureData == null)
		{
			Vespa_System_Action_Extension_Method.fire(completeAction);
			return;
		}
		_creatureData = creatureData;
		_completeAction = completeAction;
		_closeAction = closeAction;
		VespaCoroutine.Start(this, CoStartCinema());
	}

	private IEnumerator CoStartCinema()
	{
		UIHelper.EnableGameObject(CinemaTexture, enable: false);
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		UIHelper.EnableGameObject(CinemaTexture, enable: true);
		_cinema = RenderTargetCinema.CreateInstance(CreateCinemaStock(_creatureData.CodeName));
		if (_cinema == null)
		{
			CompleteAction();
			yield break;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _creatureData.CodeName, 0);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.Normal;
		_cinema.SpawnCreature(VariableKeyword.Myself, _creatureData.Index, renderFacadeBuilder);
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		_cinema.StartNodeTree(EndCinema);
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		CompleteAction();
	}

	private void EndCinema()
	{
		if (!(_cinema == null) && !(_cinema.MainCamera == null) && !(_cinema.GETRENDERFACADE(_creatureData.Index) == null))
		{
			VespaCoroutine.Start(CoStartEndCinema());
		}
	}

	private IEnumerator CoStartEndCinema()
	{
		if (LocalValueFirstView != null)
		{
			LocalValueFirstView.Value = true;
			LocalValueFirstView = null;
		}
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (IsOpen())
		{
			_closeAction = null;
			Close();
		}
		sceneChanger.Close();
		CloseAction();
	}

	private void CompleteAction()
	{
		if (_completeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_completeAction);
		}
	}

	private void CloseAction()
	{
		if (_closeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_closeAction);
		}
	}

	private RenderTargetCinema.StockData CreateCinemaStock(string code)
	{
		RenderTargetCinema.StockData result = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out result.RenderTargetWidth, out result.RenderTargetHeight);
		result.NodeTreePackCode = PackHelper.GetPathNameFromCode(code);
		result.NodeTreeName = "BossIntro_Multi";
		return result;
	}

	public void OnClickSkip()
	{
		VespaCoroutine.Start(CoStartEndCinema());
	}
}

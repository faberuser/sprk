using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using NGame2.N3D;
using NGame2.NCutScene.NModifier;
using NGame2.NCutScene.NScript;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCutScene;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene;

public class CutSceneManager : SceneAutoBehaviour<CutSceneManager>, CutSceneWindow.Listener, IInspectorGuiDrawer, ITouchDisabler
{
	public SceneModifier BaseSceneMod = new SceneModifier();

	public List<ScriptGroup> GroupList = new List<ScriptGroup>();

	public IncreaseDic<int, ShakeMaker> CreatureShakeDic = new IncreaseDic<int, ShakeMaker>();

	public IncreaseDic<GameUIRoot.ELayerType, ShakeMaker> CameraShakeDic = new IncreaseDic<GameUIRoot.ELayerType, ShakeMaker>();

	public CreatureSlotManager CreatureSlotMgr = new CreatureSlotManager();

	public BackgroundController BackgroundControl = new BackgroundController();

	public PostProcessManager PostProcessManager = new PostProcessManager();

	private ILocalTimer _timer = new UnityLocalTimer();

	private bool _canSkip = true;

	private int _skipUnityUpdate;

	private bool _init;

	private bool _show;

	private GameAssetData _gameAsset = new GameAssetData();

	private string _bgmCode = string.Empty;

	private Action _onCutSceneEnd;

	private List<VespaCoroutine.RuntimeData> _coroutineList = new List<VespaCoroutine.RuntimeData>();

	private List<Tuple<int, int>> _groupSwapList = new List<Tuple<int, int>>();

	private List<Tuple<int, int>> _groupInsertList = new List<Tuple<int, int>>();

	private List<ISound> _soundList = new List<ISound>();

	public string ScriptPath { get; private set; }

	public bool IsLoading { get; private set; }

	public bool IsPlaying { get; private set; }

	public int CurrentScript { get; private set; }

	public ScriptGroup CurrentScriptGroup => Vespa_Generic_IList_Extension_Method.GetValid(GroupList, CurrentScript);

	public ScriptGroup.ESkipType CurrentScriptSkipType => CurrentScriptGroup?.SkipType ?? ScriptGroup.ESkipType.Default;

	public CutSceneMenu CutSceneMenu => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneMenu>();

	public CutSceneWindow CutSceneWindow => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneWindow>();

	public CutSceneForeground CutSceneForeground => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneForeground>();

	bool ITouchDisabler.IsValid => true;

	public void PlaySound(SoundStock soundStock)
	{
		ISound sound = SingletonAuto<SoundContainer>.instance.CreateSound(soundStock);
		if (sound != null)
		{
			_soundList.Add(sound);
		}
	}

	private void UpdateSound()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_soundList); i++)
		{
			ISound sound = _soundList[i];
			sound.Update();
			if (sound.IsEnd())
			{
				SoundContainer.ReleaseSound(sound);
				_soundList.RemoveAt(i);
				i--;
			}
		}
	}

	public void StopSound()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_soundList); i++)
		{
			SoundContainer.ReleaseSound(_soundList[i]);
		}
		_soundList.Clear();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			StopAllVespaCoroutine();
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
			{
				GroupList[CurrentScript].FinalizeGroup();
			}
			if (!String_Extension_Method.IsNullOrEmpty(_bgmCode))
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
				_bgmCode = string.Empty;
			}
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
			StopSound();
			_gameAsset.Close();
			BackgroundControl.ReleaseGameAsset();
			PostProcessManager.Release();
		}
	}

	private void StopAllVespaCoroutine()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_coroutineList); i++)
		{
			VespaCoroutine.RuntimeData runtimeData = _coroutineList[i];
			if (!runtimeData.IsEnd)
			{
				VespaCoroutine.Stop(runtimeData);
			}
		}
		_coroutineList.Clear();
	}

	public Coroutine StartVespaCoroutine(IEnumerator enumerator)
	{
		VespaCoroutine.RuntimeData runtimeData = VespaCoroutine.StartRuntime(enumerator);
		if (runtimeData == null)
		{
			return null;
		}
		_coroutineList.Add(runtimeData);
		return runtimeData.ExecuteCo;
	}

	private void Init()
	{
		if (!_init)
		{
			int layer = -1;
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.CutScene3D);
			if (layerInfo != null)
			{
				layer = layerInfo.Layer;
			}
			_init = true;
			CreatureSlotMgr.Open(layer);
			SceneAutoBehaviour<CommonManagement>.instance.AddInstance<CutSceneWindow>("Resources/Prefab2/Window/CutScene/CutSceneWindow", GameUIRoot.ELayerType.CutSceneUI);
			CutSceneWindow cutSceneWindow = SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneWindow>();
			if (cutSceneWindow != null)
			{
				cutSceneWindow.SetListener(this);
			}
			SceneAutoBehaviour<CommonManagement>.instance.AddInstance<CutSceneForeground>("Resources/Prefab2/Window/CutScene/CutSceneForeground", GameUIRoot.ELayerType.CutSceneUI);
			SceneAutoBehaviour<CommonManagement>.instance.AddInstance<CutSceneMenu>("Resources/Prefab2/Window/CutScene/CutSceneMenu", GameUIRoot.ELayerType.CutSceneUI);
			SceneAutoBehaviour<CommonManagement>.instance.AddInstance<CutSceneBackground>("Resources/Prefab2/Window/CutScene/CutSceneBackground", GameUIRoot.ELayerType.CutSceneBottomUI);
			Show(show: true);
		}
	}

	public void Open(string scriptPath, bool canSkip, Action onCutSceneEnd, Action onCutSceneLoad = null)
	{
		Init();
		ScriptPath = scriptPath;
		_canSkip = canSkip;
		_onCutSceneEnd = onCutSceneEnd;
		StopAllVespaCoroutine();
		StartVespaCoroutine(coOpen(skipLoadScript: false, onCutSceneLoad));
	}

	public void Close(bool stopImmediate, Action onCloseComplete = null)
	{
		if (!IsPlaying)
		{
			Vespa_System_Action_Extension_Method.fire(onCloseComplete);
			return;
		}
		StopAllVespaCoroutine();
		StartVespaCoroutine(coEndCutScene(stopImmediate, delegate
		{
			Vespa_System_Action_Extension_Method.fire(_onCutSceneEnd);
			_onCutSceneEnd = null;
			Vespa_System_Action_Extension_Method.fire(onCloseComplete);
			_gameAsset.Close();
		}));
	}

	private void RestartToCurrent()
	{
		int currentIndex = CurrentScript - 1;
		currentIndex = Mathf.Max(currentIndex, 0);
		StopAllVespaCoroutine();
		Skip();
		StartVespaCoroutine(coEndCutScene(stopImmediate: false, delegate
		{
			StartVespaCoroutine(coOpen(skipLoadScript: true, delegate
			{
				if (Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, currentIndex))
				{
					SkipTo(currentIndex);
				}
			}));
		}));
	}

	private void Restart()
	{
		StopAllVespaCoroutine();
		StartVespaCoroutine(coEndCutScene(stopImmediate: false, delegate
		{
			StartVespaCoroutine(coOpen(skipLoadScript: true, null));
		}));
	}

	private IEnumerator coOpen(bool skipLoadScript, Action onCutSceneLoad)
	{
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		yield return StartVespaCoroutine(coOpenInternal(skipLoadScript, onCutSceneLoad));
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
	}

	private IEnumerator coOpenInternal(bool skipLoadScript, Action onCutSceneLoad)
	{
		IsLoading = true;
		GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.CutScene);
		WaitingPopup.Open(this);
		if (!skipLoadScript)
		{
			_gameAsset.Close();
		}
		float breakTime = 30f;
		float whileTick;
		if (!skipLoadScript)
		{
			bool isComplete = false;
			JsonData json = null;
			_gameAsset.LoadScript(ScriptPath, delegate(JsonData iJson)
			{
				json = iJson;
				isComplete = true;
			});
			whileTick = 0f;
			while (!isComplete)
			{
				whileTick += Time.deltaTime;
				if (whileTick >= breakTime)
				{
					VespaDebug.LogError("LoadScript over time");
					isComplete = true;
					json = null;
					break;
				}
				yield return null;
			}
			if (!LoadFromJson(json))
			{
				LoadEmptyScript();
			}
			if (!String_Extension_Method.IsNullOrEmpty(BaseSceneMod.BackgroundStock.EntryMenuCode))
			{
				_gameAsset.LoadEntryMenuBackground(BaseSceneMod.BackgroundStock.EntryMenuCode);
			}
			for (int num = 0; num < GroupList.Count; num++)
			{
				GroupList[num].Visit(_gameAsset);
			}
			_gameAsset.BeginLoadCreature();
			whileTick = 0f;
			while (!_gameAsset.IsCompleteAsyncLoad())
			{
				whileTick += Time.deltaTime;
				if (whileTick >= breakTime)
				{
					VespaDebug.LogError("LoadCreature over time");
					break;
				}
				yield return null;
			}
		}
		WaitingPopup.Close(this);
		bool completeBackgroundOpen = false;
		BackgroundControl.Open(this, BaseSceneMod.BackgroundStock, skipAni: false, delegate
		{
			completeBackgroundOpen = true;
		});
		whileTick = 0f;
		while (!completeBackgroundOpen)
		{
			whileTick += Time.deltaTime;
			if (whileTick >= breakTime)
			{
				VespaDebug.LogError("BackgroundOpen over time");
				break;
			}
			yield return null;
		}
		if (!BaseSceneMod.PauseMainScene)
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		_show = true;
		IsLoading = false;
		CutSceneWindow.OpenWindow();
		CutSceneMenu.OpenWindow(_canSkip);
		CreatureSlotMgr.Show = true;
		CurrentScript = -1;
		NextScript();
		IsPlaying = true;
		Vespa_System_Action_Extension_Method.fire(onCutSceneLoad);
	}

	public void ChangeEntryMenuBackground(string entryMenuCode, bool clearCreature, bool closeForeground, bool clearText, bool isSkip, bool reverse, Action onComplete)
	{
		StartVespaCoroutine(coChangeEntryMenuBackground(entryMenuCode, clearCreature, closeForeground, clearText, isSkip, reverse, onComplete));
	}

	private IEnumerator coChangeEntryMenuBackground(string entryMenuCode, bool clearCreature, bool closeForeground, bool clearText, bool isSkip, bool reverse, Action onComplete)
	{
		SceneChanger sceneChanger = null;
		if (!isSkip)
		{
			sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}
		if (clearCreature)
		{
			for (int i = 0; i < 20; i++)
			{
				if (CreatureSlotMgr.Count == 0)
				{
					break;
				}
				LeaveCreature(CreatureSlotMgr.GetSlotIndexByIndex(0));
			}
		}
		if (closeForeground && CutSceneForeground.IsOpen())
		{
			CutSceneForeground.Close();
		}
		if (clearText && CutSceneWindow.IsOpen())
		{
			CutSceneWindow.SetText(string.Empty, string.Empty);
		}
		BackgroundControl.Close();
		bool completeBackgroundOpen = false;
		BackgroundControl.Open(this, entryMenuCode, skipAni: true, reverse, delegate
		{
			completeBackgroundOpen = true;
		});
		while (!completeBackgroundOpen)
		{
			yield return null;
		}
		if (!isSkip)
		{
			sceneChanger.Close();
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private bool LoadFromJson(JsonData json)
	{
		if (json == null || !json.IsObject())
		{
			return false;
		}
		try
		{
			if (!BaseSceneMod.LOADFROMJSON(json["BaseSceneMod"]))
			{
				return false;
			}
			if (!GroupList.LoadFromJson(json["GroupList"], () => new ScriptGroup(_timer)))
			{
				return false;
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private void LoadEmptyScript()
	{
		BaseSceneMod.ZoomScale = 500f;
		BaseSceneMod.Offset = new Vector3(0f, -1f, 0f);
		DisplayText displayText = new DisplayText();
		displayText.TitleLink.Key = "Empty";
		ScriptHolder scriptHolder = new ScriptHolder();
		scriptHolder.Tick = 0;
		scriptHolder.Value = displayText;
		ScriptGroup scriptGroup = new ScriptGroup(_timer);
		scriptGroup.ScriptList.Add(scriptHolder);
		scriptGroup.TouchType = ScriptGroup.ETouchType.Touch;
		GroupList.Clear();
		GroupList.Add(scriptGroup);
		RestoredPortal.TempleMissingScene(this);
	}

	public JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("BaseSceneMod", BaseSceneMod);
		jsonData.AddFieldList("GroupList", GroupList);
		return jsonData;
	}

	public ShakeMaker GetCreatureShake(int creatureSlotIndex)
	{
		ShakeMaker shakeMaker = CreatureShakeDic.GETVALUE(creatureSlotIndex, null);
		if (shakeMaker == null)
		{
			shakeMaker = new ShakeMaker();
			CreatureShakeDic.Add(creatureSlotIndex, shakeMaker);
		}
		return shakeMaker;
	}

	public ShakeMaker GetCameraShake(GameUIRoot.ELayerType layerType)
	{
		ShakeMaker shakeMaker = CameraShakeDic.GETVALUE(layerType, null);
		if (shakeMaker == null)
		{
			shakeMaker = new ShakeMaker();
			CameraShakeDic.Add(layerType, shakeMaker);
		}
		return shakeMaker;
	}

	public void EnterCreature(int slotIndex, RenderFacadeBuilder renderFacadeBuilder, Vector3 position, float scale, float angle, Vector3 rotation)
	{
		LeaveCreature(slotIndex);
		CreatureModifier creatureModifier = new CreatureModifier();
		if (creatureModifier.Open(renderFacadeBuilder, CreatureSlotMgr.CreatureRootTm, _timer))
		{
			creatureModifier.Position = position;
			creatureModifier.Scale = scale;
			creatureModifier.Angle = angle;
			creatureModifier.Rotation = rotation;
			CreatureSlotMgr.Add(slotIndex, creatureModifier);
		}
	}

	public void LeaveCreature(int slotIndex)
	{
		CreatureModifier creatureMod = CreatureSlotMgr.GetCreatureMod(slotIndex);
		if (creatureMod != null)
		{
			CreatureSlotMgr.Remove(slotIndex);
			creatureMod.Close();
		}
	}

	public void PlayEventBgm(string bgmCode, float volume)
	{
		if (String_Extension_Method.IsNullOrEmpty(bgmCode))
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
			_bgmCode = string.Empty;
		}
		else
		{
			_bgmCode = bgmCode;
			SingletonAuto<BgmManager2>.instance.Request(_bgmCode, isEvent: true, volume);
		}
	}

	public void Show(bool show)
	{
		_show = show;
		if (!show)
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		else if (BaseSceneMod.PauseMainScene)
		{
			GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		CreatureSlotMgr.Show = show;
		if (CutSceneMenu != null && CutSceneMenu.IsOpen())
		{
			CutSceneMenu.Show(show);
		}
		if (CutSceneWindow != null && CutSceneWindow.IsOpen())
		{
			CutSceneWindow.Show(show);
		}
		BackgroundControl.Show(show);
	}

	public bool IsShowing()
	{
		return _show;
	}

	public void ChainUpdate()
	{
		_skipUnityUpdate = 2;
		UpdateInternal();
	}

	private void Update()
	{
		if (_skipUnityUpdate > 0)
		{
			_skipUnityUpdate--;
		}
		else
		{
			UpdateInternal();
		}
	}

	private void UpdateInternal()
	{
		if (_show)
		{
			BackgroundControl.ChainUpdate();
			UpdateCutScene();
			if (SingletonUpdater<TutorialManager>.instance.GetRunningState().AutoType == TutorialRunningState.TutorialAutoType.All && !SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				OnClickWindow();
			}
		}
	}

	private void UpdateCutScene()
	{
		if (IsPlaying)
		{
			CreatureSlotMgr.SetScene(BaseSceneMod.ZoomScale, BaseSceneMod.Offset);
			UpdateScript();
			CreatureSlotMgr.ChainUpdate(_timer);
			UpdataCreatureShake();
			UpdateCameraShake();
			UpdateSound();
		}
	}

	private void UpdateScript()
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
		{
			return;
		}
		UpdateKeepScript();
		ScriptGroup scriptGroup = GroupList[CurrentScript];
		if (scriptGroup == null)
		{
			NextScript();
			return;
		}
		scriptGroup.Update();
		if (scriptGroup.IsEnd)
		{
			NextScript();
		}
	}

	private void UpdateKeepScript()
	{
		ScriptGroup scriptGroup = null;
		IScript script = null;
		for (int i = 0; i < CurrentScript; i++)
		{
			scriptGroup = GroupList[i];
			if (scriptGroup == null || scriptGroup.ScriptList == null)
			{
				continue;
			}
			for (int j = 0; j < scriptGroup.ScriptList.Count; j++)
			{
				script = scriptGroup.ScriptList[j].Value;
				if (script != null && !script.IsEnd)
				{
					script.Update();
				}
			}
		}
	}

	private void UpdataCreatureShake()
	{
		for (int i = 0; i < CreatureShakeDic.Count; i++)
		{
			int keyByIndex = CreatureShakeDic.GetKeyByIndex(i);
			CreatureModifier creatureMod = CreatureSlotMgr.GetCreatureMod(keyByIndex);
			if (creatureMod != null && !(creatureMod.ShakeTransform == null))
			{
				ShakeMaker valueByIndex = CreatureShakeDic.GetValueByIndex(i);
				if (valueByIndex != null)
				{
					Vector3 shakeTarget = Vector3.zero;
					valueByIndex.ApplyShake(ref shakeTarget, 1f);
					creatureMod.ShakeTransform.localPosition = shakeTarget;
				}
			}
		}
	}

	private void UpdateCameraShake()
	{
		for (int i = 0; i < CameraShakeDic.Count; i++)
		{
			KeyValuePair<GameUIRoot.ELayerType, ShakeMaker> keyValuePairByIndex = CameraShakeDic.GetKeyValuePairByIndex(i);
			GameUIRoot.ELayerType key = keyValuePairByIndex.Key;
			ShakeMaker value = keyValuePairByIndex.Value;
			Vector3 shakeTarget = Vector3.zero;
			value.ApplyShake(ref shakeTarget, 1f);
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(key);
			if (layerInfo != null)
			{
				Transform shakeTransform = layerInfo.GetShakeTransform();
				if (shakeTransform != null)
				{
					shakeTransform.localPosition = shakeTarget;
				}
			}
		}
	}

	public bool CanSkipToEnd()
	{
		if (!IsPlaying)
		{
			return true;
		}
		if (CurrentScriptGroup == null)
		{
			return true;
		}
		for (int i = CurrentScript; i < __Extension_Method_ICollection__.COUNT(GroupList); i++)
		{
			if (GroupList[i].SkipType != ScriptGroup.ESkipType.Default)
			{
				return false;
			}
		}
		return true;
	}

	public void Skip()
	{
		SkipTo(__Extension_Method_ICollection__.COUNT(GroupList));
	}

	public void SkipTo(int scriptIndex, FinalizeType finalizeType = FinalizeType.Normal)
	{
		ScriptGroup.ESkipType eSkipType = ((CurrentScriptGroup != null) ? CurrentScriptGroup.SkipType : ScriptGroup.ESkipType.Default);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GroupList); i++)
		{
			if (CurrentScript > scriptIndex)
			{
				break;
			}
			if (!IsPlaying)
			{
				break;
			}
			if (CurrentScriptGroup == null)
			{
				break;
			}
			if (CurrentScriptGroup.SkipType != ScriptGroup.ESkipType.Default)
			{
				if (eSkipType != ScriptGroup.ESkipType.StopThis || CurrentScriptGroup.TouchType != ScriptGroup.ETouchType.Touch)
				{
					break;
				}
				NextScript(finalizeType);
			}
			else
			{
				NextScript(finalizeType);
			}
		}
	}

	public void NextScript(FinalizeType finalizeType = FinalizeType.Normal)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
		{
			GroupList[CurrentScript].FinalizeGroup(finalizeType);
		}
		CurrentScript++;
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
		{
			Close(stopImmediate: false);
			return;
		}
		ScriptGroup scriptGroup = GroupList[CurrentScript];
		scriptGroup.Begin();
		CutSceneMenu.ShowSkipButton(_canSkip && scriptGroup.SkipType != ScriptGroup.ESkipType.StopThisAndHide);
	}

	private void ReleaseKeepScript()
	{
		ScriptGroup scriptGroup = null;
		IScript script = null;
		for (int i = 0; i < CurrentScript; i++)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, i))
			{
				continue;
			}
			scriptGroup = GroupList[i];
			for (int j = 0; j < scriptGroup.ScriptList.Count; j++)
			{
				script = scriptGroup.ScriptList[j].Value;
				if (script != null && !script.IsEnd)
				{
					script.FinalizeScript(FinalizeType.ForceStop);
				}
			}
		}
	}

	private IEnumerator coEndCutScene(bool stopImmediate, Action onComplete)
	{
		ReleaseKeepScript();
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
		{
			GroupList[CurrentScript].FinalizeGroup();
			CurrentScript = -1;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CreatureShakeDic); i++)
		{
			int keyByIndex = CreatureShakeDic.GetKeyByIndex(i);
			CreatureModifier creatureMod = CreatureSlotMgr.GetCreatureMod(keyByIndex);
			if (creatureMod != null && !(creatureMod.ShakeTransform == null))
			{
				creatureMod.ShakeTransform.localPosition = Vector3.zero;
				CreatureShakeDic.GetValueByIndex(i).Clear();
			}
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(CreatureShakeDic);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(CameraShakeDic); j++)
		{
			GameUIRoot.ELayerType keyByIndex2 = CameraShakeDic.GetKeyByIndex(j);
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(keyByIndex2);
			if (layerInfo != null && layerInfo.Camera != null && layerInfo.Camera.transform != null)
			{
				layerInfo.Camera.transform.localPosition = Vector3.zero;
			}
			CameraShakeDic.GetValueByIndex(j)?.Clear();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(CameraShakeDic);
		int count = CreatureSlotMgr.Count;
		for (int k = 0; k < count; k++)
		{
			if (CreatureSlotMgr.Count == 0)
			{
				break;
			}
			LeaveCreature(CreatureSlotMgr.GetSlotIndexByIndex(0));
		}
		PostProcessManager.OnEndCutScene();
		if (CreatureSlotMgr != null)
		{
			CreatureSlotMgr.Show = false;
		}
		if (!String_Extension_Method.IsNullOrEmpty(_bgmCode))
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
			_bgmCode = string.Empty;
		}
		StopSound();
		if (CutSceneMenu.IsOpen())
		{
			CutSceneMenu.Close();
		}
		if (CutSceneWindow.IsOpen())
		{
			CutSceneWindow.Close();
		}
		if (CutSceneForeground.IsOpen())
		{
			CutSceneForeground.Close();
		}
		if (stopImmediate)
		{
			BackgroundControl.Close();
		}
		else
		{
			bool completeBackgroundClose = false;
			BackgroundControl.CloseAsync(delegate
			{
				completeBackgroundClose = true;
			});
			while (!completeBackgroundClose)
			{
				yield return null;
			}
		}
		IsPlaying = false;
		GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void OnClickWindow()
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(GroupList, CurrentScript))
		{
			GroupList[CurrentScript].OnTouch();
		}
	}
}

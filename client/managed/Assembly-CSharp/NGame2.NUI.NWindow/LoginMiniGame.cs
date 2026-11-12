using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NLogin.NMiniGame;
using NGame2.NUI.NManager;
using NGameAsset;
using NNguiExtension;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginMiniGame : BaseWindow, IGameGlobal
{
	private enum TextState
	{
		None,
		ReadyToPlay,
		StartPlay,
		GameOver
	}

	public enum ScrollWidgetType
	{
		Ground,
		Environment
	}

	[Serializable]
	public class ScrollWidget
	{
		public UIWidget Sprite;

		public float VelocityX;

		public ScrollWidgetType ScrollWidgetType;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginMiniGame";

	public const float REQUIRE_DOWNLOAD_EXPECT_TIME = 15f;

	public static bool MiniGameOpend;

	public bool OpenOnStart;

	[Range(0.1f, 3f)]
	public float MIN_TIME_SCALE = 1.5f;

	[Range(0.1f, 6f)]
	public float MAX_TIME_SCALE = 3f;

	[Range(1f, 30f)]
	public float SCALE_UP_TIME_SEC = 25f;

	[Range(1f, 2f)]
	public float OBSTACLE_CREATE_TIME_MIN_SEC = 1.3f;

	[Range(1f, 4f)]
	public float OBSTACLE_CREATE_TIME_MAX_SEC = 3f;

	private const string OBSTACLE_LONG_NAME = "minigame_art_object_01";

	private const string OBSTACLE_SHORT_NAME = "minigame_art_object_02";

	private const string OBSTACLE_ORC_NAME = "object_orc";

	private KeyCode _keyCode;

	private float _deltaTime;

	private float _elapsedTime;

	private const float _deltaTimeLimit = 0.05f;

	public UIWidget TextRoot;

	public UIWidget MenuWindow;

	public UIWidget QuitButton;

	public Transform ObstacleLayer;

	public Transform ObstacleTemplete;

	public UISpriteNumber RunDistance;

	public UISpriteNumber RunDistanceHighRecord;

	public Transform SoniaStartPosition;

	public Transform SoniaTransform;

	public Transform BackSpriteTransform;

	public UISprite TabToPlaySprite;

	public UISprite GameStartSprite;

	public UISprite GameOverSprite;

	private static LocalStorageInt HighScore = new LocalStorageInt(0, typeof(LoginMiniGame).FullName + "RunHighScore");

	public ScrollWidget[] ScrollSpriteList;

	private Queue<UISprite> _obstacleSpriteQ = new Queue<UISprite>();

	private Dictionary<int, UISprite> _obstacleContainer = new Dictionary<int, UISprite>();

	private Dictionary<string, Action<IGameEvent>> _eventHandler = new Dictionary<string, Action<IGameEvent>>();

	private Dictionary<State, StateBase> _stateContainer = new Dictionary<State, StateBase>();

	private StateBase _nextState;

	private StateBase _currentState = new StateBase();

	private IGameEventPool _gameEventPool = new GameEventPool();

	public NNguiExtension.UISpriteAnimation SoniaSpriteAnimation;

	public NNguiExtension.UISpriteAnimation DustSpriteAnimation;

	public NNguiExtension.UISpriteAnimation WeaponSpriteAnimation;

	public UITextureUVAnimation GroundAnimation;

	private bool _pause;

	private Coroutine _tabToPlayCoroutine;

	protected bool Pause
	{
		get
		{
			return _pause;
		}
		set
		{
			_pause = value;
			OnPauseChange();
		}
	}

	private void OnPauseChange()
	{
		UIHelper.EnableGameObject(TextRoot, !Pause);
		UIHelper.EnableGameObject(MenuWindow, Pause);
	}

	private void Start()
	{
	}

	public static void OpenPopup(bool openAtPatchDownload = true)
	{
		MiniGameOpend = true;
		WindowPrefab<LoginMiniGame> windowPrefab = new WindowPrefab<LoginMiniGame>("Resources/Prefab2/Window/Login/LoginMiniGame", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenWindow(openAtPatchDownload);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenWindow(bool openAtPatchDownload)
	{
		base.Open(delegate
		{
			Init(openAtPatchDownload);
		});
		SetUI(openAtPatchDownload);
	}

	public override void Reset()
	{
		base.Reset();
		ResetState();
		ResetTransform();
		ResetObstacles();
		ResetEventPool();
	}

	private void ResetState()
	{
		_nextState = null;
		_currentState = new StateBase();
	}

	private void ResetTransform()
	{
		SoniaTransform.localPosition = SoniaStartPosition.localPosition;
	}

	public override void OnSystemBackButton()
	{
		if (QuitButton.isActiveAndEnabled)
		{
			Close();
		}
	}

	private void Init(bool openAtPatchDownload)
	{
		PrintVariables();
		SetSpriteAnimation();
		RegistHandle();
		RegistState();
		SetNextState(State.ReadyState);
		StopAllAnimation();
	}

	private void PrintVariables()
	{
		if (!Define.IsLiveBuild())
		{
			VespaDebug.LogError("REQUIRE_DOWNLOAD_EXPECT_TIME : " + 15f);
			VespaDebug.LogError("MIN_TIME_SCALE : " + MIN_TIME_SCALE);
			VespaDebug.LogError("MAX_TIME_SCALE : " + MAX_TIME_SCALE);
			VespaDebug.LogError("SCALE_UP_TIME_SEC : " + SCALE_UP_TIME_SEC);
			VespaDebug.LogError("OBSTACLE_CREATE_TIME_MIN_SEC : " + OBSTACLE_CREATE_TIME_MIN_SEC);
			VespaDebug.LogError("OBSTACLE_CREATE_TIME_MAX_SEC : " + OBSTACLE_CREATE_TIME_MAX_SEC);
		}
	}

	private void SetUI(bool openAtPatchDownload)
	{
		Pause = false;
		SetCloseButton(show: true);
		bool backSprite = !openAtPatchDownload;
		SetBackSprite(backSprite);
	}

	private void SetSpriteAnimation()
	{
		SoniaSpriteAnimation.SetTimer(TimeDelta);
		DustSpriteAnimation.SetTimer(TimeDelta);
		WeaponSpriteAnimation.SetTimer(TimeDelta);
		GroundAnimation.SetTimer(TimeDelta);
	}

	private void RegistHandle()
	{
		RegistHandler<NGame2.NLogin.NMiniGame.ReadyToPlay>(ReadyToPlay);
		RegistHandler<NGame2.NLogin.NMiniGame.StartPlay>(StartPlay);
		RegistHandler<NGame2.NLogin.NMiniGame.GameOver>(GameOver);
		RegistHandler<NGame2.NLogin.NMiniGame.OnCollision>(OnCollision);
		RegistHandler<NGame2.NLogin.NMiniGame.EntityMove>(EntityMove);
		RegistHandler<NGame2.NLogin.NMiniGame.CreateObstacle>(CreateObstacle);
		RegistHandler<NGame2.NLogin.NMiniGame.MoveObstacle>(MoveObstacle);
		RegistHandler<NGame2.NLogin.NMiniGame.RemoveObstacle>(RemoveObstacle);
		RegistHandler<NGame2.NLogin.NMiniGame.MoveTerrain>(MoveTerrain);
		RegistHandler<NGame2.NLogin.NMiniGame.UpdateUI>(UpdateUI);
	}

	private void RegistHandler<T>(Action<IGameEvent> eventHandle) where T : IGameEvent
	{
		_eventHandler[typeof(T).Name] = eventHandle;
	}

	private void RegistState()
	{
		_stateContainer[State.ReadyState] = new NGame2.NLogin.NMiniGame.ReadyState
		{
			GameGlobal = this
		};
		_stateContainer[State.PlayState] = new NGame2.NLogin.NMiniGame.PlayState(CreatePlayDataProvider())
		{
			GameGlobal = this
		};
		_stateContainer[State.GameOverState] = new NGame2.NLogin.NMiniGame.GameOverState
		{
			GameGlobal = this
		};
	}

	private PlayDataProvier CreatePlayDataProvider()
	{
		PlayDataProvier playDataProvier = new PlayDataProvier();
		playDataProvier.obstacleCreateTimeMin = OBSTACLE_CREATE_TIME_MIN_SEC;
		playDataProvier.obstacleCreateTimeMax = OBSTACLE_CREATE_TIME_MAX_SEC;
		playDataProvier.Sonia = new MoveEntity(this, 1, applyGravity: true)
		{
			Rect = new Rect(SoniaStartPosition.localPosition, World.SONIA_WIDTH_HEIGHT)
		};
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ScrollSpriteList); i++)
		{
			if (ScrollSpriteList[i].ScrollWidgetType != ScrollWidgetType.Ground)
			{
				int width = ScrollSpriteList[i].Sprite.width;
				num = System.Math.Max(num, width);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(ScrollSpriteList); j++)
		{
			int num2 = ((num != 0) ? num : ScrollSpriteList[j].Sprite.width);
			int height = ScrollSpriteList[j].Sprite.height;
			if (ScrollSpriteList[j].ScrollWidgetType == ScrollWidgetType.Ground)
			{
				num2 = ScrollSpriteList[j].Sprite.width;
			}
			TerrainEntity item = new TerrainEntity(this, 100 + j)
			{
				Type = ((ScrollSpriteList[j].ScrollWidgetType != ScrollWidgetType.Ground) ? TerrainEntity.TerrainType.Environment : TerrainEntity.TerrainType.Ground),
				Rect = new Rect(ScrollSpriteList[j].Sprite.transform.localPosition, new Vector2(num2, height)),
				VelX = ScrollSpriteList[j].VelocityX
			};
			playDataProvier.TerrainList.Add(item);
		}
		return playDataProvier;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		SetTimeValues();
		Input();
		UpdateState();
		ReleaseInput();
	}

	private void DebugRectDraw()
	{
		DrawRect(SoniaTransform, World.SONIA_WIDTH_HEIGHT.x, World.SONIA_WIDTH_HEIGHT.y, Color.red);
		foreach (KeyValuePair<int, UISprite> item in _obstacleContainer)
		{
			float w = 0f;
			float y = 0f;
			if (item.Value.spriteName == "minigame_art_object_01")
			{
				w = World.OBSTACLE_WIDTH_HEIGHT_LONG.x;
				y = World.OBSTACLE_WIDTH_HEIGHT_LONG.y;
			}
			else if (item.Value.spriteName == "minigame_art_object_02")
			{
				w = World.OBSTACLE_WIDTH_HEIGHT_SHORT.x;
				y = World.OBSTACLE_WIDTH_HEIGHT_SHORT.y;
			}
			else if (item.Value.spriteName == "object_orc")
			{
				w = World.OBSTACLE_WIDTH_HEIGHT_ORC.x;
				y = World.OBSTACLE_WIDTH_HEIGHT_ORC.y;
			}
			DrawRect(item.Value.transform, w, y, Color.red);
		}
	}

	private void DrawRect(Transform trsBottomLeftAnchored, float w, float y, Color color)
	{
		Vector3 localPosition = trsBottomLeftAnchored.localPosition;
		Vector3 vector = localPosition + new Vector3(0f, y, 0f);
		Vector3 vector2 = vector + new Vector3(w, 0f, 0f);
		Vector3 vector3 = localPosition + new Vector3(w, 0f, 0f);
		localPosition = trsBottomLeftAnchored.TransformVector(localPosition);
		vector = trsBottomLeftAnchored.TransformVector(vector);
		vector2 = trsBottomLeftAnchored.TransformVector(vector2);
		vector3 = trsBottomLeftAnchored.TransformVector(vector3);
		Debug.DrawLine(localPosition, vector, color);
		Debug.DrawLine(vector, vector2, color);
		Debug.DrawLine(localPosition, vector3, color);
		Debug.DrawLine(vector3, vector2, color);
	}

	private void ReleaseInput()
	{
		_keyCode = KeyCode.None;
	}

	private void Input()
	{
		_keyCode = (UnityEngine.Input.GetKeyDown(KeyCode.Space) ? KeyCode.Space : _keyCode);
		_keyCode = (UnityEngine.Input.GetKeyDown(KeyCode.F1) ? KeyCode.F1 : _keyCode);
		_keyCode = (UnityEngine.Input.GetKeyDown(KeyCode.F2) ? KeyCode.F2 : _keyCode);
		_keyCode = (UnityEngine.Input.GetKeyDown(KeyCode.F3) ? KeyCode.F3 : _keyCode);
	}

	private float GetTimeScale()
	{
		if (Pause)
		{
			return 0f;
		}
		return Mathf.Clamp(MIN_TIME_SCALE + _elapsedTime / SCALE_UP_TIME_SEC, MIN_TIME_SCALE, MAX_TIME_SCALE);
	}

	StateBase IGameGlobal.GetCurrentState()
	{
		return _currentState;
	}

	public float TimeDelta()
	{
		return GetTimeScale() * _deltaTime;
	}

	public void SetNextState(State state)
	{
		StateBase value = null;
		if (!_stateContainer.TryGetValue(state, out value))
		{
			Debug.LogError("can't find state : " + state);
		}
		_nextState = value;
	}

	public bool OnKeyDown()
	{
		return _keyCode == KeyCode.Space;
	}

	public KeyCode GeyKeyCoden()
	{
		return _keyCode;
	}

	public T CreateEventInstance<T>() where T : IGameEvent, new()
	{
		string typeName = typeof(T).Name;
		T val = (T)_gameEventPool.Pop(typeName);
		if (val != null)
		{
			return val;
		}
		return new T();
	}

	public void OnEvent(IGameEvent gameEvent)
	{
		Action<IGameEvent> value = null;
		_ = gameEvent.GetType().Name;
		if (_eventHandler.TryGetValue(gameEvent.GetType().Name, out value))
		{
			Vespa_System_Action_Extension_Method.fire(value, gameEvent);
			_gameEventPool.Push(gameEvent);
		}
	}

	private void SetTimeValues()
	{
		_deltaTime = Time.deltaTime;
		_deltaTime = Mathf.Min(_deltaTime, 0.05f);
		if (!Pause)
		{
			_elapsedTime += Time.deltaTime;
		}
	}

	private void UpdateState()
	{
		if (_nextState != null)
		{
			_currentState.OnEnd();
			_nextState.OnBegin();
			_currentState = _nextState;
			_nextState = null;
		}
		if (!Define.IsLiveBuild() && UnityEngine.Input.GetKeyDown(KeyCode.F1))
		{
			Pause = !Pause;
		}
		_currentState.OnUpdate();
	}

	private void SetTextState(TextState state)
	{
		UIHelper.EnableGameObject(TabToPlaySprite, state == TextState.ReadyToPlay);
		UIHelper.EnableGameObject(GameStartSprite, state == TextState.StartPlay);
		UIHelper.EnableGameObject(GameOverSprite, state == TextState.GameOver);
		if (_tabToPlayCoroutine != null)
		{
			VespaCoroutine.Stop(_tabToPlayCoroutine);
		}
		switch (state)
		{
		case TextState.ReadyToPlay:
		{
			Action action = delegate
			{
				UIHelper.EnableGameObject(TabToPlaySprite, enable: true);
			};
			Action action2 = delegate
			{
				UIHelper.EnableGameObject(TabToPlaySprite, enable: false);
			};
			_tabToPlayCoroutine = ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, IntervalActionLoop(new Action[2] { action, action2 }, 0.5f));
			break;
		}
		case TextState.StartPlay:
			SetTimer(delegate
			{
				UIHelper.EnableGameObject(GameStartSprite, enable: false);
			}, 1f);
			break;
		}
	}

	private IEnumerator IntervalActionLoop(Action[] actions, float intervalTimeSec)
	{
		if (__Extension_Method_ICollection__.COUNT(actions) == 0)
		{
			yield break;
		}
		int index = 0;
		while (true)
		{
			Vespa_System_Action_Extension_Method.fire(actions[index++]);
			index %= __Extension_Method_ICollection__.COUNT(actions);
			yield return new WaitForSeconds(intervalTimeSec);
		}
	}

	private void SetTimer(Action func, float secs)
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, Timer(func, secs));
	}

	private IEnumerator Timer(Action func, float secs)
	{
		yield return new WaitForSeconds(secs);
		Vespa_System_Action_Extension_Method.fire(func);
	}

	private void ReadyToPlay(IGameEvent eventData)
	{
		SetTextState(TextState.ReadyToPlay);
		SetHighRecordDistance(GetHighScore());
		SetRunDistance(0);
	}

	private int GetHighScore()
	{
		return HighScore.Value;
	}

	private void StartPlay(IGameEvent eventData)
	{
		_elapsedTime = 0f;
		SetTextState(TextState.StartPlay);
		SetHighRecordDistance(GetHighScore());
		SetRunDistance(0);
		ResetObstacles();
		GroundAnimation.StartAnimation();
	}

	private void ResetObstacles()
	{
		Dictionary<int, UISprite>.Enumerator enumerator = _obstacleContainer.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_obstacleSpriteQ.Enqueue(enumerator.Current.Value);
			enumerator.Current.Value.gameObject.SetActive(value: false);
		}
		_obstacleContainer.Clear();
	}

	private void ResetEventPool()
	{
		_gameEventPool.Clear();
	}

	private void GameOver(IGameEvent eventData)
	{
		SetTextState(TextState.GameOver);
	}

	private void OnCollision(IGameEvent eventData)
	{
		StopAllAnimation();
	}

	private void StopAllAnimation()
	{
		GroundAnimation.StopAnimation();
		SoniaSpriteAnimation.StopAnimation();
		DustSpriteAnimation.StopAnimation();
		WeaponSpriteAnimation.StopAnimation();
	}

	private void EntityMove(IGameEvent eventData)
	{
		NGame2.NLogin.NMiniGame.EntityMove entityMove = (NGame2.NLogin.NMiniGame.EntityMove)eventData;
		if (entityMove.ID == 1)
		{
			SoniaMove(entityMove);
		}
	}

	private void SoniaMove(NGame2.NLogin.NMiniGame.EntityMove entityMove)
	{
		int state = (int)entityMove.State;
		if (SoniaSpriteAnimation.CurrentIndex != state)
		{
			SoniaSpriteAnimation.SetState(state);
			bool flag = state == 1;
			DustSpriteAnimation.SetActive(!flag);
			if (!flag)
			{
				DustSpriteAnimation.SetState(0);
			}
		}
		if (WeaponSpriteAnimation.CurrentIndex != 0)
		{
			WeaponSpriteAnimation.SetState(0);
		}
		SoniaTransform.transform.localPosition = entityMove.MovePos;
	}

	private void CreateObstacle(IGameEvent eventData)
	{
		NGame2.NLogin.NMiniGame.CreateObstacle createObstacle = eventData as NGame2.NLogin.NMiniGame.CreateObstacle;
		UISprite uISprite = CreateObstacleSprite();
		if (!(uISprite == null))
		{
			if (createObstacle.Type == NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Long)
			{
				uISprite.spriteName = "minigame_art_object_01";
			}
			else if (createObstacle.Type == NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Orc)
			{
				uISprite.spriteName = "object_orc";
			}
			else
			{
				uISprite.spriteName = "minigame_art_object_02";
			}
			uISprite.gameObject.SetActive(value: true);
			uISprite.MakePixelPerfect();
			uISprite.transform.parent = ObstacleLayer;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(uISprite.transform);
			uISprite.transform.localPosition = createObstacle.Pos;
			int obstacleID = createObstacle.ObstacleID;
			_obstacleContainer[obstacleID] = uISprite;
		}
	}

	private UISprite CreateObstacleSprite()
	{
		UISprite obstacleSpriteInQueue = GetObstacleSpriteInQueue();
		if (obstacleSpriteInQueue != null)
		{
			return obstacleSpriteInQueue;
		}
		return UnityEngine.Object.Instantiate(ObstacleTemplete).GetComponent<UISprite>();
	}

	private UISprite GetObstacleSpriteInQueue()
	{
		if (_obstacleSpriteQ.Count == 0)
		{
			return null;
		}
		UISprite uISprite = _obstacleSpriteQ.Dequeue();
		uISprite.gameObject.SetActive(value: true);
		return uISprite;
	}

	private void MoveObstacle(IGameEvent eventData)
	{
		NGame2.NLogin.NMiniGame.MoveObstacle moveObstacle = eventData as NGame2.NLogin.NMiniGame.MoveObstacle;
		if (!_obstacleContainer.ContainsKey(moveObstacle.ObstacleID))
		{
			Debug.LogError("not contain Key " + moveObstacle.ObstacleID);
		}
		else
		{
			_obstacleContainer[moveObstacle.ObstacleID].transform.localPosition = moveObstacle.Pos;
		}
	}

	private void RemoveObstacle(IGameEvent eventData)
	{
		NGame2.NLogin.NMiniGame.RemoveObstacle removeObstacle = eventData as NGame2.NLogin.NMiniGame.RemoveObstacle;
		if (!_obstacleContainer.ContainsKey(removeObstacle.ObstacleID))
		{
			Debug.LogError("not contain Key " + removeObstacle.ObstacleID);
			return;
		}
		_obstacleSpriteQ.Enqueue(_obstacleContainer[removeObstacle.ObstacleID]);
		_obstacleContainer[removeObstacle.ObstacleID].gameObject.SetActive(value: false);
		_obstacleContainer.Remove(removeObstacle.ObstacleID);
	}

	private void MoveTerrain(IGameEvent eventData)
	{
		NGame2.NLogin.NMiniGame.MoveTerrain moveTerrain = eventData as NGame2.NLogin.NMiniGame.MoveTerrain;
		int num = moveTerrain.TerrainID - 100;
		if (num >= 0 && num < __Extension_Method_ICollection__.COUNT(ScrollSpriteList))
		{
			Vector2 vector = new Vector2((int)moveTerrain.Pos.x, moveTerrain.Pos.y);
			ScrollSpriteList[num].Sprite.transform.localPosition = vector;
		}
	}

	private void UpdateUI(IGameEvent eventData)
	{
		int runDistance = (eventData as NGame2.NLogin.NMiniGame.UpdateUI).RunDistance;
		if (HighScore.Value < runDistance)
		{
			HighScore.Value = runDistance;
			SetHighRecordDistance(GetHighScore());
		}
		SetRunDistance(runDistance);
	}

	private void SetRunDistance(int value)
	{
		if (!(RunDistance == null))
		{
			RunDistance.Value = value;
		}
	}

	private void SetHighRecordDistance(int value)
	{
		if (!(RunDistanceHighRecord == null))
		{
			RunDistanceHighRecord.Value = value;
		}
	}

	public void OnDownloadComplete()
	{
		OnPause();
	}

	public void SetCloseButton(bool show)
	{
		UIHelper.EnableGameObject(QuitButton, show);
	}

	public void SetBackSprite(bool show)
	{
		UIHelper.EnableGameObject(BackSpriteTransform, show);
	}

	public void OnPause()
	{
		Pause = true;
	}

	public void OnResume()
	{
		Pause = false;
		SetCloseButton(show: true);
	}

	public void OnQuit()
	{
		Close();
	}

	public override void Close()
	{
		base.Close();
		MiniGameOpend = false;
	}

	public void OnPress()
	{
		_keyCode = KeyCode.Space;
	}

	public void OnRelease()
	{
	}

	public void OnDragStart()
	{
		_ = UICamera.currentTouch.pos;
	}

	public void OnDrag()
	{
		_ = UICamera.currentTouch.pos;
	}

	public void OnDragEnd()
	{
		_ = UICamera.currentTouch.pos;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NShared.GiveRewardHeroInnRoulette;
using NShared.RequestHeroInnRoulette;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class Roulette : BaseWindow
{
	[Serializable]
	public class ModelInfo
	{
		public Transform RenderPivot;

		[HideInInspector]
		public RenderFacade renderFacade;

		public UILabel LabelDesc;
	}

	private enum RequestComand
	{
		Info,
		Play
	}

	public static bool LOCAL_TEST;

	public const string PrefabPath = "Resources/Prefab2/Window/MiniGame/Roulette";

	public ModelInfo Model;

	public UIWidget UI_Roulette;

	public UILabel Label_RewardIndex;

	public UILabel Label_RouletteCount;

	public UILabel Label_Guide;

	public UISprite Spr_Arrow;

	public UIButton Button_Play;

	public TweenRotation[] Tween_Deco;

	public UIPlayAssetSound ArrowHitSound;

	public ParticleSystem ArrowEffect1;

	public ParticleSystem ArrowEffect2;

	public int Power = 3;

	public float RewardPopupDelaySec = 1f;

	private const string MODEL_CODE = "NPC.Inn";

	private const string MODEL_MOTION_NAME = "Touch";

	private const int ROULETTE_INDEX = 1;

	private const int TOUCH_ROTATION_MULTYPLY = 1;

	private const float MIN_ANGLE_PER_SEC = 360f;

	private const float ROTATION_TIME = 1.5f;

	private const float MIN_ARROW_MOVE_SPD_PARAM = 0.1f;

	[Range(0.1f, 3f)]
	public float ArrowHitCheckRange;

	[Range(0f, 1f)]
	public float ArrowDownDiecritonTime;

	[Range(0f, 45f)]
	public float ArrowMoveMaxDegree;

	[Range(0f, 10f)]
	public float ArrowHitAreaAngle;

	[Range(0f, 1f)]
	public float ArrowMoveDecreaseRate;

	public float ArrowFixedMoveAngleDegree;

	[Range(0.1f, 1f)]
	public float ArrowEffectStartValue;

	private NShared.GiveRewardHeroInnRoulette.Response _sucessResponse;

	private List<Component> _componentList = new List<Component>();

	private int _anglePerSec;

	private int _sectionCount;

	private int _anglePerSection;

	private Vector2 _vec2RoultteCenter;

	private Vector2 _vec2TouchPos;

	private static int _sectionIndex;

	private bool _clockWise;

	private bool _waitingPakcet;

	private bool _rotating;

	private bool _moveArrow;

	private bool _endTouchDrag;

	private float _moveSpdParam;

	private float _rotateAngle;

	private RouletteRewardInfo[] _rouletteRewardInfo;

	private void Start()
	{
		if (LOCAL_TEST)
		{
			Init();
			SetupRoulette(null);
		}
	}

	private void Update()
	{
		if (Model.renderFacade != null)
		{
			Model.renderFacade.RenderGroupModifier.SetExtraAlpha(1f);
			Model.renderFacade.ChainUpdate();
		}
	}

	public static void Create()
	{
		WindowPrefab<Roulette> windowPrefab = new WindowPrefab<Roulette>("Resources/Prefab2/Window/MiniGame/Roulette", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private bool Init()
	{
		_sectionCount = 8;
		_anglePerSection = 360 / _sectionCount;
		if (_anglePerSection == 0)
		{
			VespaDebug.LogError("_anglePerSection == 0 / _sectionCount : " + _sectionCount);
			return false;
		}
		if (!LOCAL_TEST)
		{
			SetGuideLabel();
			CreateRenderFacade("NPC.Inn", 0, null, 1);
			UIHelper.EnableButton(Button_Play, enable: false, grayscaleIfDisable: true);
		}
		PlayTweenRotation(play: false);
		PlayArrowHitEffect(play: false);
		PlayArrowSlowRotateEffect(play: false);
		UIHelper.EnableGameObject(Label_RewardIndex, LOCAL_TEST);
		return true;
	}

	private bool SetupRoulette(RouletteRewardInfo[] rouletteRewardInfo)
	{
		_rouletteRewardInfo = rouletteRewardInfo;
		if (!LOCAL_TEST)
		{
			SetRemainCount();
			CreateSectionReward();
			SetPlayButton();
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		_rouletteRewardInfo = null;
		StopAndReset();
		if (Model.renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(Model.renderFacade);
		}
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public override void Close()
	{
		if (_rotating && _waitingPakcet)
		{
			VespaDebug.LogError("roulette is Rotating and WaitingPacket ");
			return;
		}
		VespaCoroutine.StopAll(this);
		OnEndRotate();
		base.Close();
	}

	public void OpenInternal()
	{
		base.Open();
		if (!Init())
		{
			VespaDebug.LogError("init failed");
			Close();
			return;
		}
		SendRequestRoultteInfo(delegate(NShared.RequestHeroInnRoulette.Response res)
		{
			if (!SetupRoulette(res.RouletteRewardInfo))
			{
				VespaDebug.LogError("setupDone == false");
				Close();
			}
		}, delegate
		{
			Close();
		});
	}

	private void PlayArrowHitSound()
	{
		if (ArrowHitSound != null)
		{
			ArrowHitSound.Play();
		}
	}

	private void PlayArrowHitEffect(bool play)
	{
		if (play)
		{
			UIHelper.EnableGameObject(ArrowEffect2, enable: false);
		}
		UIHelper.EnableGameObject(ArrowEffect2, play);
	}

	private void PlayArrowSlowRotateEffect(bool play)
	{
		if (play)
		{
			UIHelper.EnableGameObject(ArrowEffect1, enable: false);
		}
		UIHelper.EnableGameObject(ArrowEffect1, play);
	}

	private void PlayTweenRotation(bool play)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Tween_Deco); i++)
		{
			Tween_Deco[i].enabled = play;
		}
	}

	private void SetGuideLabel()
	{
		RouletteData data = GameRoot.Instance.RouletteDataContainer.GetData(1);
		if (data != null)
		{
			string text = ((SingletonAuto<LocalizationManager>.instance == null) ? "@RouletteGuide" : LocalizationManager.GetStringDirect(data.RouletteGuide));
			UIHelper.SetLabelText(Label_Guide, text);
		}
	}

	private void SetPlayButton()
	{
		UIHelper.EnableButton(Button_Play, IsPlayable(), grayscaleIfDisable: true);
	}

	private void SetRemainCount()
	{
		RouletteInfo rouletteInfo = GlobalAutoBehaviour<AccountManager>.instance.RouletteManager.Get(1);
		if (rouletteInfo == null)
		{
			VespaDebug.LogError("roulette index [" + 1 + "]  info is null");
			UIHelper.SetLabelText(Label_RouletteCount, $"{0}/{0}");
		}
		else
		{
			UIHelper.SetLabelText(Label_RouletteCount, $"{rouletteInfo.RemainCount}/{rouletteInfo.MaxCount}");
		}
	}

	private bool IsPlayable()
	{
		if (LOCAL_TEST)
		{
			return true;
		}
		RouletteInfo rouletteInfo = GlobalAutoBehaviour<AccountManager>.instance.RouletteManager.Get(1);
		if (rouletteInfo == null)
		{
			return false;
		}
		if (_rouletteRewardInfo != null)
		{
			return rouletteInfo.RemainCount > 0;
		}
		return false;
	}

	private void CreateSectionReward()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rouletteRewardInfo); i++)
		{
			RouletteTemplateComponent rouletteTemplateComponent = RouletteTemplateComponent.PrefabAuto.Factory.Create(UI_Roulette.gameObject.transform);
			if (!(rouletteTemplateComponent == null))
			{
				string label = (i + 1).ToString();
				rouletteTemplateComponent.Open(eularAngle: new Vector3(rouletteTemplateComponent.transform.localRotation.x, rouletteTemplateComponent.transform.localRotation.y, _anglePerSection / 2 + i * _anglePerSection), label: label, rewards: _rouletteRewardInfo[i].GerRewardDataAry(), sectionIndex: i + 1);
				_componentList.Add(rouletteTemplateComponent);
			}
		}
	}

	private void CreateRenderFacade(string creatureCode, int star, CachedHeroInfo heroInfo, int costumeIndex)
	{
		InitDescLabelLayer();
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		AnimationKeyData.CreateNamedAction("Idle");
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(creatureCode);
		float num = data?.MatchPickBanScale ?? 300f;
		string text = ((data != null) ? data.HeroInnMotion : "Normal");
		Model.RenderPivot.localScale = new Vector3(num, num, num);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(Model.RenderPivot, TeamType.Left, creatureCode, star);
		renderFacadeBuilder.SetCostume(costumeIndex);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		Model.renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (Model.renderFacade != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(Model.renderFacade.gameObject, layerInfo.Layer);
			Model.renderFacade.Action.SetCustomIdle(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, text));
			ModelAction();
		}
	}

	private void ModelAction()
	{
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.MotionEnd = DefineCreature.EMotionEnd.End;
		stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Touch");
		Model.renderFacade.Action.Motion(stockData, blendMotion: true, null);
	}

	private void InitDescLabelLayer()
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(Model.LabelDesc.gameObject, layerInfo.Camera.gameObject.layer);
	}

	private void OnSlowRoulett()
	{
		PlayTweenRotation(play: false);
	}

	private void OnEndRotate()
	{
		PlayTweenRotation(play: false);
		SetPlayButton();
		PlayArrowSlowRotateEffect(play: false);
		ShowRewardPopup();
	}

	private bool SetFakeFriendShipPoint(int section, FriendshipPointResultInfo info)
	{
		if (info != null && info.NewValue > 0 && info.AddValue == 0L)
		{
			RouletteRewardInfo[] rouletteRewardInfo = _rouletteRewardInfo;
			foreach (RouletteRewardInfo rouletteRewardInfo2 in rouletteRewardInfo)
			{
				if (rouletteRewardInfo2 == null || rouletteRewardInfo2.Section != section)
				{
					continue;
				}
				DbRewardData[] array = rouletteRewardInfo2.GerRewardDataAry();
				foreach (DbRewardData dbRewardData in array)
				{
					if (dbRewardData != null && dbRewardData.Type != RewardType.None && dbRewardData.Type == RewardType.FriendshipPoint)
					{
						int value = dbRewardData.Value;
						info.AddValue = value;
						return true;
					}
				}
			}
		}
		return false;
	}

	private void ShowRewardPopup()
	{
		if (_sucessResponse != null)
		{
			SetFakeFriendShipPoint(_sucessResponse.Section, _sucessResponse.FriendshipPointResult);
			string text = ((SingletonAuto<LocalizationManager>.instance == null) ? "@룰렛 보상" : LocalizationManager.GetStringDirect("HEROINN_ROULETTE_REWARD"));
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(text).SetCurrencyResultInfos3(_sucessResponse.CurrencyResults).SetItemResultInfos(_sucessResponse.itemResults)
				.SetEquipResultInfos(_sucessResponse.equipItemInfos)
				.SetFriendshipPointResult(_sucessResponse.FriendshipPointResult), isGlobal: false);
			_sucessResponse = null;
		}
	}

	private void SetRotationAngleForIndex(int sectionIndex)
	{
		int num = sectionIndex - 1;
		int num2 = UnityEngine.Random.Range(5, _anglePerSection - 5);
		int num3 = 0;
		if (_clockWise)
		{
			num3 = num * _anglePerSection + num2;
			num3 *= -1;
		}
		else
		{
			num3 = (_sectionCount - num) * _anglePerSection - num2;
		}
		Debug.LogError("index : " + num + " / angle : " + num3);
		float num4 = _rotateAngle % 360f;
		float num5 = (float)(_clockWise ? (-360) : 360) - num4 + (float)num3;
		float num6 = (float)num3 - num4;
		_rotateAngle += ((System.Math.Abs(num5) < System.Math.Abs(num6)) ? num5 : num6);
	}

	public void OnClickPlay()
	{
		if (!_rotating)
		{
			TryRotate(1080);
		}
	}

	public void OnDragStart()
	{
		if (!_rotating && IsPlayable())
		{
			_vec2TouchPos = UICamera.currentTouch.pos;
			_endTouchDrag = false;
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoTouchRotate());
		}
	}

	public void OnDrag()
	{
		_vec2TouchPos = UICamera.currentTouch.pos;
	}

	public void OnDragEnd()
	{
		_vec2TouchPos = UICamera.currentTouch.pos;
		_endTouchDrag = true;
	}

	private float ArrowMove(float beforeMoveCount, float roatedAngle, float spdParam)
	{
		float num = Mathf.Abs(roatedAngle) / (float)_anglePerSection;
		float num2 = ArrowHitAreaAngle / (float)_anglePerSection;
		float num3 = num + num2;
		if ((int)num3 > (int)beforeMoveCount)
		{
			if (spdParam < 0.1f)
			{
				spdParam = 0.1f;
			}
			_moveSpdParam = spdParam;
			_moveArrow = true;
		}
		return num3;
	}

	private IEnumerator CoRotateRolette(int rotateAngle)
	{
		PlayTweenRotation(play: true);
		_rotating = true;
		_waitingPakcet = true;
		_rotateAngle = rotateAngle;
		float roatedAngle = 0f;
		float beforeMoveCount = 0f;
		while (_waitingPakcet)
		{
			float num = (float)_anglePerSec * Time.deltaTime;
			UI_Roulette.transform.Rotate(new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, num));
			roatedAngle += num;
			beforeMoveCount = ArrowMove(beforeMoveCount, roatedAngle, 0.1f);
			yield return null;
		}
		roatedAngle = 0f;
		beforeMoveCount = 0f;
		UI_Roulette.transform.localEulerAngles = new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, 0f);
		bool startSlow = false;
		while (Mathf.Abs(_rotateAngle) > Mathf.Abs(roatedAngle))
		{
			float num2 = roatedAngle / _rotateAngle;
			Mathf.Pow(num2, Power);
			float num3 = (float)_anglePerSec * (1f - num2);
			if (Mathf.Abs(num3) < 20f)
			{
				if (!startSlow)
				{
					OnSlowRoulett();
					startSlow = true;
				}
				num3 = 20f * (float)System.Math.Sign(num3);
			}
			float num4 = num3 * Time.deltaTime;
			UI_Roulette.transform.Rotate(new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, num4));
			roatedAngle += num4;
			beforeMoveCount = ArrowMove(beforeMoveCount, roatedAngle, num2);
			yield return null;
		}
		UI_Roulette.transform.localEulerAngles = new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, _rotateAngle);
		if (_sucessResponse != null)
		{
			for (int i = 0; i < _componentList.Count; i++)
			{
				RouletteTemplateComponent rouletteTemplateComponent = _componentList[i] as RouletteTemplateComponent;
				if (rouletteTemplateComponent != null && rouletteTemplateComponent.SectionIndex == _sucessResponse.Section)
				{
					rouletteTemplateComponent.PlayEffect();
				}
			}
		}
		PlayArrowHitEffect(play: false);
		yield return new WaitForSeconds(RewardPopupDelaySec);
		OnEndRotate();
		_rotating = false;
	}

	private IEnumerator CoDirectionArrowMove()
	{
		float recTime2 = 0f;
		bool arrowUpMove = false;
		float recoveryTime = 0f;
		while (_rotating || Spr_Arrow.transform.localEulerAngles.z > float.Epsilon)
		{
			float z = Spr_Arrow.transform.localEulerAngles.z;
			float z2 = 0f;
			if (_moveArrow)
			{
				ArrowFixedMoveAngleDegree = ArrowMoveMaxDegree - ArrowMoveMaxDegree * _moveSpdParam * ArrowMoveDecreaseRate;
				if (ArrowFixedMoveAngleDegree < ArrowHitAreaAngle)
				{
					ArrowFixedMoveAngleDegree = ArrowHitAreaAngle;
				}
				_moveArrow = false;
				float num = ArrowHitAreaAngle * ArrowHitCheckRange;
				if ((_clockWise ? (z > num) : (z < 360f - num && z > 270f)) && arrowUpMove)
				{
					continue;
				}
				recoveryTime = 0f;
				arrowUpMove = true;
				recTime2 = ArrowDownDiecritonTime * _moveSpdParam;
				if (_moveSpdParam >= ArrowEffectStartValue)
				{
					PlayArrowSlowRotateEffect(play: true);
				}
				PlayArrowHitEffect(play: true);
				PlayArrowHitSound();
			}
			if (arrowUpMove)
			{
				float num2 = (_clockWise ? (z % 360f) : ((360f - z) % 360f)) / ArrowFixedMoveAngleDegree + (1.01f - _moveSpdParam) * 0.1f;
				z2 = Mathf.LerpAngle(z, _clockWise ? ArrowFixedMoveAngleDegree : (0f - ArrowFixedMoveAngleDegree), num2);
				if (num2 > 1f)
				{
					arrowUpMove = false;
					recoveryTime = Time.time;
					z2 = (_clockWise ? ArrowFixedMoveAngleDegree : (0f - ArrowFixedMoveAngleDegree));
				}
			}
			else if (Spr_Arrow.transform.localEulerAngles.z > float.Epsilon)
			{
				float num3 = (Time.time - recoveryTime) / recTime2;
				z2 = Mathf.LerpAngle(z, 0f, num3);
				if (num3 > 1f)
				{
					z2 = 0f;
				}
			}
			Spr_Arrow.transform.localEulerAngles = new Vector3(0f, 0f, z2);
			yield return null;
		}
	}

	private IEnumerator CoTouchRotate()
	{
		_vec2RoultteCenter = UICamera.currentCamera.WorldToScreenPoint(UI_Roulette.transform.position);
		float lastTouchTime = 0f;
		float num = 0f;
		float rotateAngle = 0f;
		while (!_endTouchDrag)
		{
			Vector2 lastTouch = _vec2TouchPos;
			lastTouchTime = Time.time;
			yield return null;
			Vector2 vec2TouchPos = _vec2TouchPos;
			num = Time.time;
			if (!(Vector2.Distance(lastTouch, vec2TouchPos) < float.Epsilon))
			{
				Vector2 vector = lastTouch - _vec2RoultteCenter;
				Vector2 to = vec2TouchPos - _vec2RoultteCenter;
				rotateAngle = Vector2.Angle(vector, to);
				float num2 = Mathf.Sign(vector.x * to.y - vector.y * to.x);
				_clockWise = !(num2 > 0f);
				UI_Roulette.transform.Rotate(new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, _clockWise ? (0f - rotateAngle) : rotateAngle));
			}
		}
		int num3 = (int)(rotateAngle / (num - lastTouchTime));
		num3 = num3;
		TryRotate(num3);
	}

	private IEnumerator CoTestSendPacket(int index)
	{
		float seconds = UnityEngine.Random.Range(1.5f, 3f);
		yield return new WaitForSeconds(seconds);
		if (UnityEngine.Random.Range(0, 20) == 0)
		{
			StopAndReset();
			yield break;
		}
		NShared.GiveRewardHeroInnRoulette.Response response = new NShared.GiveRewardHeroInnRoulette.Response();
		response.CurrencyResults = new CurrencyResultInfo3[1];
		response.CurrencyResults[0] = new CurrencyResultInfo3();
		response.CurrencyResults[0].CurrencyType = CurrencyType.Gem;
		response.CurrencyResults[0].AddValue = 300L;
		response.CurrencyResults[0].NewValue = 600L;
		response.Section = index;
		response.RouletteInfo = new RouletteInfo
		{
			RouletteInfoIndex = 1,
			RemainCount = 1,
			MaxCount = 5
		};
		OnSuccess(response);
	}

	private bool TryRotate(int anglePerSec)
	{
		if ((float)anglePerSec > 360f)
		{
			_anglePerSec = anglePerSec;
			if (_clockWise)
			{
				_anglePerSec *= -1;
			}
			int rotateAngle = (int)((float)_anglePerSec * 1.5f);
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoRotateRolette(rotateAngle));
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoDirectionArrowMove());
			if (LOCAL_TEST)
			{
				ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoTestSendPacket(_sectionIndex++));
				_sectionIndex %= _sectionCount;
			}
			else
			{
				SendRequestRoultte(delegate(NShared.GiveRewardHeroInnRoulette.Response res)
				{
					SetRemainCount();
					OnSuccess(res);
				}, delegate
				{
					StopAndReset();
				});
			}
			return true;
		}
		return false;
	}

	private void StopAndReset()
	{
		VespaCoroutine.StopAll(this);
		_sucessResponse = null;
		_rotateAngle = 0f;
		_rotating = false;
		_waitingPakcet = false;
		UI_Roulette.transform.localEulerAngles = new Vector3(UI_Roulette.transform.localRotation.x, UI_Roulette.transform.localRotation.y, 0f);
		Spr_Arrow.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		UIHelper.SetLabelText(Label_RewardIndex, "StopAndReset");
	}

	private void OnSuccess(NShared.GiveRewardHeroInnRoulette.Response response)
	{
		if (response == null)
		{
			VespaDebug.LogError("HeroInnRouletteHero.Response == null");
			return;
		}
		_sucessResponse = response;
		SetRotationAngleForIndex(_sucessResponse.Section);
		UIHelper.SetLabelText(Label_RewardIndex, _sucessResponse.Section.ToString());
		_waitingPakcet = false;
	}

	private void SendRequestRoultteInfo(Action<NShared.RequestHeroInnRoulette.Response> onSucess, System.Action OnFailed)
	{
		NShared.RequestHeroInnRoulette.Request request = new NShared.RequestHeroInnRoulette.Request
		{
			RouletteIndex = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RequestHeroInnRoulette.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.RequestHeroInnRoulette.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.RouletteManager.Apply(response.RouletteInfo);
				onSucess(response);
			}
		});
	}

	private void SendRequestRoultte(Action<NShared.GiveRewardHeroInnRoulette.Response> onSucess, System.Action OnFailed)
	{
		_sucessResponse = null;
		NShared.GiveRewardHeroInnRoulette.Request request = new NShared.GiveRewardHeroInnRoulette.Request
		{
			RouletteIndex = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GiveRewardHeroInnRoulette.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
				MessagePopup.OpenError(response.BaseResult, delegate
				{
					Close();
				});
			}
			else if (response.Result != NShared.GiveRewardHeroInnRoulette.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
				MessagePopup.OpenError(response.Result, delegate
				{
					Close();
				});
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.RouletteManager.Apply(response.RouletteInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.itemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.equipItemInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.staminaResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.expResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
				onSucess(response);
			}
		});
	}
}

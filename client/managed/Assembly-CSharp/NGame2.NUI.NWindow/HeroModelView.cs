using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGameAsset.NCreature;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NWindow;

public class HeroModelView : HeroCostumeSceneView
{
	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/HeroModelView";

	public float ZoomDistance = 0.1f;

	public UIComboBox CostumeComboBox;

	public UIButton NextButton;

	public UIButton PrevButton;

	public UILabel AnimationLabel;

	public UIButton ShowSignatureAniamtionButton;

	public UIButton ChangeHeroButton;

	public UIInput HeroIndexInput;

	public UIToggle x0_5SpeedToggle;

	public UIToggle x1_0SpeedToggle;

	private List<Tuple<HeroAnimationViewData, int>> _heroAnimationViewDataList = new List<Tuple<HeroAnimationViewData, int>>();

	private List<CostumeData> _heroCostumeDataList = new List<CostumeData>();

	private CachedHeroInfo _heroInfo;

	private int _currentHeroAnimationIndex;

	private int _heroIndex;

	private bool _isInitHeroAnimation;

	private int _signatureAnimationIndex = -1;

	public HeroInfo CurrentHeroInfo
	{
		get
		{
			if (_heroInfo == null)
			{
				return base.HeroInfo;
			}
			return _heroInfo;
		}
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener, bool useSceneChanger = true)
	{
	}

	public override void Reset()
	{
		base.Reset();
		ResetData();
		SetOriginSpeed(1f);
		x0_5SpeedToggle.value = false;
		x1_0SpeedToggle.value = true;
	}

	protected void Init()
	{
		CostumeComboBox.OnChanged = SelectCostume;
		_heroIndex = base.HeroInfo.HeroIndex;
		HeroIndexInput.value = base.HeroInfo.HeroIndex.ToString();
		_isInitHeroAnimation = false;
		InitHeroCostume();
		x0_5SpeedToggle.value = false;
		x1_0SpeedToggle.value = true;
		SetOriginSpeed(1f);
	}

	private bool InitHeroAnimation()
	{
		if (_motionSequence == null)
		{
			return false;
		}
		HeroAnimationViewData[] datas = GameRoot.Instance.HeroAnimationViewDataContainer.GetDatas(CurrentHeroInfo.HeroIndex);
		if (datas == null)
		{
			return false;
		}
		Dictionary<string, Tuple<HeroAnimationViewData, int>> dictionary = new Dictionary<string, Tuple<HeroAnimationViewData, int>>();
		_motionSequence.Play(0);
		MotionSequenceData[] motionSequenceDatas = _motionSequence.GetMotionSequenceDatas();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(motionSequenceDatas))
		{
			return false;
		}
		_signatureAnimationIndex = -1;
		for (int i = 0; i < motionSequenceDatas.Length; i++)
		{
			MotionSequenceData motionSequenceData = motionSequenceDatas[i];
			string name = motionSequenceData.MotionList[0].Key.PlayName;
			HeroAnimationViewData heroAnimationViewData = Array.Find(datas, (HeroAnimationViewData e) => string.Equals(e.Animation, name));
			if (heroAnimationViewData != null)
			{
				Tuple<HeroAnimationViewData, int> value = new Tuple<HeroAnimationViewData, int>(heroAnimationViewData, i);
				if (!dictionary.ContainsKey(name))
				{
					dictionary.Add(name, value);
				}
			}
		}
		_heroAnimationViewDataList = dictionary.Values.ToList();
		_heroAnimationViewDataList.Sort((Tuple<HeroAnimationViewData, int> l, Tuple<HeroAnimationViewData, int> r) => l.Item1.Index.CompareTo(r.Item1.Index));
		for (int num = 0; num < _heroAnimationViewDataList.Count; num++)
		{
			if (_heroAnimationViewDataList[num].Item1.Animation.Equals("Signature", StringComparison.OrdinalIgnoreCase))
			{
				_signatureAnimationIndex = num;
			}
		}
		_currentHeroAnimationIndex = 0;
		UpdateAnimationButtons();
		UpdateHeroAnimation();
		return true;
	}

	private void UpdateHeroAnimation()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_heroAnimationViewDataList))
		{
			Tuple<HeroAnimationViewData, int> tuple = _heroAnimationViewDataList[_currentHeroAnimationIndex];
			_motionSequence.Play(tuple.Item2);
			string stringDirect = LocalizationManager.GetStringDirect(tuple.Item1.AnimationText);
			UIHelper.SetLabelText(AnimationLabel, stringDirect);
		}
	}

	private void InitHeroCostume()
	{
		if (CurrentHeroInfo == null)
		{
			return;
		}
		_heroCostumeDataList = new List<CostumeData>();
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if (allDatum.HeroIndex == CurrentHeroInfo.HeroIndex && allDatum.IsOpen && !allDatum.IsBonusCostume())
			{
				_heroCostumeDataList.Add(allDatum);
			}
		}
		_heroCostumeDataList.Sort((CostumeData l, CostumeData r) => l.CostumeIndex.CompareTo(r.CostumeIndex));
		bool flag = false;
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_heroCostumeDataList); num++)
		{
			CostumeData costumeData = _heroCostumeDataList[num];
			CostumeComboBox.AddItem(num, _heroCostumeDataList[num].CostumeName);
			if (costumeData.CostumeIndex == CurrentHeroInfo.CostumeIndex)
			{
				CostumeComboBox.SelectItem(num, isNotify: false);
				flag = true;
			}
		}
		if (!flag)
		{
			CostumeComboBox.SelectFirstItem(isNotify: false);
			CurrentHeroInfo.CostumeIndex = _heroCostumeDataList[0].CostumeIndex;
		}
		CostumeComboBox.Reposition();
	}

	private void SelectCostume(int selectedIndex)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_heroCostumeDataList) && _heroCostumeDataList.Count > selectedIndex)
		{
			CostumeData costumeData = _heroCostumeDataList[selectedIndex];
			HeroInfo heroInfo = JsonMarshaler.Clone(CurrentHeroInfo);
			heroInfo.CostumeIndex = costumeData.CostumeIndex;
			ChangeRenderFacadeOnly(heroInfo);
			_isInitHeroAnimation = false;
		}
	}

	private void ResetData()
	{
		_heroAnimationViewDataList.Clear();
		_heroCostumeDataList.Clear();
		CostumeComboBox.Clear();
		_currentHeroAnimationIndex = 0;
		_heroIndex = 0;
		_heroInfo = null;
	}

	public void OnClickNextAnimation()
	{
		int num = _currentHeroAnimationIndex + 1;
		if (_heroAnimationViewDataList.Count > num)
		{
			_currentHeroAnimationIndex = num;
			UpdateAnimationButtons();
			UpdateHeroAnimation();
		}
	}

	public void OnClickPrevAnimation()
	{
		int num = _currentHeroAnimationIndex - 1;
		if (num >= 0)
		{
			_currentHeroAnimationIndex = num;
			UpdateAnimationButtons();
			UpdateHeroAnimation();
		}
	}

	private void UpdateAnimationButtons()
	{
		UIHelper.EnableButton(NextButton, _currentHeroAnimationIndex < __Extension_Method_ICollection__.COUNT(_heroAnimationViewDataList) - 1);
		UIHelper.EnableButton(PrevButton, _currentHeroAnimationIndex > 0);
		UIHelper.EnableButton(ShowSignatureAniamtionButton, _signatureAnimationIndex > -1);
	}

	public void OnSubmitInputHeroIndex()
	{
		int key = Vespa_String_Extension_Method.TOINT(HeroIndexInput.value, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
		UIHelper.EnableButton(ChangeHeroButton, data != null);
	}

	public void OnChangeInputHeroIndex()
	{
		int key = Vespa_String_Extension_Method.TOINT(HeroIndexInput.value, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
		UIHelper.EnableButton(ChangeHeroButton, data != null);
	}

	public void OnClickChangeHero()
	{
		int num = Vespa_String_Extension_Method.TOINT(HeroIndexInput.value, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num);
		if (data != null)
		{
			ResetData();
			_heroIndex = num;
			_heroInfo = new CachedHeroInfo(data);
			ChangeRenderFacadeOnly(CurrentHeroInfo);
			_isInitHeroAnimation = false;
			InitHeroCostume();
		}
	}

	public bool SelectHero(int heroIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo == null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + heroIndex);
				return false;
			}
			cachedHeroInfo = new CachedHeroInfo(data);
		}
		new DelegateTask(SetupHeroWithSceneChange(cachedHeroInfo)).Commit();
		return true;
	}

	protected override void SetCameraController()
	{
		_cameraController = new CustomizingCameraController(_cinema, _heroUid);
		_cameraController.SetCameraInfo(CurrentHeroInfo.HeroIndex, CurrentHeroInfo.CostumeIndex);
		_cameraController.ZoomOutRate = 0.5f;
		_cameraController.ZoomOut_Distance = 15f;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!_isInitHeroAnimation)
		{
			_isInitHeroAnimation = InitHeroAnimation();
		}
	}

	public void OnClickPlayCurrentAnimation()
	{
		if (_motionSequence != null && !_motionSequence.IsPaused())
		{
			UpdateHeroAnimation();
		}
	}

	public void OnClickZoomPlus()
	{
		_cameraController.ZoomOutRate -= ZoomDistance;
	}

	public void OnClickZoomMinus()
	{
		_cameraController.ZoomOutRate -= 0f - ZoomDistance;
	}

	public void OnClickShowAwaken()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_heroIndex);
		if (data != null)
		{
			GetNewHero2.OpenData openData = GetNewHero2.OpenData.CreateSingleFromHeroInfo(new HeroInfo
			{
				HeroIndex = _heroIndex,
				Star = data.StartHeroStar,
				Level = 1
			}, GetNewHero2.EOpenMode.Awaken, hideInfo: true);
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>().Open(openData, null, null);
		}
	}

	public void OnClickShowTranscend()
	{
		if (GameRoot.Instance.CreatureDataContainer.GetData(_heroIndex) != null)
		{
			GetNewHero2.OpenData openData = GetNewHero2.OpenData.CreateSingleFromHeroInfo(new HeroInfo
			{
				HeroIndex = _heroIndex,
				Star = 5,
				Transcended = 5,
				Level = 1
			}, GetNewHero2.EOpenMode.Transcend, hideInfo: true);
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>().Open(openData, null, null);
		}
	}

	public void OnClickShowSignatureAnimation()
	{
		if (_signatureAnimationIndex != -1)
		{
			_currentHeroAnimationIndex = _signatureAnimationIndex;
			UpdateAnimationButtons();
			UpdateHeroAnimation();
		}
	}

	public void OnClickHalfSpeed()
	{
		x0_5SpeedToggle.value = true;
		x1_0SpeedToggle.value = false;
		SetOriginSpeed(0.5f);
	}

	public void OnClickOriginSpeed()
	{
		x0_5SpeedToggle.value = false;
		x1_0SpeedToggle.value = true;
		SetOriginSpeed(1f);
	}

	private void SetOriginSpeed(float scale)
	{
		UnityTimeScaler.SetScale(this, scale);
	}
}

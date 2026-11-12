using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroBook : FrameControlWindow, ArchiveHeroStatPanel.Listener, ArchiveHeroSkillPanel.Listener, IArchiveHeroSubPanelListener
{
	public interface Listener
	{
		void OnPrevHero();

		void OnNextHero();

		void OnShowAction();

		void OnToggleChanged(EToggle toggle);

		void OnOpenArchiveSkillDesc(int skillSlotIndex, int linkedIndex);
	}

	public enum EToggle
	{
		None,
		Profile,
		Stat,
		Skill,
		Transcend,
		Voice,
		Illust
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/HeroBook";

	public UILabel LabelHeroName;

	public UITexture HeroPreviewTexture;

	public Hero3DViewPanel Hero3DViewPanel;

	public UIToggle ToggleProfile;

	public UIToggle ToggleStat;

	public UIToggle ToggleSkill;

	public UIToggle ToggleTranscend;

	public UIToggle ToggleVoice;

	public UIToggle ToggleIllust;

	public UIWidget DisableProfile;

	public UIWidget DisableVoice;

	public UISprite SpriteBlock;

	public ArchiveHeroProfilePanel HeroProfilePanel;

	public ArchiveHeroStatPanel HeroStatPanel;

	public ArchiveHeroSkillPanel HeroSkillPanel;

	public ArchiveHeroTranscendSkillListPanel HeroTranscendSkillListPanel;

	public ArchiveHeroVoicePanel HeroVoicePanel;

	public ArchiveHeroIllustPanel HeroIllustPanel;

	public UIGrid TabGrid;

	public UILabel LabelNotHolding;

	private Listener _listener;

	private CachedHeroInfo _heroInfo;

	private HeroPreview _heroPreview;

	private EToggle _toggle;

	public float RotateSensitivity = 1f;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private Dictionary<EToggle, IArchiveHeroSubPanel> _heroSubPanelDic = new Dictionary<EToggle, IArchiveHeroSubPanel>();

	private bool _isMyHero;

	private WeakRef<RenderFacade> _facadeRef = new WeakRef<RenderFacade>();

	private List<MotionSequenceData> _sequenceList = new List<MotionSequenceData>();

	private VectorXZ _initDirection = new VectorXZ(0f, 1f);

	public CachedHeroInfo HeroInfo => _heroInfo;

	public void Init(Listener listener, CachedHeroInfo heroInfo)
	{
		_listener = listener;
		_heroInfo = heroInfo;
		InitHeroName();
		InitTab();
		SetupHero3D();
		SetupSubPanel();
		SetupMyHero();
	}

	public override void Close()
	{
		_heroSubPanelDic.Clear();
		_toggle = EToggle.None;
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateHeroCamera();
	}

	public override void Reset()
	{
		base.Reset();
		ClosePreview();
	}

	private bool IsMyHero(int heroIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(HeroInfo.HeroIndex) != null;
	}

	private void ClosePreview()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}

	public void SelectHero(CachedHeroInfo heroInfo)
	{
		_heroInfo = heroInfo;
		InitHeroName();
	}

	public void InitHeroName()
	{
		HeroProfileData data = GameRoot.Instance.HeroProfileDataContainer.GetData(_heroInfo.HeroIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.HeroName))
		{
			UIHelper.SetLabelText(LabelHeroName, LocalizationManager.GetStringDirect(data.HeroName));
		}
		else
		{
			UIHelper.SetLabelText(LabelHeroName, _heroInfo.CreatureData.Name);
		}
	}

	public void InitTab()
	{
	}

	public void SetupHero3D()
	{
		if (!(HeroPreviewTexture == null))
		{
			RenderFacadeBuilder renderFacadeBuilder = null;
			if (_heroInfo != null)
			{
				CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(_heroInfo.HeroIndex, _heroInfo.Star);
				_heroInfo.CostumeIndex = dataByHeroStar.CostumeIndex;
				renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _heroInfo);
				renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
				renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			}
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, (_heroInfo != null) ? _heroInfo.HeroIndex : 0, _heroInfo.CostumeIndex, renderFacadeBuilder, "Normal", perspective: false);
			if (_heroPreview.RenderFacade != null)
			{
				_heroPreview.RenderFacade.Action.SetDirection(_initDirection, blendMotion: false);
			}
		}
	}

	private void SetupMyHero()
	{
		_isMyHero = IsMyHero(HeroInfo.HeroIndex);
		UIHelper.EnableGameObject(SpriteBlock, !_isMyHero);
		UIHelper.EnableGameObject(DisableProfile, !_isMyHero);
		UIHelper.EnableGameObject(DisableVoice, !_isMyHero);
		UIHelper.EnableGameObject(ToggleProfile, _isMyHero);
		UIHelper.EnableGameObject(ToggleVoice, _isMyHero);
		TabGrid.Reposition();
		UIHelper.EnableGameObject(LabelNotHolding, !_isMyHero);
	}

	public void OnChangeSubPanel(CachedHeroInfo heroInfo)
	{
		_heroInfo = heroInfo;
		SetupMyHero();
		if (_isMyHero)
		{
			if (_toggle == EToggle.None)
			{
				_toggle = EToggle.Profile;
				ToggleProfile.Set(state: true);
				ToggleStat.Set(state: false);
				ToggleSkill.Set(state: false);
				ToggleTranscend.Set(state: false);
				ToggleVoice.Set(state: false);
				if (_heroSubPanelDic.ContainsKey(_toggle))
				{
					_heroSubPanelDic[_toggle].Enable(enable: true, heroInfo, this);
				}
			}
			else if (_heroSubPanelDic.ContainsKey(_toggle))
			{
				_heroSubPanelDic[_toggle].Change(HeroInfo, this);
			}
		}
		else
		{
			SetupNotMyHero();
		}
	}

	private void SetupNotMyHero()
	{
		if (_heroSubPanelDic.ContainsKey(_toggle))
		{
			_heroSubPanelDic[_toggle].Enable(enable: false, null, this);
			_toggle = EToggle.None;
			ToggleProfile.Set(state: false);
			ToggleStat.Set(state: false);
			ToggleSkill.Set(state: false);
			ToggleTranscend.Set(state: false);
			ToggleVoice.Set(state: false);
		}
	}

	public void SetupSubPanel()
	{
		_heroSubPanelDic.Add(EToggle.Profile, HeroProfilePanel);
		_heroSubPanelDic.Add(EToggle.Stat, HeroStatPanel);
		_heroSubPanelDic.Add(EToggle.Skill, HeroSkillPanel);
		_heroSubPanelDic.Add(EToggle.Transcend, HeroTranscendSkillListPanel);
		_heroSubPanelDic.Add(EToggle.Voice, HeroVoicePanel);
		_heroSubPanelDic.Add(EToggle.Illust, HeroIllustPanel);
		EToggle eToggle = EToggle.Profile;
		ToggleProfile.Set(eToggle == EToggle.Profile);
		ToggleStat.Set(eToggle == EToggle.Stat);
		ToggleSkill.Set(eToggle == EToggle.Skill);
		ToggleTranscend.Set(eToggle == EToggle.Transcend);
		ToggleVoice.Set(eToggle == EToggle.Voice);
		ToggleIllust.Set(eToggle == EToggle.Illust);
		OnToggleChanged();
		if (!IsMyHero(HeroInfo.HeroIndex))
		{
			SetupNotMyHero();
		}
	}

	private void ResponseAction(string motionName, bool bBlendAction = true)
	{
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		if (string.Compare(motionName, "Stun") == 0)
		{
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Stun);
		}
		else if (string.Compare(motionName, "Down") == 0)
		{
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Down);
		}
		else
		{
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, motionName);
		}
		stockData.MotionEnd = DefineCreature.EMotionEnd.End;
		_heroPreview.RenderFacade.Action.Motion(stockData, bBlendAction, null);
	}

	public void OnClickPrevHero()
	{
		_listener.OnPrevHero();
	}

	public void OnClickNextHero()
	{
		_listener.OnNextHero();
	}

	public void OnClickAction()
	{
		_listener.OnShowAction();
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleProfile.value)
		{
			eToggle = EToggle.Profile;
		}
		else if (ToggleStat.value)
		{
			eToggle = EToggle.Stat;
		}
		else if (ToggleSkill.value)
		{
			eToggle = EToggle.Skill;
		}
		else if (ToggleTranscend.value)
		{
			eToggle = EToggle.Transcend;
		}
		else if (ToggleVoice.value)
		{
			eToggle = EToggle.Voice;
		}
		else if (ToggleIllust.value)
		{
			eToggle = EToggle.Illust;
		}
		if (eToggle == EToggle.None || _toggle == eToggle)
		{
			return;
		}
		_toggle = eToggle;
		foreach (KeyValuePair<EToggle, IArchiveHeroSubPanel> item in _heroSubPanelDic)
		{
			EToggle key = item.Key;
			item.Value.Enable(key == _toggle, HeroInfo, this);
		}
		UIHelper.EnableGameObject(LabelNotHolding, enable: false);
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		if (_listener != null)
		{
			_listener.OnToggleChanged(_toggle);
		}
	}

	private void UpdateHeroCamera()
	{
		if (_touchList.Count != 0 && _touchList.Count == 1)
		{
			RenderFacade renderFacade = _heroPreview.RenderFacade;
			if (renderFacade != null)
			{
				UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
				VectorXZ direction = renderFacade.Action.GetDirection();
				direction = Math.Rotate(direction, mouseOrTouch.delta.x * RotateSensitivity);
				renderFacade.Action.SetDirection(direction, blendMotion: false);
			}
		}
	}

	public void OnTouch()
	{
		HeroProfileData data = GameRoot.Instance.HeroProfileDataContainer.GetData(HeroInfo.HeroIndex);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.Emoti))
		{
			string motionName = Vespa_Generic_IList_Extension_Method.RandomGet(data.Emoti);
			ResponseAction(motionName);
		}
	}

	public void OnPress()
	{
		OnDragBegin();
	}

	public void OnRelease()
	{
		OnDragEnd();
	}

	public void OnDragBegin()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
		}
	}

	public void OnDragEnd()
	{
		_touchList.Remove(UICamera.currentTouch);
	}

	void ArchiveHeroSkillPanel.Listener.OnOpenArchiveSkillDesc(int skillSlotIndex, int linkedIndex)
	{
		_listener.OnOpenArchiveSkillDesc(skillSlotIndex, linkedIndex);
	}
}

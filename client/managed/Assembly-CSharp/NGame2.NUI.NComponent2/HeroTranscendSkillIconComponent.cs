using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroTranscendSkillIconComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnSelectSkillComponent(HeroTranscendSkillIconComponent comp);
	}

	public enum ETranscendSkillStatus
	{
		NotLearn,
		CannotLearn,
		CanLearn,
		Learned,
		Viewer
	}

	public const string PrefabPath = "Prefab2/Component/Icon/HeroTranscendSkillIconComponent";

	public const string PrefabPath2 = "Prefab2/Component/Icon/HeroTranscendSkillIcon2Component";

	public static PrefabAuto<HeroTranscendSkillIconComponent> PrefabAuto = new PrefabAuto<HeroTranscendSkillIconComponent>("Prefab2/Component/Icon/HeroTranscendSkillIconComponent");

	public static PrefabAuto<HeroTranscendSkillIconComponent> PrefabAuto2 = new PrefabAuto<HeroTranscendSkillIconComponent>("Prefab2/Component/Icon/HeroTranscendSkillIcon2Component");

	[SerializeField]
	private UITexture _iconTexture;

	[SerializeField]
	private UISprite _spriteLearned;

	public TooltipTextInfo TooltipTextInfo;

	private Listener _listener;

	public UIGame3DWidget EffectAbleToLearn;

	public UIGame3DWidget EffectLearned;

	public UILabel LabelPoint;

	private HeroInfo _heroInfo;

	private SkillData _SkillData;

	private int Page;

	private ETranscendSkillStatus _SkillStatus;

	public CreatureData.TranscendSkill TranscendSkill { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (EffectAbleToLearn != null)
		{
			EffectAbleToLearn.Stop(immediatelyClear: true);
		}
	}

	public void Open(HeroInfo heroInfo, int page, CreatureData.TranscendSkill skill, Listener listener, bool readOnly = false)
	{
		_listener = listener;
		_heroInfo = heroInfo;
		Page = page;
		TranscendSkill = skill;
		_SkillData = GameRoot.Instance.SkillDataContainer.GetData(skill.TranscendSkillIndex);
		if (_SkillData == null)
		{
			VespaDebug.LogError("skill data is null, skillIndex = " + skill.TranscendSkillIndex);
			return;
		}
		AssetTexture skillIcon = UIHelper.GetSkillIcon2(skill.TranscendSkillIndex, heroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, _iconTexture, skillIcon);
		_SkillStatus = GetTranscendSkillStatus();
		SetSkillStatus(_SkillStatus);
		UIHelper.SetLabelText(LabelPoint, skill.ReqSKillPoint);
		if (_SkillStatus == ETranscendSkillStatus.CanLearn)
		{
			if (!readOnly)
			{
				if (EffectAbleToLearn != null)
				{
					EffectAbleToLearn.Play(null);
				}
			}
			else
			{
				UIHelper.SetWidgetGrayScale(_iconTexture, grayScale: true);
			}
		}
		else if (EffectAbleToLearn != null)
		{
			EffectAbleToLearn.Stop(immediatelyClear: true);
		}
		SetupTooltipText();
	}

	public void Open(HeroInfo heroInfo, CreatureData.TranscendSkill skill)
	{
		TranscendSkill = skill;
		_heroInfo = heroInfo;
		_SkillData = GameRoot.Instance.SkillDataContainer.GetData(skill.TranscendSkillIndex);
		if (_SkillData == null)
		{
			VespaDebug.LogError("skill data is null, skillIndex = " + skill.TranscendSkillIndex);
			return;
		}
		AssetTexture skillIcon = UIHelper.GetSkillIcon2(skill.TranscendSkillIndex, heroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, _iconTexture, skillIcon);
		_SkillStatus = ETranscendSkillStatus.CanLearn;
		if (EffectAbleToLearn != null)
		{
			EffectAbleToLearn.Play(null);
		}
		_iconTexture.isGrayscale = false;
		SetupTooltipText();
	}

	public void SetSkillStatus(ETranscendSkillStatus skillStatus)
	{
		_SkillStatus = skillStatus;
		UIHelper.SetWidgetGrayScale(_iconTexture, _SkillStatus == ETranscendSkillStatus.NotLearn || _SkillStatus == ETranscendSkillStatus.CannotLearn);
		UIHelper.EnableGameObject(_spriteLearned, _SkillStatus == ETranscendSkillStatus.Learned);
	}

	public void Refresh()
	{
		ETranscendSkillStatus skillStatus = _SkillStatus;
		_SkillStatus = GetTranscendSkillStatus();
		SetSkillStatus(_SkillStatus);
		if (_SkillStatus == ETranscendSkillStatus.CanLearn)
		{
			if (EffectAbleToLearn != null)
			{
				if (EffectAbleToLearn.IsPlaying)
				{
					EffectAbleToLearn.Stop(immediatelyClear: true);
				}
				EffectAbleToLearn.Play(null);
			}
		}
		else if (EffectAbleToLearn != null)
		{
			EffectAbleToLearn.Stop(immediatelyClear: true);
		}
		if (skillStatus == ETranscendSkillStatus.CanLearn && _SkillStatus == ETranscendSkillStatus.Learned && EffectLearned != null)
		{
			EffectLearned.Play(null);
		}
		SetupTooltipText();
	}

	private ETranscendSkillStatus GetTranscendSkillStatus()
	{
		if (TranscendSkill == null)
		{
			return ETranscendSkillStatus.NotLearn;
		}
		if (_heroInfo.Transcended < TranscendSkill.Transcended)
		{
			return ETranscendSkillStatus.NotLearn;
		}
		TranscendSkillPageInfo transcendSkillPageInfo = _heroInfo.GetTranscendSkillPageInfo(Page);
		if (transcendSkillPageInfo == null)
		{
			return ETranscendSkillStatus.NotLearn;
		}
		if (TranscendSkill.IsHeroSkillTranscend())
		{
			int pairIndex = ((TranscendSkill.Index % 2 == 0) ? (TranscendSkill.Index + 1) : (TranscendSkill.Index - 1));
			if (transcendSkillPageInfo.TranscendSkillDatas.Find((CreatureData.TranscendSkill skill) => skill.Transcended == TranscendSkill.Transcended && skill.Index == pairIndex) != null)
			{
				return ETranscendSkillStatus.CannotLearn;
			}
		}
		if (transcendSkillPageInfo.TranscendSkillDatas.Find((CreatureData.TranscendSkill skill) => skill.TranscendSkillIndex == TranscendSkill.TranscendSkillIndex) != null)
		{
			return ETranscendSkillStatus.Learned;
		}
		if (_heroInfo.GetUsableTranscendSkillPoint(Page) < TranscendSkill.ReqSKillPoint)
		{
			return ETranscendSkillStatus.NotLearn;
		}
		return ETranscendSkillStatus.CanLearn;
	}

	public void OnClickComponent()
	{
		if (_listener != null && (_SkillStatus == ETranscendSkillStatus.CanLearn || _SkillStatus == ETranscendSkillStatus.Viewer))
		{
			_listener.OnSelectSkillComponent(this);
		}
	}

	public void OnShowTooltip()
	{
		TranscendSkillIconTooltip.Open(_iconTexture, TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		TranscendSkillIconTooltip.Hide(_iconTexture);
	}

	private void SetupTooltipText()
	{
		if (_SkillData != null)
		{
			if (TooltipTextInfo == null)
			{
				TooltipTextInfo = new TooltipTextInfo();
			}
			TooltipTextInfo.Reset();
			TooltipTextInfo.Name.Value = SkillDescContainer.GetSkillName(_heroInfo, _SkillData);
			TooltipTextInfo.Desc.Assign(new SkillDescBuilder(TranscendSkill.TranscendSkillIndex, _heroInfo, appendLine: false));
			if (_SkillStatus == ETranscendSkillStatus.NotLearn && TranscendSkill.Transcended > _heroInfo.Transcended)
			{
				EnumBuilder stringBuilder = new EnumBuilder().Key(HeroTranscend.INSUFFICIENT_TRANSCENDED).Params(TranscendSkill.Transcended).EncodeColor(Color.red);
				TooltipTextInfo.TypeText.Assign(stringBuilder);
			}
			else
			{
				EnumBuilder stringBuilder2 = new EnumBuilder().Key(HeroTranscend.SUFFICIENT_TRANSCENDED).Params(TranscendSkill.Transcended).EncodeColor(Color.white);
				TooltipTextInfo.TypeText.Assign(stringBuilder2);
			}
		}
	}
}

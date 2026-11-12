using System.Collections;
using System.Collections.Generic;
using NGame2.NScene;
using NGame2.NUI.NHelper.NGuild;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressDungeonComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate, ChallengeRaidBanInfoComponent.Listener
{
	public interface Listener
	{
		void OnClickEnter(GuildSuppressDungeonComponent caller);

		void OnClickViewInfo(GuildSuppressDungeonComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressDungeonComponent";

	public static PrefabAuto<GuildSuppressDungeonComponent> PrefabAuto = new PrefabAuto<GuildSuppressDungeonComponent>("Prefab2/Component/GuildSuppress/GuildSuppressDungeonComponent");

	private Listener _listener;

	public UILabel NameLabel;

	public UILabel NamePrefixLabel;

	public UITexture BossImageTexture;

	public UIClipSprite HpGaugeSprite;

	public UILabel RemainHpLabel;

	public UIButton EnterButton;

	public Transform OpenedRoot;

	public Transform ClearedRoot;

	public UIGame3DWidget ClearEffectWidget;

	public UIGame3DWidget OpenedNormalEffectWidget;

	public float ClearEffectDelay = 0.2f;

	public float OpeningEffectDelay = 1f;

	public float FadeDuration = 1f;

	public UIBaseGrid GridBan;

	private int _level;

	private GuildSuppressChapterData _guildSuppressChapterData;

	private GuildSuppressDungeonData _guildSuppressDungeonData;

	private GuildSuppressPlayInfo _guildSuppressPlayInfo;

	private CreatureData _bossCreatureData;

	private CampaignWaveData _bossWaveData;

	private BanRuleData _banData;

	private List<ChallengeRaidBanInfoComponent> _banComponentList = new List<ChallengeRaidBanInfoComponent>();

	private bool _isOpen;

	private bool _playClearEffect;

	private bool _openedEffectEnabled;

	private bool _IsComplete;

	public GuildSuppressDungeonData GuildSuppressDungeonData => _guildSuppressDungeonData;

	public CreatureData BossCreatureData => _bossCreatureData;

	public GuildSuppressPlayInfo GuildSuppressPlayInfo => _guildSuppressPlayInfo;

	public int Level => _level;

	public BanRuleData BanData => _banData;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_isOpen = false;
		_playClearEffect = false;
		_openedEffectEnabled = false;
		_IsComplete = false;
		ClearBanList();
	}

	private void ClearBanList()
	{
		FactoryBase.ReleaseAndClearList(_banComponentList);
	}

	public void Open(Listener listener, int level, GuildSuppressDungeonData guildSuppressDungeonData, GuildSuppressPlayInfo guildSuppressPlayInfo, bool clearEffect)
	{
		_listener = listener;
		_level = level;
		_guildSuppressDungeonData = guildSuppressDungeonData;
		_guildSuppressChapterData = GameRoot.Instance.GuildSuppressChapterDataContainer.GetData(guildSuppressPlayInfo.RaidIndex);
		_guildSuppressPlayInfo = guildSuppressPlayInfo;
		_isOpen = true;
		_playClearEffect = clearEffect;
		_openedEffectEnabled = false;
		_IsComplete = false;
		Init();
		InitBossImage();
		InitBanInfo();
	}

	private void Init()
	{
		int raidIndex = _guildSuppressPlayInfo.RaidIndex;
		_bossCreatureData = GuildSuppressHelper.GetBossCreatureData(raidIndex, _level, out _bossWaveData);
		UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
		UIHelper.EnableButton(EnterButton, enable: false);
		UIHelper.EnableGameObject(OpenedRoot, enable: false);
		UIHelper.EnableGameObject(ClearedRoot, enable: false);
		if (_bossCreatureData != null)
		{
			string visualInfo = ((_bossWaveData != null) ? _bossWaveData.BossVisualInfo : null);
			string creatureName = string.Empty;
			string creatureNamePrefix = string.Empty;
			GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
			UIHelper.SetLabelText(NameLabel, creatureName);
			UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
		}
	}

	private void InitBossImage()
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(_guildSuppressDungeonData.PortraitPath);
		UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, plainTexture, UIHelper.EAssetTextureAssign.None, delegate
		{
			InitState();
			MakeEffectReady();
		});
	}

	private void InitState()
	{
		if (!_IsComplete)
		{
			UIHelper.SetWidgetGrayScale(BossImageTexture, grayScale: false);
			UIHelper.EnableGameObject(HpGaugeSprite, enable: true);
			UIHelper.EnableButton(EnterButton, enable: true);
			UIHelper.EnableGameObject(OpenedRoot, enable: true);
			UIHelper.EnableGameObject(ClearedRoot, enable: false);
			InitHpGauge();
		}
		else
		{
			UIHelper.SetWidgetGrayScale(BossImageTexture, grayScale: true);
			UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
			UIHelper.EnableButton(EnterButton, enable: false);
			UIHelper.EnableGameObject(OpenedRoot, enable: false);
			UIHelper.EnableGameObject(ClearedRoot, enable: true);
		}
	}

	private void InitHpGauge()
	{
		int sessionIndex = _guildSuppressPlayInfo.SessionIndex;
		int raidIndex = _guildSuppressPlayInfo.RaidIndex;
		int level = _level;
		if (!GuildSuppressHelper.GetBossHp(sessionIndex, raidIndex, level, _guildSuppressPlayInfo.MonsterHp0, out var bossRemainHp, out var bossMaxHp))
		{
			UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
			return;
		}
		float num = 1f - (float)bossRemainHp / (float)bossMaxHp;
		float fillAmount = Mathf.Clamp01(1f - num);
		UIHelper.SetSpriteFillAmount(HpGaugeSprite, fillAmount);
		BattleDefineData battleDefineData = GuildSuppressHelper.GetBattleDefineData(raidIndex, level);
		if (battleDefineData == null || battleDefineData.HPPercent)
		{
			int num2 = Mathf.CeilToInt((float)bossRemainHp / (float)bossMaxHp * 100f);
			UIHelper.SetLabelText(RemainHpLabel, $"{num2}%");
		}
		else
		{
			UIHelper.SetLabelText(RemainHpLabel, Vespa_Long_Extension_Method.ToCommaString(bossRemainHp));
		}
	}

	private void InitBanInfo()
	{
		if (_guildSuppressDungeonData == null)
		{
			return;
		}
		_banData = GuildSuppressHelper.GetBanRuleData(_guildSuppressDungeonData);
		if (_banData == null)
		{
			VespaDebug.LogError("banData == null // _guildSuppressDungeonData.BanIndex : " + _guildSuppressDungeonData.BanIndex);
			return;
		}
		GuildSuppressHelper.CreateBanComponent(_banData, delegate(RaidBanType banType, string banValue)
		{
			ChallengeRaidBanInfoComponent challengeRaidBanInfoComponent = UIHelper.AddGridItem(GridBan, ChallengeRaidBanInfoComponent.PrefabAuto);
			challengeRaidBanInfoComponent.Open(this, banType, banValue);
			_banComponentList.Add(challengeRaidBanInfoComponent);
		});
		UIHelper.EnableGameObject(GridBan, enable: false);
		UIHelper.EnableGameObject(GridBan, enable: true);
		UIHelper.RepositionGrid(GridBan);
	}

	public void Refresh()
	{
		if (_guildSuppressPlayInfo.SessionIndex != 0)
		{
			Init();
			InitState();
			if (!_IsComplete)
			{
				StartOpenedEffect();
			}
			else
			{
				StopOpenedEffect();
			}
		}
	}

	public void SetComplete()
	{
		_IsComplete = true;
		UIHelper.SetWidgetGrayScale(BossImageTexture, grayScale: true);
		UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
		UIHelper.EnableButton(EnterButton, enable: false);
		UIHelper.EnableGameObject(OpenedRoot, enable: false);
		UIHelper.EnableGameObject(ClearedRoot, enable: true);
		StopOpenedEffect();
	}

	private void MakeEffectReady()
	{
		if (_playClearEffect)
		{
			VespaCoroutine.Start(this, coBeginFade(ClearEffectWidget, ClearEffectDelay));
		}
	}

	public void StartOpenedEffect()
	{
		if (OpenedNormalEffectWidget != null)
		{
			OpenedNormalEffectWidget.Play(null);
		}
	}

	public void StopOpenedEffect()
	{
		if (OpenedNormalEffectWidget != null && OpenedNormalEffectWidget.IsPlaying)
		{
			OpenedNormalEffectWidget.Stop(immediatelyClear: true);
		}
	}

	public void StartFadeEffect()
	{
		_openedEffectEnabled = true;
	}

	private IEnumerator coBeginFade(UIGame3DWidget effectWidget, float delay)
	{
		GameObject cloned = Object.Instantiate(BossImageTexture.gameObject);
		cloned.transform.parent = BossImageTexture.transform.parent;
		cloned.transform.localPosition = BossImageTexture.transform.localPosition;
		cloned.transform.localScale = BossImageTexture.transform.localScale;
		cloned.transform.localRotation = BossImageTexture.transform.localRotation;
		UITexture clonedTexture = cloned.GetComponent<UITexture>();
		clonedTexture.isGrayscale = !clonedTexture.isGrayscale;
		clonedTexture.alpha = 1f;
		clonedTexture.depth = BossImageTexture.depth + 1;
		clonedTexture.SetDirty();
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		while (!_openedEffectEnabled)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		if ((bool)effectWidget)
		{
			effectWidget.Play(null);
		}
		float startTime = Time.time;
		while (Time.time < startTime + FadeDuration && _isOpen)
		{
			yield return null;
			float num = Time.time - startTime;
			float alpha = Mathf.Clamp01(1f - num / FadeDuration);
			clonedTexture.alpha = alpha;
		}
		yield return null;
		VespaObject.DestroyGo(cloned);
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this);
		}
	}

	public void OnClickViewInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickViewInfo(this);
		}
	}

	void ChallengeRaidBanInfoComponent.Listener.OnShowTooltip(ChallengeRaidBanInfoComponent comp)
	{
		TooltipTextInfo tooltipTextInfo = new TooltipTextInfo();
		tooltipTextInfo.Name.Value = _banData.RuleName;
		tooltipTextInfo.Desc.Value = _banData.RuleName;
		ChallengeRaidBanTooltip.Open(comp.GetComponent<UIWidget>(), tooltipTextInfo);
	}

	void ChallengeRaidBanInfoComponent.Listener.OnHideTooltip(ChallengeRaidBanInfoComponent comp)
	{
		ChallengeRaidBanTooltip.Hide(comp.GetComponent<UIWidget>());
	}

	public void Tutorial_BanInfo(int[] banHeroIndexArray)
	{
		ClearBanList();
		foreach (int key in banHeroIndexArray)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
			if (data != null)
			{
				ChallengeRaidBanInfoComponent challengeRaidBanInfoComponent = UIHelper.AddGridItem(GridBan, ChallengeRaidBanInfoComponent.PrefabAuto);
				challengeRaidBanInfoComponent.Open(this, RaidBanType.CodeName, data.CodeName);
				_banComponentList.Add(challengeRaidBanInfoComponent);
			}
		}
		UIHelper.EnableGameObject(GridBan, enable: false);
		UIHelper.EnableGameObject(GridBan, enable: true);
		UIHelper.RepositionGrid(GridBan);
	}
}

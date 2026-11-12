using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGameAsset.NCreature;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroEnterDungeon : BaseWindow, ChallengeReqItemComponent.Listener
{
	public interface Listener
	{
		void EnterHeroDungeon(CachedHeroInfo heroInfo);

		void BattleReady(CachedHeroInfo heroInfo);

		int GetHeroAwakeStarItemCount(int star, int transcended);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/HeroTrialDungeon/HeroEnterDungeon";

	public const string SealsActivate = "Activate";

	public UISimpleGrid EnterReqItemGrid;

	public UIButton Button_Enter;

	public UIGame3DWidget SealsEffect;

	public UIGame3DWidget TranscendSealsEffect;

	public UIWidget WidgetGate;

	public UIWidget WidgetBossInfo;

	public UITexture Tex3dRender;

	public UILabel Label_BossName;

	public UILabel Label_BossLevel;

	public GameObject[] Stars;

	public UIButton Button_Ready;

	public UILabel Label_ReqItemDesc;

	public UILabel Label_ChallengeReady;

	public UILabel Label_TranscendChallengeReady;

	public UILabel Label_EnterDungeonTitle;

	public UIPlayAssetSound OpenLockSound;

	private CachedHeroInfo _HeroInfo;

	private CreatureData _creatureData;

	private CreatureAwakeChallengeItemData _ChallengeItemData;

	private Listener _listener;

	private List<ChallengeReqItemComponent> _ListChallengeReqItems = new List<ChallengeReqItemComponent>();

	private MonsterHeroInfo _BossInfo;

	private HeroPreview _boss3dPreview;

	public override void Reset()
	{
		base.Reset();
		Close3dModel();
		if (SealsEffect != null)
		{
			SealsEffect.Stop(immediatelyClear: true);
		}
		if (TranscendSealsEffect != null)
		{
			TranscendSealsEffect.Stop(immediatelyClear: true);
		}
		_HeroInfo = null;
		_creatureData = null;
		_ChallengeItemData = null;
		_BossInfo = null;
		FactoryBase.ReleaseAndClearList(_ListChallengeReqItems);
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener)
	{
		Open(delegate
		{
			Init(heroInfo, listener);
		}, null);
	}

	public void Init(CachedHeroInfo heroInfo, Listener listener)
	{
		if (heroInfo == null)
		{
			VespaDebug.LogError("heroInfo is null");
			return;
		}
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (_creatureData == null)
		{
			VespaDebug.LogError("creatureData is null");
			return;
		}
		_ChallengeItemData = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(_creatureData.TagType);
		if (_ChallengeItemData == null)
		{
			VespaDebug.LogError("AwakeChallengeItemData is null");
			return;
		}
		_listener = listener;
		_HeroInfo = heroInfo;
		UIHelper.SetLabelText(Label_EnterDungeonTitle, LocalizationManager.GetString(HeroTrial.TRIAL_ENTER_DUNGEON_TITLE, _HeroInfo.CreatureData.Name));
		if (_HeroInfo.EnterDungeon == 0)
		{
			SetupGate();
		}
		else
		{
			SetupBossInfo();
		}
		SetupChallengeHeroReqItemList();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	private void Close3dModel()
	{
		UIHelper.CloseHero3dPreview(ref _boss3dPreview, Tex3dRender);
	}

	private void Setup3dModel()
	{
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _BossInfo.HeroInfo, null);
		renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
		UIHelper.InitHero3dPreview(ref _boss3dPreview, Tex3dRender, _BossInfo.HeroInfo.HeroIndex, 0, renderFacadeBuilder, null, perspective: true);
	}

	private void SetupGate()
	{
		UIHelper.EnableGameObject(WidgetGate, enable: true);
		UIHelper.EnableGameObject(Button_Enter, enable: true);
		UIHelper.EnableGameObject(WidgetBossInfo, enable: false);
		UIHelper.EnableGameObject(Button_Ready, enable: false);
		UIHelper.EnableGameObject(Label_ReqItemDesc, enable: true);
		UIHelper.EnableGameObject(Label_ChallengeReady, enable: false);
		UIHelper.EnableGameObject(Label_TranscendChallengeReady, enable: false);
		InitSeals();
		for (int i = 0; i < Stars.Length; i++)
		{
			UIHelper.EnableGameObject(Stars[i], enable: false);
		}
	}

	private UIGame3DWidget GetSealsEffectWidget()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		if (_HeroInfo.Star < valueInt)
		{
			return SealsEffect;
		}
		return TranscendSealsEffect;
	}

	private bool DecideChallengeable()
	{
		if (_HeroInfo == null || _ChallengeItemData == null)
		{
			return false;
		}
		int heroAwakeStarItemCount = GetHeroAwakeStarItemCount(_HeroInfo.Star, _HeroInfo.Transcended);
		for (int i = 0; i < 10; i++)
		{
			string itemCode = _ChallengeItemData.GetItemCode(i + 1);
			if (String_Extension_Method.IsNullOrEmpty(itemCode))
			{
				break;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode == null)
			{
				return false;
			}
			if (Helper.GetItemCount(dataByCode.Index) < heroAwakeStarItemCount)
			{
				return false;
			}
		}
		return true;
	}

	private int GetHeroAwakeStarItemCount(int star, int transcended)
	{
		if (_listener != null)
		{
			return _listener.GetHeroAwakeStarItemCount(star, transcended);
		}
		return HeroTrialDungeonManagement.GetAwakeHeroItemCount(star, transcended);
	}

	private void SetupChallengeHeroReqItemList()
	{
		int heroAwakeStarItemCount = GetHeroAwakeStarItemCount(_HeroInfo.Star, _HeroInfo.Transcended);
		for (int i = 0; i < 10; i++)
		{
			string empty = string.Empty;
			empty = ((!NGame2.NAccount.UserHeroManager.IsMaxStar(_HeroInfo)) ? _ChallengeItemData.GetItemCode(i + 1) : _ChallengeItemData.GetTranscendItemCode(i + 1));
			if (String_Extension_Method.IsNullOrEmpty(empty))
			{
				break;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(empty);
			if (dataByCode == null)
			{
				break;
			}
			CreateChallengeReqItemComponent(dataByCode, Helper.GetItemCount(dataByCode.Index), heroAwakeStarItemCount, _HeroInfo.EnterDungeon != 0);
		}
		if (NGame2.NAccount.UserHeroManager.IsMaxStar(_HeroInfo))
		{
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(_HeroInfo.Star, _HeroInfo.Transcended);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(nextData.HeroTranscendItemIndex);
			CreateChallengeReqItemComponent(data, Helper.GetItemCount(data.Index), nextData.HeroTranscendItemCount, _HeroInfo.EnterDungeon != 0);
		}
		UIHelper.RepositionGrid(EnterReqItemGrid);
		if (_HeroInfo.EnterDungeon == 0)
		{
			UIHelper.EnableButton(Button_Enter, enable: false);
		}
	}

	private void CreateChallengeReqItemComponent(ItemData itemData, int ItemCount, int ReqItemCount, bool Challengeable)
	{
		ChallengeReqItemComponent challengeReqItemComponent = UIHelper.AddGridItem(EnterReqItemGrid, ChallengeReqItemComponent.PrefabAuto);
		challengeReqItemComponent.Open(itemData, ItemCount, ReqItemCount, this);
		if (Challengeable)
		{
			challengeReqItemComponent.SetUsed();
		}
		_ListChallengeReqItems.Add(challengeReqItemComponent);
	}

	private void InitSeals()
	{
		UIGame3DWidget sealsEffectWidget = GetSealsEffectWidget();
		if (sealsEffectWidget != null)
		{
			sealsEffectWidget.Stop(immediatelyClear: true);
			sealsEffectWidget.Play(null);
			sealsEffectWidget.PlayEntryPhase(0, "Activate");
		}
	}

	private int GetCurUnlockedSealCount()
	{
		int num = 0;
		for (int i = 0; i < _ListChallengeReqItems.Count; i++)
		{
			if (_ListChallengeReqItems[i].Used)
			{
				num++;
			}
		}
		return num;
	}

	void ChallengeReqItemComponent.Listener.OnClickOpen(ChallengeReqItemComponent comp)
	{
		if (comp.Used)
		{
			return;
		}
		bool flag = Helper.GetItemCount(comp.ItemData.Index) - comp.reqItemCount >= 0;
		if (!flag)
		{
			CenterMessage.Open(LocalizationManager.GetString(HeroTrial.RESULT_TYPE_NOT_ENOUGH_MATERIAL));
			return;
		}
		UseMessagePopup.OpenUseToUnlockSeal(comp.ItemData, comp.reqItemCount, flag, delegate
		{
			int curUnlockedSealCount = GetCurUnlockedSealCount();
			comp.SetUsed();
			UIGame3DWidget sealsEffectWidget = GetSealsEffectWidget();
			if (sealsEffectWidget != null)
			{
				int entryIndex = curUnlockedSealCount + 1;
				sealsEffectWidget.PlayEntryPhase(entryIndex, "Activate");
				if (OpenLockSound != null)
				{
					OpenLockSound.Play();
				}
			}
			if (GetCurUnlockedSealCount() == _ListChallengeReqItems.Count)
			{
				SingletonUpdater<Executer>.instance.DelayRun(0.5f, this, UpdateEnterButton);
			}
		});
	}

	public void OnClickOpen()
	{
		if (_listener != null)
		{
			_listener.EnterHeroDungeon(_HeroInfo);
		}
	}

	public void OnClickBattleReady()
	{
		if (_listener != null)
		{
			_listener.BattleReady(_HeroInfo);
		}
	}

	public void OnClickBackButton()
	{
		Close();
	}

	private void UpdateEnterButton()
	{
		int curUnlockedSealCount = GetCurUnlockedSealCount();
		UIHelper.EnableButton(Button_Enter, curUnlockedSealCount == _ListChallengeReqItems.Count);
	}

	private void SetupChallengeReadyText()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		UIHelper.EnableGameObject(Label_ChallengeReady, _HeroInfo.Star < valueInt);
		UIHelper.EnableGameObject(Label_TranscendChallengeReady, _HeroInfo.Star >= valueInt);
	}

	private void SetupBossInfo()
	{
		UIHelper.EnableGameObject(WidgetGate, enable: false);
		UIHelper.EnableGameObject(Button_Enter, enable: false);
		UIHelper.EnableGameObject(WidgetBossInfo, enable: true);
		UIHelper.EnableGameObject(Button_Ready, enable: true);
		UIHelper.EnableGameObject(Label_ReqItemDesc, enable: false);
		SetupChallengeReadyText();
		CampaignDungeonData trialDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetTrialDungeonData(_HeroInfo.HeroIndex, _HeroInfo.Star, _HeroInfo.Transcended);
		if (trialDungeonData == null)
		{
			VespaDebug.LogError("dungeon data not found, HeroIndex = " + _HeroInfo.HeroIndex + " Star = " + _HeroInfo.Star);
			return;
		}
		List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(trialDungeonData.ChapterIndex, trialDungeonData.DungeonIndex, trialDungeonData.DungeonIndex, NShared.DungeonDifficultyType.Easy));
		if (__Extension_Method_ICollection__.IsNullOrEmpty(monsterInfoFromDungeon))
		{
			VespaDebug.LogError("monster info not found, ChapterIndex = " + trialDungeonData.ChapterIndex + " Star = " + trialDungeonData.DungeonIndex);
			return;
		}
		_ = monsterInfoFromDungeon[0].IsBoss;
		_BossInfo = monsterInfoFromDungeon[0];
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_BossInfo.HeroInfo.HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("creature data not found creatureIndex = " + _BossInfo.HeroInfo.HeroIndex);
			return;
		}
		UIHelper.SetLabelText(Label_BossName, data.Name);
		UIHelper.SetLabelText(Label_BossLevel, _BossInfo.HeroInfo.Level);
		for (int i = 0; i < Stars.Length; i++)
		{
			UIHelper.EnableGameObject(Stars[i], i == _BossInfo.HeroInfo.Star - 1);
		}
		Setup3dModel();
	}
}

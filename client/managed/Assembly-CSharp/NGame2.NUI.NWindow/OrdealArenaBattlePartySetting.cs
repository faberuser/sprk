using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NOrdeal;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class OrdealArenaBattlePartySetting : BattlePartySetting, HeroPortraitComponent.Listener
{
	public new interface IDataProvider : BattlePartySetting.IDataProvider
	{
		List<CachedHeroInfo> GetEnemyHeroList();

		bool IsDeadHero(int index);
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaBattlePartySetting";

	public UISimpleGrid EnemyGrid;

	private List<HeroPortraitComponent> _listEnemyProtrait = new List<HeroPortraitComponent>();

	private new IDataProvider _dataProvider;

	private int[] _cachedDeadHeroIndices;

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		Open(listener, dataProvider, delegate
		{
			SceneAutoBehaviour<OrdealArenaManagement>.instance.AddUpdateWindow(this, UpdateDeadHeros);
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FactoryBase.ReleaseAndClearList(_listEnemyProtrait);
		_cachedDeadHeroIndices = null;
	}

	protected override void SetSquardEmptySlot()
	{
		_currentSquard = GetSquardUi(SquardUi.ESquardType.OnlyMain).Squard;
		if (_currentSquard != null)
		{
			_currentSquard.InitSquard(4, 0, this);
			_currentSquard.InitAnimationSquardPostionAction(AnimateSquardPositionAction);
		}
		else
		{
			VespaDebug.LogError("Current Squard Is Null");
		}
	}

	private void SetEnemySquard()
	{
		if (EnemyGrid == null)
		{
			return;
		}
		List<CachedHeroInfo> enemeyHeroList = GetEnemeyHeroList();
		SortHeroList(enemeyHeroList);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(enemeyHeroList); i++)
		{
			CachedHeroInfo cachedHeroInfo = enemeyHeroList[i];
			if (cachedHeroInfo != null)
			{
				HeroPortraitComponent heroPortraitComponent = EnemyGrid.AddGridItem(HeroPortraitComponent.SmallPrefabAuto);
				if (!(heroPortraitComponent == null))
				{
					heroPortraitComponent.Open(cachedHeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
					_listEnemyProtrait.Add(heroPortraitComponent);
				}
			}
		}
		UIHelper.RepositionGrid(EnemyGrid);
	}

	private void SortHeroList(List<CachedHeroInfo> heroList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroList))
		{
			return;
		}
		Dictionary<int, long> dicSortScore = new Dictionary<int, long>();
		heroList.Sort(delegate(CachedHeroInfo lhs, CachedHeroInfo rhs)
		{
			long num = 0L;
			long num2 = 0L;
			int heroIndex = lhs.HeroIndex;
			int heroIndex2 = rhs.HeroIndex;
			if (dicSortScore.ContainsKey(heroIndex))
			{
				num = dicSortScore[heroIndex];
			}
			else
			{
				num = StatCalculator.GetCreatureWavePositionScore(lhs);
				dicSortScore.Add(heroIndex, num);
			}
			if (dicSortScore.ContainsKey(heroIndex2))
			{
				num2 = dicSortScore[heroIndex2];
			}
			else
			{
				num2 = StatCalculator.GetCreatureWavePositionScore(rhs);
				dicSortScore.Add(heroIndex2, num2);
			}
			return num2.CompareTo(num);
		});
		dicSortScore.Clear();
		dicSortScore = null;
	}

	private List<CachedHeroInfo> GetEnemeyHeroList()
	{
		if (_dataProvider == null)
		{
			return null;
		}
		return _dataProvider.GetEnemyHeroList();
	}

	protected override void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		base.SetSquardHeroList(out mainHeroList, out subHeroList);
		SetEnemySquard();
	}

	protected override void SetUserHeroList()
	{
		base.SetUserHeroList();
		ResetDeadHeros();
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (!heroPortrait.IsControllerable || _currentSquard == null)
		{
			return;
		}
		if (heroPortrait.IsBanned)
		{
			ShowEnterClassBanMessage();
		}
		else
		{
			if (heroPortrait.IsDispatched)
			{
				return;
			}
			if (heroPortrait.HeroInfo == null)
			{
				VespaDebug.LogError("HeroInfo is null!!!");
				return;
			}
			int heroIndex = heroPortrait.HeroInfo.HeroIndex;
			long ownerId = heroPortrait.OwnerId;
			if (OrdealHelper.SprkSyncDeadPortrait(heroPortrait))
			{
				int resurrectionHeroGemCount = OrdealHelper.GetResurrectionHeroGemCount();
				long num = OrdealHelper.SprkTotalGem(GlobalAutoBehaviour<AccountManager>.instance.UserInfo);
				int num2 = resurrectionHeroGemCount;
				if (num >= num2)
				{
					string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_REVIVEHERO_TITLE");
					string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_REVIVEHERO_DESC", resurrectionHeroGemCount);
					string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
					PreserveMessagePopup2.Open(stringDirect, stringDirect2, yes, PreserveMessagePopup2.EPriceType.Gem, resurrectionHeroGemCount, delegate
					{
						GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestResurrectionHero(heroIndex);
					});
				}
				else
				{
					string text = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup3Button.Open(text, message, null, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
				}
				return;
			}
			Common.HeroPortraitAnim squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, 0L);
			if (squardHeroProtraitAnim != null && Vespa_Common_Class_Extension_Method.EQUALS(squardHeroProtraitAnim.Component, heroPortrait))
			{
				RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
				return;
			}
			squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, ownerId);
			if (squardHeroProtraitAnim != null)
			{
				RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
				return;
			}
			if (!CanBanClassEnter(heroPortrait))
			{
				ShowEnterClassBanMessage();
				return;
			}
			squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, 0L);
			if (squardHeroProtraitAnim != null)
			{
				CenterMessage.Open(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			}
			else if (CheckMainSquardSelectableClassTag(heroPortrait))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("NOT_REQ_HEROES"));
			}
			else if (!IsAcceptedClassTagType(heroPortrait))
			{
				string text2 = _acceptedClassStringBuilder.ToString();
				CenterMessage.Open(LocalizationManager.GetString(GodkingTrial.NOTICE_ACCEPTED_CLASS, text2));
			}
			else if (IsBannedHero(heroPortrait))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("NOT_REQ_HEROES"));
			}
			else
			{
				bool isMainSquard = _currentSquard.IsSelectedMainSquard();
				InsertSquardHero(heroPortrait, isMainSquard);
				RefreshMainSquardClassTag();
				UpdateBanHeroPortrait();
			}
		}
	}

	public void ResetDeadHeros()
	{
		if (UserHeroListPanel == null)
		{
			return;
		}
		_cachedDeadHeroIndices = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetDeadHeroInidices();
		string remainText = SceneAutoBehaviour<OrdealArenaManagement>.instance.GetHeroRemainResetTimeText();
		UserHeroListPanel.ForeachAction(delegate(HeroPortraitComponent comp)
		{
			if (!(comp == null))
			{
				CachedHeroInfo heroInfo = comp.HeroInfo;
				if (heroInfo != null)
				{
					bool dead = _dataProvider.IsDeadHero(heroInfo.HeroIndex);
					int resurrectionHeroGemCount = OrdealHelper.GetResurrectionHeroGemCount();
					comp.SetDead(dead);
					comp.SetReviveGemCount(resurrectionHeroGemCount);
					comp.SetResetRemainTime(remainText);
				}
			}
		});
	}

	public override void OnHeroPositionToggleChange()
	{
		base.OnHeroPositionToggleChange();
		ResetDeadHeros();
	}

	public void ResetDeadHero(int heroIndex)
	{
		if (UserHeroListPanel != null)
		{
			_cachedDeadHeroIndices = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetDeadHeroInidices();
			long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			UserHeroListPanel.UpdateAction(id, heroIndex, delegate(HeroPortraitComponent comp)
			{
				comp.SetDead(isDead: false);
			});
		}
	}

	public void UpdateDeadHeros()
	{
		ResetDeadHeros();
		if (UserHeroListPanel == null || __Extension_Method_ICollection__.IsNullOrEmpty(_cachedDeadHeroIndices))
		{
			return;
		}
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		int num = __Extension_Method_ICollection__.COUNT(_cachedDeadHeroIndices);
		string remainText = SceneAutoBehaviour<OrdealArenaManagement>.instance.GetHeroRemainResetTimeText();
		for (int i = 0; i < num; i++)
		{
			int heroIndex = _cachedDeadHeroIndices[i];
			UserHeroListPanel.UpdateAction(id, heroIndex, delegate(HeroPortraitComponent comp)
			{
				comp.SetResetRemainTime(remainText);
			});
		}
	}
}

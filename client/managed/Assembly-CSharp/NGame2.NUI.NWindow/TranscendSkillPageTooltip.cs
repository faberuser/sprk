using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TranscendSkillPageTooltip : SimpleTooltip<TranscendSkillPageTooltipInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/TranscendSkillPageTooltip";

	private static WindowPrefab<TranscendSkillPageTooltip> _instance = new WindowPrefab<TranscendSkillPageTooltip>("Resources/Prefab2/Window/Tooltip/TranscendSkillPageTooltip", GameUIRoot.ELayerType.UI);

	public UISimpleGrid SimpleGridTranscend1;

	public UISimpleGrid SimpleGridTranscend2;

	public UISimpleGrid SimpleGridTranscend34;

	public UISimpleGrid SimpleGridTranscend5;

	public List<HeroTranscendSkillIconComponent> _listComponents = new List<HeroTranscendSkillIconComponent>();

	public static TranscendSkillPageTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, TranscendSkillPageTooltipInfo info)
	{
		SimpleTooltip<TranscendSkillPageTooltipInfo>.Open(_instance, ownerWidget, info);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<TranscendSkillPageTooltipInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_listComponents);
	}

	protected override void SetupText(TranscendSkillPageTooltipInfo textInfo)
	{
		base.SetupText(textInfo);
		int heroIndex = textInfo.HeroIndex;
		if (heroIndex == 0)
		{
			return;
		}
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data != null)
			{
				int transcnedSkillPage = textInfo.TranscnedSkillPage;
				CreatueTrascendSkillGroup(SimpleGridTranscend1, 1, transcnedSkillPage, heroInfo, data);
				CreatueTrascendSkillGroup(SimpleGridTranscend2, 2, transcnedSkillPage, heroInfo, data);
				CreatueTrascendSkillGroup(SimpleGridTranscend34, 3, transcnedSkillPage, heroInfo, data);
				CreatueTrascendSkillGroup(SimpleGridTranscend5, 5, transcnedSkillPage, heroInfo, data);
			}
		}
	}

	private void CreatueTrascendSkillGroup(IGrid grid, int transcended, int transcendSkillPage, CachedHeroInfo cachedHeroInfo, CreatureData creatureData)
	{
		if (grid == null || creatureData == null)
		{
			return;
		}
		int[] transcendSkill = creatureData.GetTranscendSkill(transcended);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(transcendSkill))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(transcendSkill); i++)
		{
			HeroTranscendSkillIconComponent heroTranscendSkillIconComponent = UIHelper.AddGridItem(grid, HeroTranscendSkillIconComponent.PrefabAuto);
			CreatureData.TranscendSkill transcendSkillData = creatureData.GetTranscendSkillData(transcended, i);
			if (transcendSkillData == null)
			{
				continue;
			}
			heroTranscendSkillIconComponent.Open(cachedHeroInfo, transcendSkillPage, transcendSkillData, null, readOnly: true);
			_listComponents.Add(heroTranscendSkillIconComponent);
			if (IsLockIconCreate(transcended, i))
			{
				HeroTranscendLockIconComponent heroTranscendLockIconComponent = UIHelper.AddGridItem(grid, HeroTranscendLockIconComponent.PrefabAuto);
				if (heroTranscendLockIconComponent != null)
				{
					_listComponents.Add(heroTranscendLockIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	private bool IsLockIconCreate(int transcended, int index)
	{
		string key = $"TranscendLockIcon{transcended}_{index + 1}";
		return GameRoot.Instance.ConstantDataContainer.GetValueBool(key);
	}
}

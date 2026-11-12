using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildArenaSkillSelectComponent : LinkFactoryComponent, InitOnCreate, HeroSkillIconComponent.Listener
{
	public interface Listener
	{
		void OnSelectSkillComponent(int deckIndex, int slotIndex, int skillSlotNum);
	}

	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaSkillSelectComponent";

	public static PrefabAuto<GuildArenaSkillSelectComponent> PrefabAuto = new PrefabAuto<GuildArenaSkillSelectComponent>("Prefab2/Component/GuildArena/GuildArenaSkillSelectComponent");

	public UISimpleGrid Grid_HeroGrid;

	public UISimpleGrid Grid_SkillGrid;

	public UIToggle[] ToggleList;

	private Listener _listener;

	private CachedHeroInfo _cachedHeroInfo;

	private List<CachedHeroInfo> CachedHeroInfos = new List<CachedHeroInfo>();

	private List<HeroPortraitComponent> PortraitComponents = new List<HeroPortraitComponent>();

	private List<HeroSkillIconComponent> SkillIconComponents = new List<HeroSkillIconComponent>();

	private int _selectedSkillIndex;

	private int _deckIndex;

	private int _slotIndex;

	public void Init(Listener listener, int deckIndex, int slotIndex, CachedHeroInfo cachedHeroInfo, int selectedSkillIndex)
	{
		_listener = listener;
		_cachedHeroInfo = cachedHeroInfo;
		_deckIndex = deckIndex;
		_slotIndex = slotIndex;
		_selectedSkillIndex = selectedSkillIndex;
		CreateHeroIcon();
		InitSkillIcons();
	}

	private void CreateHeroIcon()
	{
		ClearHeorIcon();
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(Grid_HeroGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(_cachedHeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		heroPortraitComponent.gameObject.SetActive(value: true);
		PortraitComponents.Add(heroPortraitComponent);
		CachedHeroInfos.Add(_cachedHeroInfo);
		UIHelper.RepositionGrid(Grid_HeroGrid);
	}

	private void ClearHeorIcon()
	{
		FactoryBase.ReleaseAndClearList(PortraitComponents);
		CachedHeroInfos.Clear();
	}

	private void InitSkillIcons()
	{
		ClearSkillIcons();
		for (int i = 1; i < 4; i++)
		{
			HeroSkillIconComponent heroSkillIconComponent = UIHelper.AddGridItem(Grid_SkillGrid, HeroSkillIconComponent.PrefabAuto);
			if (_cachedHeroInfo != null)
			{
				heroSkillIconComponent.Open(_cachedHeroInfo, i, this);
			}
			else
			{
				heroSkillIconComponent.SetEmptySlot(enable: true);
			}
			heroSkillIconComponent.SetSelected(_selectedSkillIndex == i);
			SkillIconComponents.Add(heroSkillIconComponent);
		}
		UIHelper.RepositionGrid(Grid_SkillGrid);
	}

	private void ClearSkillIcons()
	{
		FactoryBase.ReleaseAndClearList(SkillIconComponents);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearHeorIcon();
		ClearSkillIcons();
	}

	void HeroSkillIconComponent.Listener.OnSelectSkillComponent(HeroSkillIconComponent comp)
	{
		if (comp.HeroInfo == null || comp.IsEmptySlot())
		{
			return;
		}
		foreach (HeroSkillIconComponent skillIconComponent in SkillIconComponents)
		{
			if (skillIconComponent == comp)
			{
				skillIconComponent.SetSelected(!skillIconComponent.IsChecked());
				_selectedSkillIndex = skillIconComponent.SkillSlotIndex;
				if (!skillIconComponent.IsChecked())
				{
					_selectedSkillIndex = 0;
				}
				if (_listener != null)
				{
					_listener.OnSelectSkillComponent(_deckIndex, _slotIndex, _selectedSkillIndex);
				}
			}
			else
			{
				skillIconComponent.SetSelected(select: false);
			}
		}
	}
}

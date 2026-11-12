using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class GuildArenaDeckDetailPopup : BaseWindow, HeroEquipListComponent.Listener
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaDeckDetailPopup";

	private Listener _listener;

	public UILabel Desc;

	public UIScrollGrid HeroEquipItemGrid;

	private CachedHeroInfo[] _heroInfos;

	protected List<HeroEquipListComponent> _heroEquipListComponentList = new List<HeroEquipListComponent>();

	public UILabel Label_DeckName;

	public UILabel Label_DescNotSet;

	private string _titleName;

	public override void Close()
	{
		base.Close();
	}

	public void Open(Listener listener, CachedHeroInfo[] heroInfos, string titleName)
	{
		base.Open();
		_listener = listener;
		_titleName = titleName;
		_heroInfos = heroInfos;
		Init();
	}

	public void Init()
	{
		InitLabels();
		InitHeroEquipList();
	}

	private void InitLabels()
	{
		UIHelper.SetLabelText(Label_DeckName, _titleName);
		UIHelper.EnableGameObject(Label_DescNotSet, _heroInfos.Length == 0);
	}

	private void InitHeroEquipList()
	{
		if (!(HeroEquipItemGrid == null))
		{
			FactoryBase.ReleaseAndClearList(_heroEquipListComponentList);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroInfos); i++)
			{
				CachedHeroInfo heroInfo = _heroInfos[i];
				CreateHeroEquipListComponent(heroInfo);
			}
			UIHelper.RepositionGrid(HeroEquipItemGrid);
		}
	}

	private HeroEquipListComponent CreateHeroEquipListComponent(CachedHeroInfo heroInfo)
	{
		HeroEquipListComponent heroEquipListComponent = UIHelper.AddGridItem(HeroEquipItemGrid, HeroEquipListComponent.PrefabAuto);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroEquipListComponent.transform);
		heroEquipListComponent.Open(heroInfo, this);
		heroEquipListComponent.ForEachItemComponent(PostProcessItemComponent);
		_heroEquipListComponentList.Add(heroEquipListComponent);
		return heroEquipListComponent;
	}

	private void PostProcessItemComponent(ItemIconComponent component)
	{
		_ = component.EquipItemInfo;
		bool flag = false;
		component.SetDisabled(!flag);
	}

	void HeroEquipListComponent.Listener.OnClick(HeroPortraitComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClick(ItemIconComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClickThis(int heroIndex)
	{
	}
}

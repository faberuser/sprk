using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroSkillPanel : MonoBehaviour, IHeroPanel, IArchiveHeroSubPanel, HeroSkillSubComponent.Listener
{
	public interface Listener : IArchiveHeroSubPanelListener
	{
		void OnOpenArchiveSkillDesc(int skillSlotIndex, int linkedIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroSkillPanel";

	public static PrefabAuto<ArchiveHeroSkillPanel> PrefabAuto = new PrefabAuto<ArchiveHeroSkillPanel>("Resources/Prefab2/Component/Hero/ArchiveHeroSkillPanel");

	public UIScrollGrid SkillListGrid;

	public UITable Table;

	public UIWidget RootWidget;

	public UIScrollView ScrollView;

	private List<Component> _componentList = new List<Component>();

	[HideInInspector]
	public CachedHeroInfo CachedHeroInfo;

	private Listener _listener;

	private void Init(CachedHeroInfo cachedHeroInfo, Listener listener)
	{
		_listener = listener;
		base.gameObject.SetActive(value: true);
		Reset();
		CachedHeroInfo = cachedHeroInfo;
		SetupSkillSubComponent();
	}

	private void SetupSkillSubComponent()
	{
		for (int i = 0; i < 4; i++)
		{
			HeroSkillSubComponent heroSkillSubComponent = UIHelper.AddGridItem(SkillListGrid, HeroSkillSubComponent.PrefabAuto);
			heroSkillSubComponent.Open(CachedHeroInfo, i + 1, this, isMine: false);
			heroSkillSubComponent.ShowSkillLevel(isShow: false);
			heroSkillSubComponent.ActiveForceSkillGroup();
			_componentList.Add(heroSkillSubComponent);
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	private void Reset()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void TableReposition()
	{
		if (!(Table == null))
		{
			CalcBounds();
			Table.Reposition();
			Table.repositionNow = true;
		}
	}

	void HeroSkillSubComponent.Listener.OnSelectSkillComponent(HeroSkillSubComponent This)
	{
		if (_listener != null)
		{
			_listener.OnOpenArchiveSkillDesc(This.SkillSlotIndex, This.LinkedIndex);
		}
	}

	void HeroSkillSubComponent.Listener.OnClickSwitchLinkedSkill(HeroSkillSubComponent This)
	{
		This.ActiveForceSkillGroup();
	}

	public void OnExpand()
	{
		TableReposition();
	}

	private void CalcBounds()
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = Table.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		RootWidget.width = (int)bounds.size.x;
		RootWidget.height = (int)bounds.size.y;
		RootWidget.ForceUpdateAnchors(includeChildren: true);
	}

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Init(cachedHeroInfo, listener as Listener);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Init(cachedHeroInfo, listener as Listener);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}
}

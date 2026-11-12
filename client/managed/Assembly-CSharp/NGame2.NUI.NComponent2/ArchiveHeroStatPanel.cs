using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroStatPanel : HeroStatPanel, IArchiveHeroSubPanel
{
	public new interface Listener
	{
	}

	public new const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroStatPanel";

	public new static PrefabAuto<HeroStatPanel> PrefabAuto = new PrefabAuto<HeroStatPanel>("Resources/Prefab2/Component/Hero/ArchiveHeroStatPanel");

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Open(cachedHeroInfo, null);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Open(cachedHeroInfo, null);
	}

	protected override void SetupAwakeMaterial()
	{
		UIHelper.EnableGameObject(WidgetAwakeTranscendInfo, enable: false);
		HeroAttributeResizeGrid.ExtraSize.y = 80f;
		HeroAttributeResizeGrid.Refresh();
		UIHelper.RepositionGrid(HeroAttributeGrid);
		Vector3 localPosition = HeroAttributeGrid.transform.localPosition;
		localPosition.y = -88f;
		HeroAttributeGrid.transform.localPosition = localPosition;
		UIHelper.EnableGameObject(Label_BaseInfo, enable: true);
	}
}

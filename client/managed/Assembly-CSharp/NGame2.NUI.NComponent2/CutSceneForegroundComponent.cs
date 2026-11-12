using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CutSceneForegroundComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/CutScene/CutSceneForegroundComponent";

	public static PrefabAuto<CutSceneForegroundComponent> PrefabAuto = new PrefabAuto<CutSceneForegroundComponent>("Resources/Prefab2/Component/CutScene/CutSceneForegroundComponent");

	public UIBundleLoader UIBundleLoader;

	public UIWidget UIWidget;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(Transform rootTr, string decorateWidget, string assetPath)
	{
		base.gameObject.name = decorateWidget;
		if (!(UIBundleLoader == null))
		{
			UIBundleLoader.SetAssetPath(assetPath);
			if (!(UIWidget == null))
			{
				UIWidget.leftAnchor.target = rootTr;
				UIWidget.rightAnchor.target = rootTr;
			}
		}
	}
}

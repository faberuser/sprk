using NGame2.NUI.NComponent2.NItemIconSub;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ClassBuff;

public class ClassBuffIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/ClassBuff/ClassBuffIconComponent";

	public static PrefabAuto<ClassBuffIconComponent> PrefabAuto = new PrefabAuto<ClassBuffIconComponent>("Prefab2/Component/ClassBuff/ClassBuffIconComponent");

	public UILabel LevelLabel;

	public UIGame3DWidget LevelUpEffect3DWidget;

	public Transform ItemBaseParentTransform;

	public void Open(ClassBuffData data)
	{
		SetInfo(data);
	}

	public void SetInfo(ClassBuffData data)
	{
		SetLevel(data.ClassBuffLevel);
		SetItemBase(data);
	}

	private void SetLevel(int level)
	{
		if (!(LevelLabel == null))
		{
			UIHelper.SetLabelText(LevelLabel, (level > 0) ? ("Lv. " + level) : "");
		}
	}

	private void SetItemBase(ClassBuffData data)
	{
		if (!(ItemBaseParentTransform == null))
		{
			ItemBaseSubComponent itemBaseSubComponent = null;
			itemBaseSubComponent = ((ItemBaseParentTransform.childCount <= 0) ? ItemBaseSubComponent.PrefabAuto.Factory.Create(ItemBaseParentTransform) : ItemBaseParentTransform.GetComponentInChildren<ItemBaseSubComponent>());
			if (!(itemBaseSubComponent == null))
			{
				itemBaseSubComponent.SetInfo(data.ClassBuffIconPath, data.Grade);
			}
		}
	}

	public void PlayLevelUpEffect()
	{
		if (!(LevelUpEffect3DWidget == null))
		{
			if (LevelUpEffect3DWidget.IsPlaying)
			{
				LevelUpEffect3DWidget.Stop(immediatelyClear: true);
			}
			LevelUpEffect3DWidget.Play(null);
		}
	}
}

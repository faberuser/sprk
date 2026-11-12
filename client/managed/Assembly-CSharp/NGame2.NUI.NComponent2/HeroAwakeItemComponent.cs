using System;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroAwakeItemComponent : AwakeChallengeItemComponent
{
	public new const string PrefabPath = "Prefab2/Component/Hero/HeroAwakeItemComponent";

	public new static PrefabAuto<HeroAwakeItemComponent> PrefabAuto = new PrefabAuto<HeroAwakeItemComponent>("Prefab2/Component/Hero/HeroAwakeItemComponent");

	public void Open(ItemData itemData, int ReqCount, int CurCount, bool canStartAwake, HeroAwakeType awakeType, Action<ItemIconComponent> onClick = null)
	{
		Action<ItemIconComponent> onClick2 = null;
		Open(itemData, ReqCount, CurCount, onClick2);
		UIHelper.EnableGameObject(Label_Count, canStartAwake);
		if (!canStartAwake)
		{
			UIHelper.EnableGameObject(Sprite_New, canStartAwake);
		}
		if (awakeType == HeroAwakeType.Transcend || awakeType == HeroAwakeType.LimitBreak)
		{
			SetListener(onClick);
		}
		else
		{
			SetListener(null);
		}
	}

	public void ShowCountLabel(bool show)
	{
		UIHelper.EnableGameObject(Label_Count, show);
	}
}

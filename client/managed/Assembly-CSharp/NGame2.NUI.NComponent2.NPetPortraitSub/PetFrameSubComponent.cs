using System;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class PetFrameSubComponent : BaseSubComponent<PetPortraitComponent>
{
	[Serializable]
	public class PetFrameInfo
	{
		public PetPortraitComponent.EPortraitMode EPortraitMode;

		public UISprite Sprite_Frame;
	}

	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/PetFrameSubComponent";

	public static PrefabAuto<PetFrameSubComponent> PrefabAuto = new PrefabAuto<PetFrameSubComponent>(PrefabPath);

	public PetFrameInfo[] PetFrameInfos;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.Frame));
	}

	public void SetFrame(PetPortraitComponent.EPortraitMode ePortraitMode)
	{
		for (int i = 0; i < PetFrameInfos.Length; i++)
		{
			UIHelper.EnableGameObject(PetFrameInfos[i].Sprite_Frame, PetFrameInfos[i].EPortraitMode == ePortraitMode);
		}
	}
}

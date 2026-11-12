using NGame2.N3D;
using NGame2.NUI.NRender3D;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class Hero3DViewTexture : MonoBehaviour, IInspectorGuiDrawer
{
	public interface Listener
	{
		void OnClick();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/Hero3DViewTexture";

	public static PrefabAuto<Hero3DViewTexture> PrefabAuto = new PrefabAuto<Hero3DViewTexture>("Resources/Prefab2/Component/Hero/Hero3DViewTexture");

	public UITexture HeroPreviewTexture;

	public CachedHeroInfo CachedHeroInfo;

	private Listener _listener;

	private HeroPreview _heroPreview;

	public void Init(Listener listener, CachedHeroInfo cachedHeroInfo)
	{
		base.gameObject.SetActive(value: true);
		CachedHeroInfo = cachedHeroInfo;
		_listener = listener;
		InitPreview();
	}

	public void Reset()
	{
		ClosePreview();
	}

	private void InitPreview()
	{
		if (!(HeroPreviewTexture == null) && CachedHeroInfo != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, CachedHeroInfo);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, CachedHeroInfo.HeroIndex, CachedHeroInfo.CostumeIndex, renderFacadeBuilder, "Customizing", perspective: true);
		}
	}

	private void ClosePreview()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}
}

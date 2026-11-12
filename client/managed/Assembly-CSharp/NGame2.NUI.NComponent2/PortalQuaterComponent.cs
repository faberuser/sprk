using System.Collections;
using NGame2.NUI.NWindow;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalQuaterComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalQuaterComponent";

	public static PrefabAuto<PortalQuaterComponent> PrefabAuto = new PrefabAuto<PortalQuaterComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalQuaterComponent");

	[Header("[PortalQuaterComponent]")]
	public UIScrollView DescScrollView;

	[HideInInspector]
	public bool Press = true;

	[HideInInspector]
	public bool Release;

	protected override void Init()
	{
		base.Init();
	}

	public override void ResolutionComp(float contentsWidth)
	{
		base.ResolutionComp(contentsWidth);
		VespaCoroutine.Start(coResolutionComp(), null, null);
	}

	private IEnumerator coResolutionComp()
	{
		if (!String_Extension_Method.IsNullOrEmpty(base._portalRenewalData.DescLocalKey))
		{
			UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: false);
			yield return null;
			if (DescScrollView != null && _basicData.ContentsDescriptionLabel != null)
			{
				float width = DescScrollView.panel.width;
				_basicData.ContentsDescriptionLabel.width = (int)width;
				Vector2 vector = new Vector2
				{
					x = DescScrollView.transform.localPosition.x + DescScrollView.panel.baseClipRegion.x,
					y = DescScrollView.transform.localPosition.y + DescScrollView.panel.baseClipRegion.y
				};
				DescScrollView.transform.localPosition = vector;
			}
			DescScrollView.movement = UIScrollView.Movement.Unrestricted;
			UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, base.LockedState == PortalRenewal.LockedState.NONE);
			yield return null;
			DescScrollView.ResetPosition();
			yield return null;
			DescScrollView.movement = UIScrollView.Movement.Vertical;
		}
	}

	protected override void UpdateResetTime()
	{
	}
}

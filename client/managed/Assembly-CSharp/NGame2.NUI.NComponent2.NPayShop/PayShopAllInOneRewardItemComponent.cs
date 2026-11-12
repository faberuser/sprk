using System.Collections;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopAllInOneRewardItemComponent : ItemIconComponent
{
	public new const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopAllInOneRewardItemComponent";

	public new static PrefabAuto<PayShopAllInOneRewardItemComponent> PrefabAuto = new PrefabAuto<PayShopAllInOneRewardItemComponent>("Resources/Prefab2/Component/PayShop2/PayShopAllInOneRewardItemComponent");

	public GameObject rewardGet;

	public UILabel rewardLabel;

	public UIGame3DWidget UI3DWidgetGet;

	public UIGame3DWidget UI3DWidgetReset;

	private ECompState state;

	public void InitRewardItemIcon(ItemData itemData)
	{
		Open(itemData);
	}

	public void Refresh(ECompState state)
	{
		if (this.state != ECompState.None && this.state != ECompState.Get)
		{
			UI3DWidgetPlay(UI3DWidgetGet, state == ECompState.Get);
		}
		UIHelper.EnableGameObject(rewardGet, state == ECompState.Get);
		this.state = state;
	}

	public IEnumerator ResetItemComponent()
	{
		UI3DWidgetPlay(UI3DWidgetReset, isPlay: true);
		yield return new WaitForSeconds(0.2f);
		UIHelper.EnableGameObject(rewardGet, enable: false);
	}

	private void UI3DWidgetPlay(UIGame3DWidget widget, bool isPlay)
	{
		if (widget == null)
		{
			return;
		}
		if (isPlay)
		{
			if (widget.IsPlaying)
			{
				widget.Stop(immediatelyClear: false);
			}
			widget.SetDirty();
			widget.Play(null);
		}
		else
		{
			widget.Stop(immediatelyClear: false);
		}
	}
}

using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSelectRewardPopup;

[Serializable]
public class SelectRewardItemListInfo
{
	public UISimpleGrid RewardItemGrid;

	[HideInInspector]
	public List<ItemIconComponent> RewardItemIconComponentList = new List<ItemIconComponent>();

	public bool CheckIsNull
	{
		get
		{
			if (RewardItemGrid == null)
			{
				return true;
			}
			return RewardItemIconComponentList == null;
		}
	}
}

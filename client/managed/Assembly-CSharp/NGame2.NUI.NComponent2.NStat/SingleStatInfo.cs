using System;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class SingleStatInfo : MonoBehaviour, IStatInfo
{
	public enum StatIconType
	{
		Normal,
		Renew
	}

	[Serializable]
	public class StatIconInfo
	{
		public StatIconType Type;

		public Transform Root;
	}

	public UILabel NameLabel;

	public UILabel ValueLabel;

	public UISimpleGrid RuneGrid;

	public StatIconInfo[] StatIcon;

	public int HeightSize;

	public void SelectIcon(StatIconType iconType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StatIcon); i++)
		{
			UIHelper.EnableGameObject(StatIcon[i].Root, StatIcon[i].Type == iconType);
		}
	}

	public void DisableAllIcon()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StatIcon); i++)
		{
			UIHelper.EnableGameObject(StatIcon[i].Root, enable: false);
		}
	}

	int IStatInfo.GetHeight()
	{
		if (HeightSize <= 0)
		{
			if (!NameLabel)
			{
				return 0;
			}
			return NameLabel.height;
		}
		return HeightSize;
	}
}

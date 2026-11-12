using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatChangeInfo : MonoBehaviour, IStatInfo
{
	public UILabel NameLabel;

	public UILabel BeforeValueLabel;

	public UILabel AfterValueLabel;

	int IStatInfo.GetHeight()
	{
		if (!NameLabel)
		{
			if (!BeforeValueLabel)
			{
				if (!AfterValueLabel)
				{
					return 0;
				}
				return AfterValueLabel.height;
			}
			return BeforeValueLabel.height;
		}
		return NameLabel.height;
	}
}

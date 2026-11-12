using NShared;
using UnityEngine;

namespace NGame2.NUI;

public class LinkHelpButton : MonoBehaviour
{
	public UIButton ButtonHelp;

	public ManualCategory Category;

	public ManualSubCode Code;

	private void Start()
	{
		if (ButtonHelp != null && __Extension_Method_ICollection__.IsNullOrEmpty(ButtonHelp.onClick))
		{
			ButtonHelp.onClick.Add(new EventDelegate(delegate
			{
			}));
		}
	}
}

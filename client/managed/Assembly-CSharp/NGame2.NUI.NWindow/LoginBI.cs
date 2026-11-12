using NGame2.NUI.NManager;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginBI : MonoBehaviour
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginBI";

	public bool ForcePlay;

	public UIGame3DWidgetFromRes BIEffectOnce;

	public static bool IsLoginBINull { get; set; }

	public void Play()
	{
		if (BIEffectOnce == null)
		{
			IsLoginBINull = true;
			return;
		}
		if (ForcePlay)
		{
			LoginManagement.BIOncePlayed.Value = false;
		}
		if (!LoginManagement.BIOncePlayed.Value)
		{
			BIEffectOnce.Play(delegate
			{
				LoginManagement.BIOncePlayed.Value = true;
			});
		}
	}
}

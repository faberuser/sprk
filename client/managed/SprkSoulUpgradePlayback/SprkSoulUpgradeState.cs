using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using UnityEngine;

public sealed class SprkSoulUpgradeState : MonoBehaviour
{
	public readonly SprkSoulUpgradeSession Session = new SprkSoulUpgradeSession();

	public SoulWeaponUpgradeResultWindow Window;

	public Animator Animation;

	public AnimatorUpdateMode SavedMode;

	public float SavedSpeed;

	public float Started;

	public void Finish()
	{
		if (Session.Active)
		{
			Session.End();
			if (Animation != null)
			{
				Animation.updateMode = SavedMode;
				Animation.speed = SavedSpeed;
			}
			if (Window != null && GlobalTopmostWindow.IsValidInstance)
			{
				GlobalTopmostWindow.instance.RemoveTouchDisabler(Window);
			}
			Debug.Log("SPRK Soul Weapon advancement: result released; elapsed=" + (Time.realtimeSinceStartup - Started) + " scale=" + Time.timeScale);
			Animation = null;
		}
	}

	private void OnDisable()
	{
		Finish();
	}

	private void OnDestroy()
	{
		Finish();
	}
}

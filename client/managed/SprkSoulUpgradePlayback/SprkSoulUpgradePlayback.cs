using System.Collections;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NSoulWeapon;
using NVespa;
using UnityEngine;

public static class SprkSoulUpgradePlayback
{
	public static void Begin(SoulWeaponUpgradeResultWindow window)
	{
		SprkSoulUpgradeState sprkSoulUpgradeState = window.GetComponent<SprkSoulUpgradeState>();
		if (sprkSoulUpgradeState == null)
		{
			sprkSoulUpgradeState = window.gameObject.AddComponent<SprkSoulUpgradeState>();
		}
		sprkSoulUpgradeState.Finish();
		sprkSoulUpgradeState.Window = window;
		sprkSoulUpgradeState.Session.Begin();
		sprkSoulUpgradeState.Started = Time.realtimeSinceStartup;
		if (window.BgNodeTreeUIPlayer != null)
		{
			window.BgNodeTreeUIPlayer.Close();
		}
		sprkSoulUpgradeState.Animation = window.Animator;
		if (sprkSoulUpgradeState.Animation != null)
		{
			sprkSoulUpgradeState.SavedMode = sprkSoulUpgradeState.Animation.updateMode;
			sprkSoulUpgradeState.SavedSpeed = sprkSoulUpgradeState.Animation.speed;
			sprkSoulUpgradeState.Animation.updateMode = AnimatorUpdateMode.UnscaledTime;
			sprkSoulUpgradeState.Animation.speed = 1f;
		}
		Debug.Log("SPRK Soul Weapon advancement: result opened; scale=" + Time.timeScale + " unscaledDelta=" + Time.unscaledDeltaTime);
	}

	public static IEnumerator Guard(IEnumerator original, SoulWeaponUpgradeResultWindow window)
	{
		SprkSoulUpgradeState component = window.GetComponent<SprkSoulUpgradeState>();
		return component.Session.Guard(original, component.Session.Generation, () => window != null && window.IsOpen() && window.gameObject.activeInHierarchy);
	}

	public static void End(SoulWeaponUpgradeResultWindow window)
	{
		SprkSoulUpgradeState component = window.GetComponent<SprkSoulUpgradeState>();
		if (component != null)
		{
			component.Finish();
		}
	}

	public static ILocalTimer Timer(ILocalTimer original, RenderTargetCinema cinema)
	{
		if (cinema != null && SprkSoulUpgradeSession.IsUpgrade(cinema.Stock.NodeTreePackCode, cinema.Stock.NodeTreeName))
		{
			return GlobalTimer.UnscaledLocal;
		}
		return original;
	}
}

using NGame2.NAccount;
using NGame2.NUI;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkProfilePortraitState : MonoBehaviour
{
	public UIBundleLoader Target;

	public bool FullPortrait;

	private float nextPoll;

	private void OnEnable()
	{
		nextPoll = 0f;
	}

	public void Refresh()
	{
		if (Target == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance.UserManager != null && instance.HeroManager != null)
		{
			int avatarHeroIndex = instance.UserManager.AvatarHeroIndex;
			string text = (FullPortrait ? SprkLobbyAvatar.GetProfilePortraitPath(avatarHeroIndex) : SprkLobbyAvatar.GetProfileIconPath(avatarHeroIndex));
			if (!string.IsNullOrEmpty(text) && Target.AssetPath != text)
			{
				Target.SetAssetPath(text, FullPortrait ? UIHelper.EAssetTextureAssign.ClipCenter : UIHelper.EAssetTextureAssign.__Ignore__);
			}
		}
	}

	private void Update()
	{
		if (!(Time.realtimeSinceStartup < nextPoll))
		{
			nextPoll = Time.realtimeSinceStartup + 0.1f;
			Refresh();
		}
	}
}

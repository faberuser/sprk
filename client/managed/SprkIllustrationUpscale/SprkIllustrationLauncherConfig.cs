using System;

[Serializable]
public sealed class SprkIllustrationLauncherConfig
{
	public string SelectedProfile;

	public string ManifestUrl;

	public SprkIllustrationLauncherProfile[] Profiles;
}

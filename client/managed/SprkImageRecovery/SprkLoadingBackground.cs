using System;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NHelper;
using NGameAsset;
using UnityEngine;

public sealed class SprkLoadingBackground : MonoBehaviour
{
	private Component owner;

	private UITexture target;

	private FieldInfo assetField;

	private AssetTexture asset;

	private string path;

	private float missingSince;

	private float nextPoll;

	private int attempts;

	private bool watching;

	public void Open(Component view, UITexture texture, FieldInfo field)
	{
		owner = view;
		target = texture;
		assetField = field;
		asset = (AssetTexture)field.GetValue(view);
		path = ((asset == null) ? "" : asset.Path);
		missingSince = Time.realtimeSinceStartup;
		nextPoll = 0f;
		attempts = 0;
		watching = true;
		BindOrCover();
	}

	private bool BindOrCover()
	{
		AssetTexture.TextureInfo textureInfo = ((asset == null) ? null : asset.GetTextureInfo());
		if (textureInfo != null && textureInfo.Tex != null)
		{
			if (target.mainTexture != textureInfo.Tex || target.material != textureInfo.Mtrl)
			{
				UIHelper.SetTextureInfo(target, textureInfo);
			}
			return true;
		}
		target.material = null;
		target.mainTexture = Texture2D.blackTexture;
		target.uvRect = new Rect(0f, 0f, 1f, 1f);
		return false;
	}

	private void LateUpdate()
	{
		if (!watching || owner == null || target == null)
		{
			return;
		}
		try
		{
			if (assetField.GetValue(owner) != asset)
			{
				watching = false;
				return;
			}
			if (BindOrCover())
			{
				missingSince = Time.realtimeSinceStartup;
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup < nextPoll) && !string.IsNullOrEmpty(path) && attempts < 3 && !(realtimeSinceStartup - missingSince < ((asset != null && !asset.IsDone) ? 8f : 0.5f)))
			{
				attempts++;
				PlainAssetHolder component = target.GetComponent<PlainAssetHolder>();
				if (component != null)
				{
					component.Close();
				}
				if (asset != null)
				{
					asset.Close();
				}
				SprkImageRecovery.PrepareAtlas(path);
				asset = UIHelper.GetPlainTexture(path);
				assetField.SetValue(owner, asset);
				component = UIHelper.ResetPlaneAssetTextureAsync(owner, target, asset);
				if (component != null)
				{
					component.ReleaseAssetOnClose = true;
				}
				missingSince = realtimeSinceStartup;
				nextPoll = realtimeSinceStartup + (float)attempts * 0.5f;
				BindOrCover();
				if (attempts == 1)
				{
					Debug.LogWarning("[SPRK Images] Retrying loading background: " + path);
				}
			}
		}
		catch (Exception ex)
		{
			watching = false;
			Debug.LogWarning("[SPRK Images] Loading background recovery stopped: " + ex.Message);
		}
	}

	private void OnDisable()
	{
		watching = false;
	}
}

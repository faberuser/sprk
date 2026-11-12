using System;
using System.Collections.Generic;
using NNguiExtension;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIBundleTexSelector : UIBasicSelector, IInspectorGuiDrawer
{
	[Serializable]
	public class SelectData
	{
		public string Name = string.Empty;

		public string AssetPath = string.Empty;

		public Color Color = Color.white;

		public Vector3 Position;

		public int Width;

		public int Height;

		public UIBasicSprite.Flip Flip;
	}

	[SerializeField]
	private UIBundleLoader _targetBundleTex;

	[HideInInspector]
	[SerializeField]
	private bool _applyAssetPath = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyColor = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyPosition = true;

	[HideInInspector]
	[SerializeField]
	private bool _applySize = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyFlip;

	[HideInInspector]
	public List<SelectData> SelectList = new List<SelectData>();

	public UIBundleLoader GetTargetBundleTex()
	{
		if (_targetBundleTex != null)
		{
			return _targetBundleTex;
		}
		return GetComponent<UIBundleLoader>();
	}

	private void Awake()
	{
	}

	public void Show(bool show)
	{
		UIBundleLoader targetBundleTex = GetTargetBundleTex();
		if ((bool)targetBundleTex)
		{
			targetBundleTex.gameObject.SetActive(show);
		}
	}

	public override bool Select(string name)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectList))
		{
			return false;
		}
		_ = SelectList[0];
		for (int i = 0; i < SelectList.Count; i++)
		{
			if (SelectList[i].Name == name)
			{
				SelectByIndex(i);
				return true;
			}
		}
		return false;
	}

	public override void SelectByIndex(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(SelectList, index))
		{
			return;
		}
		SelectData selectData = SelectList[index];
		UIBundleLoader targetBundleTex = GetTargetBundleTex();
		if (_applyAssetPath)
		{
			targetBundleTex.AssetPath = selectData.AssetPath;
		}
		if (_applyColor)
		{
			UIHelper.SetWidgetColor(targetBundleTex.UITEX(), selectData.Color);
		}
		if (_applyPosition)
		{
			targetBundleTex.transform.localPosition = selectData.Position;
		}
		if (_applySize)
		{
			UITexture uITexture = targetBundleTex.UITEX();
			if (uITexture != null)
			{
				uITexture.width = selectData.Width;
				uITexture.height = selectData.Height;
			}
		}
		if (_applyFlip)
		{
			UITexture uITexture2 = targetBundleTex.UITEX();
			if (uITexture2 != null)
			{
				uITexture2.flip = selectData.Flip;
			}
		}
	}
}

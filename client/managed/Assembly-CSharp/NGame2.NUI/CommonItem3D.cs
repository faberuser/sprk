using System.Collections.Generic;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow;
using NVespa;

namespace NGame2.NUI;

public abstract class CommonItem3D : BaseWindow
{
	public UITexture TextureItem;

	public UIWidget Item3DRoot;

	protected List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	protected ItemPreview ItemPreview { get; set; }

	protected Item3DInfo Item3DInfo { get; set; }

	public virtual void Open(int itemIndex)
	{
		Open();
	}

	protected ItemPreview.InfoData GetPreviewFurnitureInfo()
	{
		ItemPreview.InfoData result = new ItemPreview.InfoData
		{
			ItemType = Item3DInfo.ItemType
		};
		SingletonAuto<ResolutionInfo>.instance.CalcRenderTargetSize(TextureItem.width, TextureItem.height, out result.RenderTargetWidth, out result.RenderTargetHeight);
		if (ItemPreview != null)
		{
			ItemPreview.ReleaseInstance(ItemPreview);
			ItemPreview = null;
			TextureItem.mainTexture = null;
		}
		result.extraTransform = new TransformData();
		result.extraTransform.Position = Item3DInfo.Position;
		result.extraTransform.RotationEuler = Item3DInfo.Rotation;
		result.extraTransform.Scale = Item3DInfo.Item3DScale;
		result.Code = Item3DInfo.Item3DModelName;
		return result;
	}

	public override void Close()
	{
		base.Close();
		ItemPreview.ReleaseInstance(ItemPreview);
		ItemPreview = null;
	}

	public void OnPress()
	{
		OnDragBegin();
	}

	public void OnRelease()
	{
		OnDragEnd();
	}

	protected virtual void OnDragBegin()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
		}
	}

	protected virtual void OnDragEnd()
	{
		_touchList.Remove(UICamera.currentTouch);
	}

	protected abstract void UpdateCamera();
}

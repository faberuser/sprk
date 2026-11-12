using NGame2.NUI.NRender3D;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class Item3DBook : CommonItem3D
{
	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/Item3DBook";

	public UILabel LabelName;

	public float RotateSensitivity = 1f;

	public bool EditMode;

	public float Scale = 1f;

	private float angle;

	public override void Open(int itemIndex)
	{
		base.Open(itemIndex);
	}

	public void Init(ItemData itemData)
	{
		UIHelper.SetLabelText(LabelName, itemData.Name);
		base.Item3DInfo = new Item3DInfo(itemData.Type, itemData.Item3DScale, itemData.Item3DPath, new Vector3(0f, 1.8f, 0f), new Vector3(0f, itemData.Item3DFlip ? 180f : 0f, 0f));
		base.ItemPreview = ItemPreview.CreateInstance(GetPreviewFurnitureInfo());
		TextureItem.mainTexture = base.ItemPreview.RenderTarget;
		angle = Math.Angle(Vespa_Unity_Vector3_Extension_Method.VecXZ(Vector3.forward));
	}

	public override void Close()
	{
		base.Close();
		UIHelper.SetLabelText(LabelName, string.Empty);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateCamera();
	}

	protected override void UpdateCamera()
	{
		if (_touchList.Count != 0 && _touchList.Count == 1)
		{
			UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
			VectorXZ i_v = Math.RotateXZ(angle);
			i_v = Math.Rotate(i_v, mouseOrTouch.delta.x * RotateSensitivity);
			angle = Math.Angle(i_v);
			base.ItemPreview.ItemRoot.transform.localRotation = Quaternion.LookRotation(new Vector3(i_v.x, 0f, i_v.z), Vector3.up);
		}
	}
}

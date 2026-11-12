using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
public class UIClipSprite : UISprite
{
	public enum EClipType
	{
		None,
		FillHorizonal,
		FillHorizonalInv,
		FillVertical,
		FillVerticalInv
	}

	[HideInInspector]
	[SerializeField]
	private EClipType _clipType;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _leftClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _rightClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _bottomClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _topClipAmount;

	public EClipType ClipType
	{
		get
		{
			return _clipType;
		}
		set
		{
			if (_clipType != value)
			{
				float clipFillAmount = ClipFillAmount;
				_clipType = value;
				_leftClipAmount = 0f;
				_rightClipAmount = 0f;
				_bottomClipAmount = 0f;
				_topClipAmount = 0f;
				ClipFillAmount = clipFillAmount;
				mChanged = true;
			}
		}
	}

	public override float fillAmount
	{
		get
		{
			if (_clipType != EClipType.None)
			{
				return ClipFillAmount;
			}
			return base.fillAmount;
		}
		set
		{
			if (_clipType == EClipType.None)
			{
				base.fillAmount = value;
				return;
			}
			ClipFillAmount = value;
			base.fillAmount = 1f;
		}
	}

	public float ClipFillAmount
	{
		get
		{
			if (_clipType == EClipType.None)
			{
				return 1f;
			}
			if (_clipType == EClipType.FillHorizonal)
			{
				return 1f - RightClipAmount;
			}
			if (_clipType == EClipType.FillHorizonalInv)
			{
				return 1f - LeftClipAmount;
			}
			if (_clipType == EClipType.FillVertical)
			{
				return 1f - TopClipAmount;
			}
			if (_clipType == EClipType.FillVerticalInv)
			{
				return 1f - BottomClipAmount;
			}
			return 1f;
		}
		set
		{
			if (_clipType != EClipType.None)
			{
				if (_clipType == EClipType.FillHorizonal)
				{
					RightClipAmount = 1f - value;
				}
				else if (_clipType == EClipType.FillHorizonalInv)
				{
					LeftClipAmount = 1f - value;
				}
				else if (_clipType == EClipType.FillVertical)
				{
					TopClipAmount = 1f - value;
				}
				else if (_clipType == EClipType.FillVerticalInv)
				{
					BottomClipAmount = 1f - value;
				}
			}
		}
	}

	public float LeftClipAmount
	{
		get
		{
			return _leftClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_leftClipAmount != num)
			{
				_leftClipAmount = num;
				mChanged = true;
			}
		}
	}

	public float RightClipAmount
	{
		get
		{
			return _rightClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_rightClipAmount != num)
			{
				_rightClipAmount = num;
				mChanged = true;
			}
		}
	}

	public float TopClipAmount
	{
		get
		{
			return _topClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_topClipAmount != num)
			{
				_topClipAmount = num;
				mChanged = true;
			}
		}
	}

	public float BottomClipAmount
	{
		get
		{
			return _bottomClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_bottomClipAmount != num)
			{
				_bottomClipAmount = num;
				mChanged = true;
			}
		}
	}

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (_rightClipAmount + _leftClipAmount >= 1f || _topClipAmount + _bottomClipAmount >= 1f)
		{
			return;
		}
		base.OnFill(verts, uvs, cols);
		if (verts == null || uvs == null || cols == null || verts.size < 4 || uvs.size < 4 || cols.size < 4)
		{
			return;
		}
		Vector3 lhs = verts[0];
		Vector3 lhs2 = verts[1];
		for (int i = 1; i < verts.Count; i++)
		{
			Vector3 rhs = verts[i];
			lhs = Vector3.Min(lhs, rhs);
			lhs2 = Vector3.Max(lhs2, rhs);
		}
		float num = Mathf.Lerp(lhs.x, lhs2.x, _leftClipAmount);
		float num2 = Mathf.Lerp(lhs2.x, lhs.x, _rightClipAmount);
		float num3 = Mathf.Lerp(lhs.y, lhs2.y, _bottomClipAmount);
		float num4 = Mathf.Lerp(lhs2.y, lhs.y, _topClipAmount);
		int num5 = verts.size / 4;
		Vector2 value2 = default(Vector2);
		for (int j = 0; j < num5; j++)
		{
			Vector3 vector = verts[j * 4];
			Vector3 to = verts[j * 4 + 2];
			if (vector.x > num2 || to.x < num || to.y < num3 || vector.y > num4)
			{
				verts[j * 4] = Vector3.zero;
				verts[j * 4 + 1] = Vector3.zero;
				verts[j * 4 + 2] = Vector3.zero;
				verts[j * 4 + 3] = Vector3.zero;
				continue;
			}
			Vector2 vector2 = uvs[j * 4];
			Vector2 vector3 = uvs[j * 4 + 2];
			for (int k = 0; k < 4; k++)
			{
				Vector3 value = verts[j * 4 + k];
				value.x = Mathf.Clamp(value.x, num, num2);
				value.y = Mathf.Clamp(value.y, num3, num4);
				Vector3 vector4 = Math.InverseLerp(vector, to, value);
				value2.x = Mathf.Lerp(vector2.x, vector3.x, vector4.x);
				value2.y = Mathf.Lerp(vector2.y, vector3.y, vector4.y);
				verts[j * 4 + k] = value;
				uvs[j * 4 + k] = value2;
			}
		}
	}
}

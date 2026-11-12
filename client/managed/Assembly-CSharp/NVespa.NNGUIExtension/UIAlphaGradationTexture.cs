using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIAlphaGradationTexture : UITexture, IUIAlphaGradation
{
	[Range(0f, 0.5f)]
	[HideInInspector]
	[SerializeField]
	private float _allClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _topClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _bottomClipAmount = 1f;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _leftClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _rightClipAmount = 1f;

	[ExecuteInEditMode]
	public float AllClipAmount
	{
		get
		{
			return _allClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_allClipAmount != num)
			{
				_allClipAmount = num;
				mChanged = true;
			}
		}
	}

	[ExecuteInEditMode]
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

	[ExecuteInEditMode]
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

	[ExecuteInEditMode]
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

	[ExecuteInEditMode]
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

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		UIAlphaGradationSprite.FillGradation(_topClipAmount, _bottomClipAmount, _leftClipAmount, _rightClipAmount, verts, uvs, cols);
	}
}

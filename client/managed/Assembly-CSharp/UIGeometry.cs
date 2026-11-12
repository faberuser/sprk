using NVespa;
using UnityEngine;

public class UIGeometry
{
	public RenderingList<Vector3> verts = new RenderingList<Vector3>();

	public RenderingList<Vector2> uvs = new RenderingList<Vector2>();

	public RenderingList<Color32> cols = new RenderingList<Color32>();

	private RenderingList<Vector3> mRtpVerts = new RenderingList<Vector3>();

	private Vector3 mRtpNormal;

	private Vector4 mRtpTan;

	public bool hasVertices => verts.Count > 0;

	public bool hasTransformed
	{
		get
		{
			if (mRtpVerts != null && mRtpVerts.Count > 0)
			{
				return mRtpVerts.Count == verts.Count;
			}
			return false;
		}
	}

	public void Clear()
	{
		verts.Clear();
		uvs.Clear();
		cols.Clear();
		mRtpVerts.Clear();
	}

	public void ClearAndRelease()
	{
		verts.ClearAndReleaseBuffer();
		uvs.ClearAndReleaseBuffer();
		cols.ClearAndReleaseBuffer();
		mRtpVerts.ClearAndReleaseBuffer();
	}

	public void ApplyTransform(Matrix4x4Fast widgetToPanel)
	{
		mRtpVerts.Clear();
		if (verts.Count > 0)
		{
			Matrix4x4 value = widgetToPanel.Value;
			mRtpVerts.ReserveCount(verts.Count);
			if (widgetToPanel.FastType == Matrix4x4Fast.EFastType.TranslateOnly)
			{
				ApplyTranslate(widgetToPanel);
			}
			else if (widgetToPanel.FastType == Matrix4x4Fast.EFastType.ScaleOnly)
			{
				ApplyScaleOnly(widgetToPanel);
			}
			else if (widgetToPanel.FastType == Matrix4x4Fast.EFastType.ScaleAndTranslate)
			{
				ApplyScaleAndTranslate(widgetToPanel);
			}
			else if (widgetToPanel.FastType == Matrix4x4Fast.EFastType.None)
			{
				ApplyNone();
			}
			else
			{
				ApplyNormal(value);
			}
		}
	}

	private void ApplyTranslate(Matrix4x4Fast widgetToPanel)
	{
		Vector3 translate = widgetToPanel.Translate;
		int count = verts.Count;
		Vector3[] buffer = verts.buffer;
		int count2 = mRtpVerts.Count;
		mRtpVerts.AddEmpty(count);
		Vector3[] buffer2 = mRtpVerts.buffer;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = buffer[i];
			buffer2[count2 + i] = new Vector3(vector.x + translate.x, vector.y + translate.y, vector.z + translate.z);
		}
		mRtpNormal = Vector3.back;
		mRtpTan = new Vector4(Vector3.right.x, Vector3.right.y, Vector3.right.z, -1f);
	}

	private void ApplyScaleOnly(Matrix4x4Fast widgetToPanel)
	{
		Vector3 scale = widgetToPanel.Scale;
		int count = verts.Count;
		Vector3[] buffer = verts.buffer;
		int count2 = mRtpVerts.Count;
		mRtpVerts.AddEmpty(count);
		Vector3[] buffer2 = mRtpVerts.buffer;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = buffer[i];
			buffer2[count2 + i] = new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
		}
		mRtpNormal = Vector3.back;
		mRtpTan = new Vector4(Vector3.right.x, Vector3.right.y, Vector3.right.z, -1f);
	}

	private void ApplyScaleAndTranslate(Matrix4x4Fast widgetToPanel)
	{
		Vector3 scale = widgetToPanel.Scale;
		Vector3 translate = widgetToPanel.Translate;
		int count = verts.Count;
		Vector3[] buffer = verts.buffer;
		int count2 = mRtpVerts.Count;
		mRtpVerts.AddEmpty(count);
		Vector3[] buffer2 = mRtpVerts.buffer;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = buffer[i];
			buffer2[count2 + i] = new Vector3(vector.x * scale.x + translate.x, vector.y * scale.y + translate.y, vector.z * scale.z + translate.z);
		}
		mRtpNormal = Vector3.back;
		mRtpTan = new Vector4(Vector3.right.x, Vector3.right.y, Vector3.right.z, -1f);
	}

	private void ApplyNone()
	{
		mRtpVerts.AddList(verts);
		mRtpNormal = Vector3.back;
		mRtpTan = new Vector4(Vector3.right.x, Vector3.right.y, Vector3.right.z, -1f);
	}

	private void ApplyNormal(Matrix4x4 matrix)
	{
		for (int i = 0; i < verts.Count; i++)
		{
			mRtpVerts.AddUnsafe(matrix.MultiplyPoint3x4(verts[i]));
		}
		mRtpNormal = matrix.MultiplyVector(Vector3.back).normalized;
		Vector3 normalized = matrix.MultiplyVector(Vector3.right).normalized;
		mRtpTan = new Vector4(normalized.x, normalized.y, normalized.z, -1f);
	}

	public void WriteToBuffers(UIDrawCallData dcData, bool generateNormals)
	{
		if (mRtpVerts != null && mRtpVerts.Count > 0)
		{
			dcData.PendingVerts(mRtpVerts, mRtpVerts.Count);
			dcData.PendingUvs(uvs, mRtpVerts.Count);
			dcData.PendingCols(cols, mRtpVerts.Count);
			if (generateNormals)
			{
				dcData.PendingNormsCount(mRtpNormal, mRtpVerts.Count);
				dcData.PendingTans(mRtpTan, mRtpVerts.Count);
			}
		}
	}
}

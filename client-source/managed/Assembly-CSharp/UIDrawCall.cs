using System;
using System.Collections.Generic;
using NDrawCall;
using NVespa;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	public enum Clipping
	{
		None = 0,
		TextureMask = 1,
		SoftClip = 3,
		ConstrainButDontClip = 4
	}

	public delegate void OnRenderCallback(Material mat);

	public static int MeshCreateCount = 0;

	public static int MeshDestroyCount = 0;

	private static Dictionary<int, UIDrawCall> mActiveDic = new Dictionary<int, UIDrawCall>();

	private static Queue<UIDrawCall> mInactiveQueue = new Queue<UIDrawCall>();

	[NonSerialized]
	[HideInInspector]
	public int widgetCount;

	[NonSerialized]
	[HideInInspector]
	public UIPanel manager;

	[NonSerialized]
	[HideInInspector]
	public UIPanel panel;

	[NonSerialized]
	[HideInInspector]
	public Texture2D clipTexture;

	[NonSerialized]
	[HideInInspector]
	public bool alwaysOnScreen;

	[NonSerialized]
	[HideInInspector]
	public UIDrawCallData dcData = new UIDrawCallData();

	private Material mMaterial;

	private Texture mTexture;

	private Shader mShader;

	private UIRect.ERenderKeywords mRenderKeywords;

	private int mClipCount;

	private Transform mTrans;

	private Mesh mMesh;

	private MeshFilter mFilter;

	private MeshRenderer mRenderer;

	private Material mDynamicMat;

	private int _depthStart = int.MaxValue;

	private int _depthEnd = int.MinValue;

	private MeshIndexBuffer _indexBuffer = new MeshIndexBuffer();

	private bool mRebuildMat = true;

	private bool mLegacyShader;

	private int mRenderQueue = 3000;

	private int mTriangles;

	[NonSerialized]
	private bool mTextureClip;

	public OnRenderCallback onRender;

	private const int maxIndexBufferCache = 10;

	private static int[] ClipRange = null;

	private static int[] ClipArgs = null;

	public static Dictionary<int, UIDrawCall> activeDic => mActiveDic;

	public static Queue<UIDrawCall> inactiveQueue => mInactiveQueue;

	public int depthStart => _depthStart;

	public int depthEnd => _depthEnd;

	public bool isDirty { get; set; }

	public int renderQueue
	{
		get
		{
			return mRenderQueue;
		}
		set
		{
			if (mRenderQueue != value)
			{
				mRenderQueue = value;
				if (mDynamicMat != null)
				{
					mDynamicMat.renderQueue = value;
				}
			}
		}
	}

	public int sortingOrder
	{
		get
		{
			if (!(mRenderer != null))
			{
				return 0;
			}
			return mRenderer.sortingOrder;
		}
		set
		{
			if (mRenderer != null && mRenderer.sortingOrder != value)
			{
				mRenderer.sortingOrder = value;
			}
		}
	}

	public int finalRenderQueue
	{
		get
		{
			if (!(mDynamicMat != null))
			{
				return mRenderQueue;
			}
			return mDynamicMat.renderQueue;
		}
	}

	public Transform cachedTransform
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = base.transform;
			}
			return mTrans;
		}
	}

	public Material baseMaterial
	{
		get
		{
			return mMaterial;
		}
		set
		{
			if (mMaterial != value)
			{
				mMaterial = value;
				mRebuildMat = true;
			}
		}
	}

	public Material dynamicMaterial => mDynamicMat;

	public Texture mainTexture
	{
		get
		{
			return mTexture;
		}
		set
		{
			mTexture = value;
			if (mDynamicMat != null)
			{
				mDynamicMat.mainTexture = value;
			}
		}
	}

	public Shader shader
	{
		get
		{
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				mShader = value;
				mRebuildMat = true;
			}
		}
	}

	public UIRect.ERenderKeywords renderKeywords
	{
		get
		{
			return mRenderKeywords;
		}
		set
		{
			if (mRenderKeywords != value)
			{
				mRenderKeywords = value;
				mRebuildMat = true;
			}
		}
	}

	public int triangles
	{
		get
		{
			if (!(mMesh != null))
			{
				return 0;
			}
			return mTriangles;
		}
	}

	public bool isClipped => mClipCount != 0;

	public void SetDepthRange(int depth)
	{
		_depthStart = depth;
		_depthEnd = depth;
	}

	public void UpdateDepthRange(int depth)
	{
		if (depth < _depthStart)
		{
			_depthStart = depth;
		}
		if (depth > _depthEnd)
		{
			_depthEnd = depth;
		}
	}

	private void CreateMaterial()
	{
		mTextureClip = false;
		mLegacyShader = false;
		mClipCount = panel.clipCount;
		string text = ((mShader != null) ? mShader.name : ((mMaterial != null) ? mMaterial.shader.name : "Unlit/Transparent Colored"));
		ShaderContainer.ShaderCache shaderCache = SingletonAuto<ShaderContainer>.instance.GetShaderCache(text, panel.clipping, mClipCount);
		mTextureClip = shaderCache.TextureClip;
		mLegacyShader = shaderCache.IsLegacyShader;
		shader = shaderCache.Shader;
		if (shader == null)
		{
			Debug.LogError(text + " shader doesn't have a clipped shader version for " + mClipCount + " clip regions");
		}
		else if (mMaterial != null)
		{
			mDynamicMat = SingletonAuto<MaterialFactory>.instance.CreateMaterial(shader, "[NGUI] " + mMaterial.name);
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			mDynamicMat.CopyPropertiesFromMaterial(mMaterial);
			string[] shaderKeywords = mMaterial.shaderKeywords;
			for (int i = 0; i < shaderKeywords.Length; i++)
			{
				mDynamicMat.EnableKeyword(shaderKeywords[i]);
			}
			if ((renderKeywords & UIRect.ERenderKeywords.GRAYSCALE_ON) == UIRect.ERenderKeywords.GRAYSCALE_ON)
			{
				mDynamicMat.EnableKeyword("GRAYSCALE_ON");
			}
			mDynamicMat.shader = shader;
		}
		else
		{
			string empty = string.Empty;
			mDynamicMat = SingletonAuto<MaterialFactory>.instance.CreateMaterial(shader, empty);
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			if ((renderKeywords & UIRect.ERenderKeywords.GRAYSCALE_ON) == UIRect.ERenderKeywords.GRAYSCALE_ON)
			{
				mDynamicMat.EnableKeyword("GRAYSCALE_ON");
			}
		}
	}

	private void CreateMaterial2()
	{
		mTextureClip = false;
		mLegacyShader = false;
		mClipCount = panel.clipCount;
		string text = ((mShader != null) ? mShader.name : ((mMaterial != null) ? mMaterial.shader.name : "Unlit/Transparent Colored"));
		ShaderContainer.ShaderCache shaderCache = SingletonAuto<ShaderContainer>.instance.GetShaderCache(text, panel.clipping, mClipCount);
		mTextureClip = shaderCache.TextureClip;
		mLegacyShader = shaderCache.IsLegacyShader;
		shader = shaderCache.Shader;
		if (mMaterial != null)
		{
			mDynamicMat = new Material(mMaterial);
			mDynamicMat.name = "[NGUI] " + mMaterial.name;
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			mDynamicMat.CopyPropertiesFromMaterial(mMaterial);
			string[] shaderKeywords = mMaterial.shaderKeywords;
			for (int i = 0; i < shaderKeywords.Length; i++)
			{
				mDynamicMat.EnableKeyword(shaderKeywords[i]);
			}
			if ((renderKeywords & UIRect.ERenderKeywords.GRAYSCALE_ON) == UIRect.ERenderKeywords.GRAYSCALE_ON)
			{
				mDynamicMat.EnableKeyword("GRAYSCALE_ON");
			}
			if (shader != null)
			{
				mDynamicMat.shader = shader;
			}
			else if (mClipCount != 0)
			{
				Debug.LogError(text + " shader doesn't have a clipped shader version for " + mClipCount + " clip regions");
			}
		}
		else
		{
			mDynamicMat = new Material(shader);
			mDynamicMat.name = "[NGUI] " + shader.name;
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			if ((renderKeywords & UIRect.ERenderKeywords.GRAYSCALE_ON) == UIRect.ERenderKeywords.GRAYSCALE_ON)
			{
				mDynamicMat.EnableKeyword("GRAYSCALE_ON");
			}
		}
	}

	private Material RebuildMaterial()
	{
		MaterialFactory.ReleaseMaterial(mDynamicMat);
		mDynamicMat = null;
		CreateMaterial();
		mDynamicMat.renderQueue = mRenderQueue;
		if (mTexture != null)
		{
			mDynamicMat.mainTexture = mTexture;
		}
		if (mRenderer != null)
		{
			mRenderer.sharedMaterials = new Material[1] { mDynamicMat };
		}
		return mDynamicMat;
	}

	private void UpdateMaterials()
	{
		if (mRebuildMat || mDynamicMat == null || mClipCount != panel.clipCount || mTextureClip != (panel.clipping == Clipping.TextureMask))
		{
			RebuildMaterial();
			mRebuildMat = false;
		}
		else if (mRenderer.sharedMaterial != mDynamicMat)
		{
			mRenderer.sharedMaterials = new Material[1] { mDynamicMat };
		}
	}

	public void UpdateGeometry(int widgetCount)
	{
		dcData.FlushPending();
		this.widgetCount = widgetCount;
		int size = dcData.verts.size;
		if (size > 0 && size == dcData.uvs.size && size == dcData.cols.size && size % 4 == 0)
		{
			if (mFilter == null)
			{
				mFilter = base.gameObject.GetComponent<MeshFilter>();
			}
			if (mFilter == null)
			{
				mFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			if (dcData.verts.size < 65000)
			{
				bool flag = _indexBuffer.VertCount != size;
				if (mMesh == null)
				{
					MeshCreateCount++;
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = ((mMaterial != null) ? ("[NGUI] " + mMaterial.name) : "[NGUI] Mesh");
					mMesh.MarkDynamic();
					flag = true;
				}
				bool flag2 = dcData.uvs.buffer.Length != dcData.verts.buffer.Length || dcData.cols.buffer.Length != dcData.verts.buffer.Length || (dcData.norms.Count > 0 && dcData.norms.buffer.Length != dcData.verts.buffer.Length) || (dcData.tans.Count > 0 && dcData.tans.buffer.Length != dcData.verts.buffer.Length);
				if (!flag2 && dcData.verts.size << 1 < dcData.verts.buffer.Length)
				{
					flag2 = true;
				}
				mTriangles = dcData.verts.size >> 1;
				if (flag2 || dcData.verts.buffer.Length > 65000)
				{
					if (flag2 || mMesh.vertexCount != dcData.verts.size)
					{
						mMesh.Clear();
						flag = true;
					}
					mMesh.vertices = dcData.verts.ToArray();
					mMesh.uv = dcData.uvs.ToArray();
					mMesh.colors32 = dcData.cols.ToArray();
					if (dcData.norms.Count > 0)
					{
						mMesh.normals = dcData.norms.ToArray();
					}
					if (dcData.tans.Count > 0)
					{
						mMesh.tangents = dcData.tans.ToArray();
					}
				}
				else
				{
					if (mMesh.vertexCount != dcData.verts.buffer.Length)
					{
						mMesh.Clear();
						flag = true;
					}
					mMesh.vertices = dcData.verts.buffer;
					mMesh.uv = dcData.uvs.buffer;
					mMesh.colors32 = dcData.cols.buffer;
					if (dcData.norms.Count > 0)
					{
						mMesh.normals = dcData.norms.buffer;
					}
					if (dcData.tans.Count > 0)
					{
						mMesh.tangents = dcData.tans.buffer;
					}
				}
				if (flag)
				{
					_indexBuffer.SetVertCount(size);
					mMesh.triangles = _indexBuffer.Buffer;
				}
				if (flag2 || !alwaysOnScreen)
				{
					mMesh.RecalculateBounds();
				}
				mFilter.mesh = mMesh;
			}
			else
			{
				mTriangles = 0;
				if (mFilter.mesh != null)
				{
					mFilter.mesh.Clear();
				}
				Debug.LogError("Too many vertices on one panel: " + dcData.verts.size);
			}
			if (mRenderer == null)
			{
				mRenderer = base.gameObject.GetComponent<MeshRenderer>();
			}
			if (mRenderer == null)
			{
				mRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			UpdateMaterials();
		}
		else
		{
			if (mFilter.mesh != null)
			{
				mFilter.mesh.Clear();
			}
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + size);
		}
		dcData.ClearAll();
	}

	private void OnWillRenderObject()
	{
		UpdateMaterials();
		if (onRender != null)
		{
			onRender(mDynamicMat ?? mMaterial);
		}
		if (mDynamicMat == null || mClipCount == 0)
		{
			return;
		}
		if (mTextureClip)
		{
			Vector4 drawCallClipRange = panel.drawCallClipRange;
			Vector2 clipSoftness = panel.clipSoftness;
			Vector2 vector = new Vector2(1000f, 1000f);
			if (clipSoftness.x > 0f)
			{
				vector.x = drawCallClipRange.z / clipSoftness.x;
			}
			if (clipSoftness.y > 0f)
			{
				vector.y = drawCallClipRange.w / clipSoftness.y;
			}
			mDynamicMat.SetVector(ClipRange[0], new Vector4((0f - drawCallClipRange.x) / drawCallClipRange.z, (0f - drawCallClipRange.y) / drawCallClipRange.w, 1f / drawCallClipRange.z, 1f / drawCallClipRange.w));
			mDynamicMat.SetTexture("_ClipTex", clipTexture);
		}
		else if (!mLegacyShader)
		{
			UIPanel parentPanel = panel;
			int num = 0;
			while (parentPanel != null)
			{
				if (parentPanel.hasClipping)
				{
					float angle = 0f;
					Vector4 drawCallClipRange2 = parentPanel.drawCallClipRange;
					if (parentPanel != panel)
					{
						Vector3 vector2 = parentPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
						drawCallClipRange2.x -= vector2.x;
						drawCallClipRange2.y -= vector2.y;
						Vector3 eulerAngles = panel.cachedTransform.rotation.eulerAngles;
						Vector3 vector3 = parentPanel.cachedTransform.rotation.eulerAngles - eulerAngles;
						vector3.x = NGUIMath.WrapAngle(vector3.x);
						vector3.y = NGUIMath.WrapAngle(vector3.y);
						vector3.z = NGUIMath.WrapAngle(vector3.z);
						if (Mathf.Abs(vector3.x) > 0.001f || Mathf.Abs(vector3.y) > 0.001f)
						{
							Debug.LogWarning("Panel can only be clipped properly if X and Y rotation is left at 0", panel);
						}
						angle = vector3.z;
					}
					SetClipping(num++, drawCallClipRange2, parentPanel.clipSoftness, angle);
				}
				parentPanel = parentPanel.parentPanel;
			}
		}
		else
		{
			Vector2 clipSoftness2 = panel.clipSoftness;
			Vector4 drawCallClipRange3 = panel.drawCallClipRange;
			Vector2 mainTextureOffset = new Vector2((0f - drawCallClipRange3.x) / drawCallClipRange3.z, (0f - drawCallClipRange3.y) / drawCallClipRange3.w);
			Vector2 mainTextureScale = new Vector2(1f / drawCallClipRange3.z, 1f / drawCallClipRange3.w);
			Vector2 vector4 = new Vector2(1000f, 1000f);
			if (clipSoftness2.x > 0f)
			{
				vector4.x = drawCallClipRange3.z / clipSoftness2.x;
			}
			if (clipSoftness2.y > 0f)
			{
				vector4.y = drawCallClipRange3.w / clipSoftness2.y;
			}
			mDynamicMat.mainTextureOffset = mainTextureOffset;
			mDynamicMat.mainTextureScale = mainTextureScale;
			mDynamicMat.SetVector("_ClipSharpness", vector4);
		}
	}

	private void SetClipping(int index, Vector4 cr, Vector2 soft, float angle)
	{
		angle *= -MathF.PI / 180f;
		Vector2 vector = new Vector2(1000f, 1000f);
		if (soft.x > 0f)
		{
			vector.x = cr.z / soft.x;
		}
		if (soft.y > 0f)
		{
			vector.y = cr.w / soft.y;
		}
		if (index < ClipRange.Length)
		{
			mDynamicMat.SetVector(ClipRange[index], new Vector4((0f - cr.x) / cr.z, (0f - cr.y) / cr.w, 1f / cr.z, 1f / cr.w));
			mDynamicMat.SetVector(ClipArgs[index], new Vector4(vector.x, vector.y, Mathf.Sin(angle), Mathf.Cos(angle)));
		}
	}

	private void Awake()
	{
		if (ClipRange == null)
		{
			ClipRange = new int[4]
			{
				Shader.PropertyToID("_ClipRange0"),
				Shader.PropertyToID("_ClipRange1"),
				Shader.PropertyToID("_ClipRange2"),
				Shader.PropertyToID("_ClipRange4")
			};
		}
		if (ClipArgs == null)
		{
			ClipArgs = new int[4]
			{
				Shader.PropertyToID("_ClipArgs0"),
				Shader.PropertyToID("_ClipArgs1"),
				Shader.PropertyToID("_ClipArgs2"),
				Shader.PropertyToID("_ClipArgs3")
			};
		}
	}

	private void OnEnable()
	{
		mRebuildMat = true;
	}

	private void ResetDrawCall()
	{
		_depthStart = int.MaxValue;
		_depthEnd = int.MinValue;
		panel = null;
		manager = null;
		mMaterial = null;
		mTexture = null;
		mShader = null;
		clipTexture = null;
		MaterialFactory.ReleaseMaterial(mDynamicMat);
		mDynamicMat = null;
		if (mRenderer != null)
		{
			mRenderer.sharedMaterials = new Material[0];
		}
	}

	private void OnDisable()
	{
		ResetDrawCall();
	}

	private void OnDestroy()
	{
		ResetDrawCall();
		NGUITools.DestroyImmediate(mMesh);
		mMesh = null;
		MeshDestroyCount++;
	}

	public static UIDrawCall Create(UIPanel panel, Material mat, Texture tex, Shader shader, UIRect.ERenderKeywords renderKeywords)
	{
		return Create(null, panel, mat, tex, shader, renderKeywords);
	}

	private static UIDrawCall Create(string name, UIPanel pan, Material mat, Texture tex, Shader shader, UIRect.ERenderKeywords renderKeywords)
	{
		UIDrawCall uIDrawCall = Create(name);
		uIDrawCall.gameObject.layer = pan.cachedGameObject.layer;
		uIDrawCall.baseMaterial = mat;
		uIDrawCall.mainTexture = tex;
		uIDrawCall.shader = shader;
		uIDrawCall.renderKeywords = renderKeywords;
		uIDrawCall.renderQueue = pan.startingRenderQueue;
		uIDrawCall.sortingOrder = pan.sortingOrder;
		uIDrawCall.manager = pan;
		return uIDrawCall;
	}

	private static UIDrawCall Create(string name)
	{
		if (mInactiveQueue.Count > 0)
		{
			UIDrawCall uIDrawCall = mInactiveQueue.Dequeue();
			mActiveDic.Add(uIDrawCall.GetInstanceID(), uIDrawCall);
			if (name != null)
			{
				uIDrawCall.name = name;
			}
			uIDrawCall.gameObject.SetActive(value: true);
			return uIDrawCall;
		}
		UIDrawCall uIDrawCall2 = SingletonAuto<DCReservePool>.instance.Create();
		mActiveDic.Add(uIDrawCall2.GetInstanceID(), uIDrawCall2);
		return uIDrawCall2;
	}

	public static void ClearAll()
	{
		_ = Application.isPlaying;
		foreach (KeyValuePair<int, UIDrawCall> item in mActiveDic)
		{
			UIDrawCall value = item.Value;
			if (value != null)
			{
				NGUITools.SetActive(value.gameObject, state: false);
			}
		}
		mActiveDic.Clear();
	}

	public static void ReleaseAll()
	{
		ClearAll();
		ReleaseInactive();
	}

	public static void ReleaseInactive()
	{
		while (mInactiveQueue.Count > 0)
		{
			UIDrawCall uIDrawCall = mInactiveQueue.Dequeue();
			if ((bool)uIDrawCall)
			{
				NGUITools.DestroyImmediate(uIDrawCall.gameObject);
			}
		}
	}

	public static void Destroy(UIDrawCall dc)
	{
		if ((bool)dc)
		{
			dc.onRender = null;
			if (mActiveDic.Remove(dc.GetInstanceID()))
			{
				dc.ResetDrawCall();
				dc.gameObject.SetActive(value: false);
				mInactiveQueue.Enqueue(dc);
			}
		}
	}
}

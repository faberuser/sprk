using System.Collections.Generic;
using JsonBin;
using NShader;
using NVespa;
using NVespa.NRenderProxy;
using UnityEngine;

namespace NGameAsset.NEffect;

public class RendererExtraData : ILoadJson
{
	public class MaterialProxy
	{
		private RenderProxy _renderProxy;

		private bool _hasColor = true;

		private bool _hasMainTex = true;

		private Color _color;

		private Vector2 _mainTextureOffset;

		private Vector2 _mainTextureScale;

		public bool IsValid => _renderProxy != null;

		public Color color
		{
			get
			{
				return _color;
			}
			set
			{
				if (!_hasColor || _color == value)
				{
					return;
				}
				_color = value;
				if (_renderProxy != null)
				{
					Material material = _renderProxy.material;
					if (!(material == null))
					{
						material.color = value;
					}
				}
			}
		}

		public Vector2 mainTextureOffset
		{
			get
			{
				return _mainTextureOffset;
			}
			set
			{
				if (_hasMainTex && !Math.Approximately(_mainTextureOffset, value))
				{
					_mainTextureOffset = value;
					if (_renderProxy != null && _renderProxy.material != null)
					{
						_renderProxy.material.mainTextureOffset = value;
					}
				}
			}
		}

		public Vector2 mainTextureScale
		{
			get
			{
				return _mainTextureScale;
			}
			set
			{
				if (_hasMainTex && !Math.Approximately(_mainTextureScale, value))
				{
					_mainTextureScale = value;
					if (_renderProxy != null && _renderProxy.material != null)
					{
						_renderProxy.material.mainTextureScale = value;
					}
				}
			}
		}

		public MaterialProxy(RenderProxy renderProxy)
		{
			_renderProxy = renderProxy;
			_hasColor = _renderProxy.sharedMaterial.HasProperty(NShader.Define.COLOR_ID);
			_hasMainTex = _renderProxy.sharedMaterial.HasProperty(NShader.Define.MAINTEX_ID);
			if (_hasColor)
			{
				_color = _renderProxy.sharedMaterial.color;
			}
			if (_hasMainTex)
			{
				_mainTextureOffset = _renderProxy.sharedMaterial.mainTextureOffset;
				_mainTextureScale = _renderProxy.sharedMaterial.mainTextureScale;
			}
		}
	}

	public class CoordinateData : ILoadJson
	{
		public enum ECoordinateType
		{
			None,
			Curve,
			Tiling
		}

		public class CurveData : ILoadJson
		{
			public AnimationCurve UOffsetCurve = new AnimationCurve();

			public AnimationCurve VOffsetCurve = new AnimationCurve();

			public AnimationCurve UTilingCurve = new AnimationCurve();

			public AnimationCurve VTilingCurve = new AnimationCurve();

			public bool IsEmpty()
			{
				if (Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(UOffsetCurve) && Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(VOffsetCurve) && Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(UTilingCurve))
				{
					return Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(VTilingCurve);
				}
				return false;
			}

			bool ILoadJson.Load(JsonRead json)
			{
				UOffsetCurve = json["UOffsetCurve"].GetAnimationCurve(UOffsetCurve);
				VOffsetCurve = json["VOffsetCurve"].GetAnimationCurve(VOffsetCurve);
				UTilingCurve = json["UTilingCurve"].GetAnimationCurve(UTilingCurve);
				VTilingCurve = json["VTilingCurve"].GetAnimationCurve(VTilingCurve);
				return true;
			}
		}

		public class TilingData : ILoadJson
		{
			public bool IsVertical;

			public int X = 1;

			public int Y = 1;

			public int LoopCount = 1;

			bool ILoadJson.Load(JsonRead json)
			{
				IsVertical = json["IsVertical"].GETBOOLEAN(IsVertical);
				X = json["X"].GETINT(X);
				Y = json["Y"].GETINT(Y);
				LoopCount = json["LoopCount"].GETINT(LoopCount);
				X = Mathf.Max(1, X);
				Y = Mathf.Max(1, Y);
				LoopCount = Mathf.Max(1, LoopCount);
				return true;
			}

			public void Calculate(float frame, out Vector2 texOffset, out Vector2 texScale)
			{
				int num = X * Y * LoopCount;
				int num2 = Mathf.Min(num - 1, (int)((float)num * frame)) % (X * Y);
				int num3;
				int num4;
				if (!IsVertical)
				{
					num3 = num2 % X;
					num4 = num2 / X;
				}
				else
				{
					num3 = num2 / Y;
					num4 = num2 % Y;
				}
				num4 = Y - num4 - 1;
				texOffset = new Vector2((float)num3 / (float)X, (float)num4 / (float)Y);
				texScale = new Vector2(1f / (float)X, 1f / (float)Y);
			}
		}

		public ECoordinateType CoordinateType = ECoordinateType.Curve;

		public CurveData Curve = new CurveData();

		public TilingData Tiling = new TilingData();

		public bool IsEmpty()
		{
			if (CoordinateType == ECoordinateType.None)
			{
				return true;
			}
			if (CoordinateType == ECoordinateType.Curve)
			{
				return Curve.IsEmpty();
			}
			if (CoordinateType == ECoordinateType.Tiling)
			{
				return false;
			}
			return true;
		}

		public void UpdateCoordinate(float frame, MaterialProxy mtrlProxy)
		{
			if (CoordinateType != ECoordinateType.None)
			{
				if (CoordinateType == ECoordinateType.Curve)
				{
					float num = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve.UOffsetCurve, frame, mtrlProxy.mainTextureOffset.x);
					float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve.VOffsetCurve, frame, mtrlProxy.mainTextureOffset.y);
					float x = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve.UTilingCurve, frame, mtrlProxy.mainTextureScale.x);
					float y = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve.VTilingCurve, frame, mtrlProxy.mainTextureScale.y);
					mtrlProxy.mainTextureOffset = new Vector2(0f - num, 0f - num2);
					mtrlProxy.mainTextureScale = new Vector2(x, y);
				}
				else if (CoordinateType == ECoordinateType.Tiling)
				{
					Tiling.Calculate(frame, out var texOffset, out var texScale);
					mtrlProxy.mainTextureOffset = texOffset;
					mtrlProxy.mainTextureScale = texScale;
				}
			}
		}

		bool ILoadJson.Load(JsonRead json)
		{
			CoordinateType = json["CoordinateType"].GETENUM(CoordinateType);
			if (CoordinateType != ECoordinateType.None)
			{
				if (CoordinateType == ECoordinateType.Curve)
				{
					Curve.LOADFROMJSON(json["Curve"]);
				}
				else if (CoordinateType == ECoordinateType.Tiling)
				{
					Tiling.LOADFROMJSON(json["Tiling"]);
				}
			}
			return true;
		}
	}

	public enum EColorType
	{
		None,
		Multiply,
		Add,
		Overwrite
	}

	public string RendererPath = string.Empty;

	public EColorType ColorType;

	public Gradient ColorCurve = new Gradient();

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public CoordinateData Coordinate;

	private GameObject _ownerAsset;

	public Renderer RenderComponent { get; private set; }

	public Material SharedMaterial
	{
		get
		{
			if (RenderComponent == null)
			{
				return null;
			}
			return RenderComponent.sharedMaterial;
		}
	}

	public IMaterialFactory MaterialFactory => SingletonAuto<MaterialPoolingFactory2>.instance;

	public RendererExtraData(GameObject asset)
	{
		_ownerAsset = asset;
	}

	public RendererExtraData(GameObject asset, string rendererPath)
	{
		_ownerAsset = asset;
		RendererPath = rendererPath;
		Transform transform = asset.transform.Find(RendererPath);
		if (!(transform == null))
		{
			RenderComponent = transform.gameObject.GetComponent<Renderer>();
		}
	}

	public void UpdateMaterial(float frame, MaterialProxy mtrlProxy, Color originalColor, float extraAlpha)
	{
		float num = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, frame, 1f) * extraAlpha;
		if (ColorType == EColorType.None)
		{
			originalColor.a = num;
			mtrlProxy.color = originalColor;
		}
		else if (ColorType == EColorType.Multiply)
		{
			originalColor.a = num;
			Color color = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorCurve, frame, Color.white);
			mtrlProxy.color = originalColor * color;
		}
		else if (ColorType == EColorType.Add)
		{
			Color color2 = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorCurve, frame, Color.white);
			Color color3 = default(Color);
			color3.r = originalColor.r + color2.r * color2.a;
			color3.g = originalColor.g + color2.g * color2.a;
			color3.b = originalColor.b + color2.b * color2.a;
			color3.a = num;
			mtrlProxy.color = color3;
		}
		else if (ColorType == EColorType.Overwrite)
		{
			Color color4 = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorCurve, frame, Color.white);
			color4.a *= num;
			mtrlProxy.color = color4;
		}
		else
		{
			mtrlProxy.color = Vespa_Unity_Color_Extension_Method.Alpha(originalColor, num);
		}
		if (Coordinate != null && !Coordinate.IsEmpty())
		{
			Coordinate.UpdateCoordinate(frame, mtrlProxy);
		}
	}

	bool ILoadJson.Load(JsonRead json)
	{
		RendererPath = json["RendererPath"].GETSTRING(RendererPath);
		ColorType = json["ColorType"].GETENUM(ColorType);
		ColorCurve = json["ColorCurve"].GetGradient(ColorCurve);
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		if (json.Contains("Coordinate"))
		{
			Coordinate = new CoordinateData();
			Coordinate.LOADFROMJSON(json["Coordinate"]);
		}
		if (_ownerAsset != null)
		{
			Transform transform = _ownerAsset.transform.Find(RendererPath);
			if (transform != null)
			{
				RenderComponent = transform.gameObject.GetComponent<Renderer>();
			}
		}
		return true;
	}

	public static void LoadList(JsonRead json, List<RendererExtraData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			RendererExtraData rendererExtraData = new RendererExtraData(null);
			if (rendererExtraData.LOADFROMJSON(fromArray))
			{
				list.Add(rendererExtraData);
			}
		}
	}
}

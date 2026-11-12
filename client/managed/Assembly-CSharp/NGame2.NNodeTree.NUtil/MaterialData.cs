using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NUtil.NMaterial;
using NShader;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NUtil;

public class MaterialData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	[Flags]
	public enum EUsingFlag
	{
		FrameColor = 1
	}

	public class DefineShaderData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string ShaderName;

		public bool IsAdditive;

		public bool IsGroundClip;

		bool ILoadJson.Load(JsonData json)
		{
			if (!json.IsObject())
			{
				return false;
			}
			ShaderName = json["ShaderName"].GETSTRING(ShaderName);
			IsAdditive = json["IsAdditive"].GETBOOLEAN(IsAdditive);
			IsGroundClip = json["IsGroundClip"].GETBOOLEAN(IsGroundClip);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("ShaderName", ShaderName);
			jsonData.AddFieldBool("IsAdditive", IsAdditive);
			jsonData.AddFieldBool("IsGroundClip", IsGroundClip);
			return jsonData;
		}

		public void SetupFromOld(string oldName)
		{
			if (oldName == "Add")
			{
				ShaderName = "NormalNoZ";
				IsAdditive = true;
				IsGroundClip = false;
			}
			if (oldName == "AlphaBlend")
			{
				ShaderName = "NormalNoZ";
				IsAdditive = false;
				IsGroundClip = false;
			}
			if (oldName == "OverlayAdd")
			{
				ShaderName = "Overlay";
				IsAdditive = true;
				IsGroundClip = false;
			}
			if (oldName == "OverlayAlphaBlend")
			{
				ShaderName = "Overlay";
				IsAdditive = false;
				IsGroundClip = false;
			}
			if (oldName == "GroundClip_Add")
			{
				ShaderName = "NormalNoZ";
				IsAdditive = true;
				IsGroundClip = true;
			}
			if (oldName == "GroundClip_Blend")
			{
				ShaderName = "NormalNoZ";
				IsAdditive = false;
				IsGroundClip = true;
			}
		}
	}

	private static readonly string[] ShaderNameArray = new string[3] { "Normal", "NormalNoZ", "Overlay" };

	private static readonly string[] ShaderPathArray = new string[3] { "Vespa/Effect2/Normal", "Vespa/Effect2/NormalNoZ", "Vespa/Effect2/Overlay" };

	private static Dictionary<string, Shader> _shaderDic;

	public DefineShaderData DefineShader = new DefineShaderData();

	public int RenderQueue = -1;

	public PrefabResAuto<Texture2D> TexAuto = new PrefabResAuto<Texture2D>();

	public PrefabResAuto<Texture2D> AlphaTexAuto = new PrefabResAuto<Texture2D>();

	public TextureTransformData TextureTransform = new TextureTransformData();

	public EUsingFlag UsingFlag;

	public FrameColor FrameColor = new FrameColor();

	private static Dictionary<string, Shader> ShaderDic
	{
		get
		{
			if (_shaderDic == null)
			{
				_shaderDic = new Dictionary<string, Shader>();
				for (int i = 0; i < ShaderNameArray.Length; i++)
				{
					Shader shader = Shader.Find(ShaderPathArray[i]);
					if (!(shader == null))
					{
						_shaderDic.Add(ShaderNameArray[i], shader);
					}
				}
			}
			return _shaderDic;
		}
	}

	public void ReservePrefab()
	{
		TexAuto.ReservePrefab();
		AlphaTexAuto.ReservePrefab();
	}

	public bool IsUsing(EUsingFlag flag)
	{
		return (UsingFlag & flag) != 0;
	}

	public void InitMaterial(Material material, float initFrame)
	{
		if (!String_Extension_Method.IsNullOrEmpty(DefineShader.ShaderName))
		{
			Shader vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ShaderDic, DefineShader.ShaderName, null);
			if (vALUE != null)
			{
				material.shader = vALUE;
				if (DefineShader.IsAdditive)
				{
					material.SetInt(Define.BLENDMODESRC_ID, 1);
					material.SetInt(Define.BLENDMODEDST_ID, 1);
				}
				else
				{
					material.SetInt(Define.BLENDMODESRC_ID, 1);
					material.SetInt(Define.BLENDMODEDST_ID, 10);
				}
				if (DefineShader.IsGroundClip)
				{
					material.EnableKeyword(Keyword.GROUNDCLIP_ON);
				}
			}
		}
		if (RenderQueue > 0)
		{
			material.renderQueue = RenderQueue;
		}
		if (TexAuto.Value != null)
		{
			material.mainTexture = TexAuto.Value.Res;
		}
		if (AlphaTexAuto.Value != null)
		{
			material.EnableKeyword("ALPHATEX_ON");
			material.DisableKeyword("ALPHATEX_OFF");
			material.SetTexture("_AlphaTex", AlphaTexAuto.Value.Res);
		}
		else
		{
			material.DisableKeyword("ALPHATEX_ON");
			material.EnableKeyword("ALPHATEX_OFF");
		}
		TextureTransform.InitMaterial(material, initFrame);
		if (IsUsing(EUsingFlag.FrameColor))
		{
			material.color = FrameColor.Evaluate(initFrame);
		}
		else
		{
			material.color = Color.white;
		}
	}

	public void UpdateMaterial(Material material, float frame)
	{
		TextureTransform.UpdateMaterial(material, frame);
		if (IsUsing(EUsingFlag.FrameColor) && FrameColor.TryEvaluate(frame, out var o_value))
		{
			material.color = o_value;
		}
	}

	public void CalcTextureTransform(float frame, out Vector2 offset, out Vector2 scale)
	{
		TextureTransform.Calculate(frame, out offset, out scale);
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		JsonData i_json = json["ShaderName"];
		if (!i_json.isNone())
		{
			DefineShader.SetupFromOld(i_json.GETSTRING(string.Empty));
		}
		JsonData jsonData = json["DefineShader"];
		if (jsonData.IsObject())
		{
			DefineShader.LOADFROMJSON(jsonData);
		}
		RenderQueue = json["RenderQueue"].GETINT(RenderQueue);
		TexAuto.LOADFROMJSON(json.GetJsonAny("TexAuto", "TexPath"));
		AlphaTexAuto.LOADFROMJSON(json.GetJsonAny("AlphaTexAuto", "AlphaTexPath"));
		TextureTransform.LOADFROMJSON(json["TextureTransform"]);
		UsingFlag = json["UsingFlag"].GETENUM(UsingFlag);
		FrameColor.LOADFROMJSON(json["FrameColor"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("DefineShader", DefineShader);
		jsonData.AddFieldLong("RenderQueue", RenderQueue);
		jsonData.AddFieldJsonMake("TexAuto", TexAuto);
		jsonData.AddFieldJsonMake("AlphaTexAuto", AlphaTexAuto);
		jsonData.AddFieldJsonMake("TextureTransform", TextureTransform);
		jsonData.AddFieldString("UsingFlag", UsingFlag.ToString());
		jsonData.AddFieldJsonMake("FrameColor", FrameColor);
		return jsonData;
	}
}

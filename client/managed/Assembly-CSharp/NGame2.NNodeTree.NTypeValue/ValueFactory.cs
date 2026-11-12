using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NTypeValue.NConstant;
using NGame2.NNodeTree.NTypeValue.NFromExternal;
using NGame2.NNodeTree.NTypeValue.NOperator;
using NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;
using NGame2.NNodeTree.NTypeValue.NOperator.NData;
using NGame2.NNodeTree.NTypeValue.NOperator.NData.NOperationData;
using NGame2.NNodeTree.NTypeValue.NOperator.NData.NSkillData;
using NGame2.NNodeTree.NTypeValue.NOperator.NData.NString;
using NGame2.NNodeTree.NTypeValue.NOperator.NDecimal;
using NGame2.NNodeTree.NTypeValue.NOperator.NFloat;
using NGame2.NNodeTree.NTypeValue.NOperator.NInteger;
using NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion;
using NGame2.NNodeTree.NTypeValue.NOperator.NString;
using NGame2.NNodeTree.NTypeValue.NOperator.NTransform;
using NGame2.NNodeTree.NTypeValue.NOperator.NVector3;
using NGame2.NNodeTree.NTypeValue.NSelect;
using NGame2.NNodeTree.NTypeValue.NVariable;
using NGame2.NNodeTree.NUtil;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue;

public class ValueFactory : SingletonAuto<ValueFactory>
{
	private Dictionary<string, ITypeValueBase> _prototypeDic = new Dictionary<string, ITypeValueBase>();

	protected override bool ThreadSafe => true;

	public ValueFactory()
	{
		AddPrototype("Uid/Constant", new UidValue());
		AddPrototype("Uid/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<long>());
		AddPrototype("Uid/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<long>());
		AddPrototype("Uid/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<long>());
		AddPrototype("Uid/Creature/CostumeIndex", new OneArgOperatorWithVar<long, long>(NGame2.NNodeTree.NTypeValue.NOperator.NInteger.CreatureOperation.GetCostumeIndex));
		AddPrototype("Uid/Creature/GetSoulWeaponGrade", new OneArgOperatorWithVar<long, long>(NGame2.NNodeTree.NTypeValue.NOperator.NInteger.CreatureOperation.GetSoulWeaponGrade));
		AddPrototype("Boolean/Constant", new BooleanValue());
		AddPrototype("Boolean/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<bool>());
		AddPrototype("Boolean/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<bool>());
		AddPrototype("Boolean/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<bool>());
		AddPrototype("Boolean/HasExternal", new HasExternal());
		AddPrototype("Boolean/Calculate/Equal", new TwoArgOperator<bool, bool, bool>(BooleanOperation.Equal));
		AddPrototype("Boolean/Calculate/NotEqual", new TwoArgOperator<bool, bool, bool>(BooleanOperation.NotEqual));
		AddPrototype("Boolean/Calculate/And", new TwoArgOperator<bool, bool, bool>(BooleanOperation.And));
		AddPrototype("Boolean/Calculate/Or", new TwoArgOperator<bool, bool, bool>(BooleanOperation.Or));
		AddPrototype("Boolean/Calculate/Inverse", new OneArgOperator<bool, bool>(BooleanOperation.Inverse));
		AddPrototype("Boolean/Float/Equal", new TwoArgOperator<bool, float, float>(CompareFloatOperation.Equal));
		AddPrototype("Boolean/Float/Less", new TwoArgOperator<bool, float, float>(CompareFloatOperation.Less));
		AddPrototype("Boolean/Float/LessEqual", new TwoArgOperator<bool, float, float>(CompareFloatOperation.LessEqual));
		AddPrototype("Boolean/Float/Great", new TwoArgOperator<bool, float, float>(CompareFloatOperation.Great));
		AddPrototype("Boolean/Float/GreatEqual", new TwoArgOperator<bool, float, float>(CompareFloatOperation.GreatEqual));
		AddPrototype("Boolean/Decimal/Equal", new TwoArgOperator<bool, decimal, decimal>((decimal a, decimal b) => a == b));
		AddPrototype("Boolean/Decimal/Less", new TwoArgOperator<bool, decimal, decimal>((decimal a, decimal b) => a < b));
		AddPrototype("Boolean/Decimal/LessEqual", new TwoArgOperator<bool, decimal, decimal>((decimal a, decimal b) => a <= b));
		AddPrototype("Boolean/Decimal/Great", new TwoArgOperator<bool, decimal, decimal>((decimal a, decimal b) => a > b));
		AddPrototype("Boolean/Decimal/GreatEqual", new TwoArgOperator<bool, decimal, decimal>((decimal a, decimal b) => a >= b));
		AddPrototype("Boolean/FieldMap/ExistFieldMapCreature", new OneArgOperatorWithVar<bool, long>(FieldMapOpertaion.ExistFieldMapRenderFacacde));
		AddPrototype("Boolean/Creature/HasSubCreature", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.HasSubCreature));
		AddPrototype("Boolean/Creature/IsAlive", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsAlive));
		AddPrototype("Boolean/Creature/IsDead", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsDead));
		AddPrototype("Boolean/Creature/IsLeftTeam", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsLeftTeam));
		AddPrototype("Boolean/Creature/IsRightTeam", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsRightTeam));
		AddPrototype("Boolean/Creature/IsFlip", new OneArgOperatorWithVar<bool, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsFlip));
		AddPrototype("Boolean/Creature/IsUsingSkillIndex", new TwoArgOperator<bool, long, long>(NGame2.NNodeTree.NTypeValue.NOperator.NBoolean.CreatureOperation.IsUsingSkillIndex));
		AddPrototype("Float/Constant", new FloatValue());
		AddPrototype("Float/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<float>());
		AddPrototype("Float/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<float>());
		AddPrototype("Float/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<float>());
		AddPrototype("Float/Calculate/Add", new TwoArgOperator<float, float, float>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.TwoArgOperation.Add));
		AddPrototype("Float/Calculate/Sub", new TwoArgOperator<float, float, float>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.TwoArgOperation.Sub));
		AddPrototype("Float/Calculate/Min", new TwoArgOperator<float, float, float>(Mathf.Min));
		AddPrototype("Float/Calculate/Max", new TwoArgOperator<float, float, float>(Mathf.Max));
		AddPrototype("Float/Calculate/Multiply", new TwoArgOperator<float, float, float>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.TwoArgOperation.Multiply));
		AddPrototype("Float/Calculate/Divide", new TwoArgOperator<float, float, float>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.TwoArgOperation.Divide));
		AddPrototype("Float/Calculate/Repeat", new TwoArgOperator<float, float, float>(Mathf.Repeat, "Value", "Length"));
		AddPrototype("Float/Calculate/Clamp", new ThreeArgOperator<float, float, float, float>(Mathf.Clamp, "Value", "Min", "Max"));
		AddPrototype("Float/Calculate/Clamp01", new OneArgOperator<float, float>(Mathf.Clamp01));
		AddPrototype("Float/Calculate/Blend", new Blend());
		AddPrototype("Float/Calculate/Blend01", new Blend01());
		AddPrototype("Float/From Vector3/X", new OneArgOperator<float, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.GetVector3X));
		AddPrototype("Float/From Vector3/Y", new OneArgOperator<float, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.GetVector3Y));
		AddPrototype("Float/From Vector3/Z", new OneArgOperator<float, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.GetVector3Z));
		AddPrototype("Float/From Vector3/Length", new OneArgOperator<float, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.Length));
		AddPrototype("Float/From Vector3/Distance", new TwoArgOperator<float, Vector3, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.TwoArgOperation.Distance));
		AddPrototype("Float/From Creature/Radius", new OneArgOperatorWithVar<float, long>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.GetCreatureRadius));
		AddPrototype("Float/From Creature/Height", new OneArgOperatorWithVar<float, long>(NGame2.NNodeTree.NTypeValue.NOperator.NFloat.OneArgOperation.GetCreatureHeight));
		AddPrototype("Vector3/Constant", new Vector3Value());
		AddPrototype("Vector3/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<Vector3>());
		AddPrototype("Vector3/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<Vector3>());
		AddPrototype("Vector3/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<Vector3>());
		AddPrototype("Vector3/Calculate/Add", new TwoArgOperator<Vector3, Vector3, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.Add));
		AddPrototype("Vector3/Calculate/Sub", new TwoArgOperator<Vector3, Vector3, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.Sub));
		AddPrototype("Vector3/Calculate/Multiply", new TwoArgOperator<Vector3, Vector3, float>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.Multiply));
		AddPrototype("Vector3/Calculate/Divide", new TwoArgOperator<Vector3, Vector3, float>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.Divide));
		AddPrototype("Vector3/Calculate/Random", new TwoArgOperator<Vector3, Vector3, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.Random));
		AddPrototype("Vector3/Calculate/LookAtDir", new TwoArgOperator<Vector3, Vector3, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.LookAtDirection, "From", "To"));
		AddPrototype("Vector3/Operation/GetDirection", new OneArgOperatorWithVar<Vector3, long>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.OneArgOperation.GetDirection));
		AddPrototype("Vector3/Operation/GetTeamDirection", new OneArgOperatorWithVar<Vector3, long>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.OneArgOperation.GetTeamDirection));
		AddPrototype("Vector3/Operation/CreateFromXYZ", new ThreeArgOperator<Vector3, float, float, float>(ThreeArgOperation.FromXYZ, "X", "Y", "Z"));
		AddPrototype("Vector3/Operation/Rotate", new ThreeArgOperator<Vector3, Vector3, Vector3, float>(ThreeArgOperation.Rotate, "Dir", "Axis", "Angle"));
		AddPrototype("Vector3/Operation/GetPosition", new GetPosition());
		AddPrototype("Vector3/Operation/BlendDirect", new BlendDirect());
		AddPrototype("Vector3/Operation/BlendRelative", new BlendRelative());
		AddPrototype("Vector3/Operation/BlendSimple", new BlendSimple());
		AddPrototype("Vector3/Replace/X", new TwoArgOperator<Vector3, Vector3, float>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.ReplaceX));
		AddPrototype("Vector3/Replace/Y", new TwoArgOperator<Vector3, Vector3, float>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.ReplaceY));
		AddPrototype("Vector3/Replace/Z", new TwoArgOperator<Vector3, Vector3, float>(NGame2.NNodeTree.NTypeValue.NOperator.NVector3.TwoArgOperation.ReplaceZ));
		AddPrototype("Quaternion/Constant", new QuaternionValue());
		AddPrototype("Quaternion/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<Quaternion>());
		AddPrototype("Quaternion/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<Quaternion>());
		AddPrototype("Quaternion/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<Quaternion>());
		AddPrototype("Quaternion/Calculate/Euler", new OneArgOperator<Quaternion, Vector3>(Quaternion.Euler));
		AddPrototype("Quaternion/Calculate/CreateFromDirection", new OneArgOperator<Quaternion, Vector3>(NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion.OneArgOperation.CreateFromDirection));
		AddPrototype("Quaternion/Calculate/Inverse", new OneArgOperator<Quaternion, Quaternion>(NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion.OneArgOperation.Inverse));
		AddPrototype("Quaternion/Calculate/Multiply", new TwoArgOperator<Quaternion, Quaternion, Quaternion>(NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion.TwoArgOperation.Multiply, "Parent", "Child"));
		AddPrototype("Quaternion/Operation/GetRotation Real", new OneArgOperatorWithVar<Quaternion, long>(NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion.OneArgOperation.GetRotationReal));
		AddPrototype("Quaternion/Operation/GetRotation View", new OneArgOperatorWithVar<Quaternion, long>(NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion.OneArgOperation.GetRotationView));
		AddPrototype("Transform/Variable", new TransformValue());
		AddPrototype("Transform/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<Transform>());
		AddPrototype("Transform/FromExternal", new NGame2.NNodeTree.NTypeValue.NFromExternal.GenericValue<Transform>());
		AddPrototype("Transform/Find Transform", new FindTransform());
		AddPrototype("Transform/Find FieldMap Transform", new FindFieldMapTransform());
		AddPrototype("Transform/SubCreature/GetTransform", new OneArgOperatorWithVar<Transform, long>(SubCreature.GetTransfrom));
		AddPrototype("Object/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<object>());
		AddPrototype("Object/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<object>());
		AddPrototype("String/Constant", new StringValue());
		AddPrototype("String/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<string>());
		AddPrototype("String/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<string>());
		AddPrototype("String/Concat", new ConcatFunc());
		AddPrototype("String/Format", new FormatFunc());
		AddPrototype("String/Localize", new LocalizeFunc());
		AddPrototype("String/SoulWeapon/GetSoulStoneBodyPrefix", new OneArgOperatorWithVar<string, long>(ContentsFunc.GetSoulStoneBodyPrefix));
		AddPrototype("Decimal/Constant", new DecimalValue());
		AddPrototype("Decimal/Variable", new NGame2.NNodeTree.NTypeValue.NVariable.GenericValue<decimal>());
		AddPrototype("Decimal/Select", new NGame2.NNodeTree.NTypeValue.NSelect.GenericValue<decimal>());
		AddPrototype("Decimal/Calculate/Add", new TwoArgOperator<decimal, decimal, decimal>(NGame2.NNodeTree.NTypeValue.NOperator.NDecimal.TwoArgOperation.Add));
		AddPrototype("Decimal/Calculate/Sub", new TwoArgOperator<decimal, decimal, decimal>(NGame2.NNodeTree.NTypeValue.NOperator.NDecimal.TwoArgOperation.Sub));
		AddPrototype("Decimal/Calculate/Multiply", new TwoArgOperator<decimal, decimal, decimal>(NGame2.NNodeTree.NTypeValue.NOperator.NDecimal.TwoArgOperation.Multiply));
		AddPrototype("Decimal/Calculate/Divide", new TwoArgOperator<decimal, decimal, decimal>(NGame2.NNodeTree.NTypeValue.NOperator.NDecimal.TwoArgOperation.Divide));
		AddPrototype("Decimal/Calculate/Min", new TwoArgOperator<decimal, decimal, decimal>(System.Math.Min));
		AddPrototype("Decimal/Calculate/Max", new TwoArgOperator<decimal, decimal, decimal>(System.Math.Max));
		AddPrototype("Decimal/Calculate/Round", new OneArgOperator<decimal, decimal>(System.Math.Round));
		AddPrototype("Decimal/Calculate/Ceiling", new OneArgOperator<decimal, decimal>(System.Math.Ceiling));
		AddPrototype("Decimal/Calculate/Floor", new OneArgOperator<decimal, decimal>(System.Math.Floor));
		AddPrototype("Decimal/CreatureCustomStat", new CreatureCustomStat());
		AddPrototype("Decimal/Convert/ToFloat", new OneArgOperator<float, decimal>((decimal value) => (float)value));
		AddPrototype("Decimal/Convert/FromFloat", new OneArgOperator<decimal, float>((float value) => (decimal)value));
		AddPrototype("Data/ValueFromSkillData", new ValueFromSkillData());
		AddPrototype("Data/ValueFromOperationData", new ValueFromOperationData());
		AddPrototype("Data/ValueFromStatData", new ValueFromStatData());
		AddPrototype("Data/String/GetSkillName", new GetSkillName());
		AddPrototype("Data/SkillData/OperationFireCount", new OperationFireCount());
		AddPrototype("Data/OperationData/CalcDamage", new CalcDamage());
		AddPrototype("Data/OperationData/CalcDot", new CalcDot());
		AddPrototype("Data/OperationData/CalcDot_OneTick", new CalcDot_OneTick());
	}

	private void AddPrototype(string valuePath, ITypeValueBase valueBase)
	{
		valueBase.ValuePath = valuePath;
		_prototypeDic[valuePath] = valueBase;
	}

	public ITypeValueBase Create(string typePath)
	{
		ITypeValueBase vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_prototypeDic, typePath, null);
		if (vALUE == null)
		{
			return null;
		}
		ITypeValueBase typeValueBase = vALUE.CreateFromPrototype();
		typeValueBase.ValuePath = vALUE.ValuePath;
		return typeValueBase;
	}

	public ITypeValueBase CreateFromJson(JsonData json)
	{
		ITypeValueBase typeValueBase = Create(json["ValuePath"].GETSTRING(string.Empty));
		if (typeValueBase == null)
		{
			return null;
		}
		typeValueBase.LoadFromBodyJson(json["Body"]);
		return typeValueBase;
	}

	public JsonData MakeJson(ITypeValueBase valueBase)
	{
		if (valueBase == null)
		{
			return null;
		}
		string valuePath = valueBase.ValuePath;
		if (String_Extension_Method.IsNullOrEmpty(valuePath))
		{
			return null;
		}
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("ValuePath", valuePath);
		jsonData.AddField("Body", valueBase.MakeBodyJson());
		return jsonData;
	}

	public ITypeValueBase CreateDefault()
	{
		return Create("Float/Constant");
	}

	public ITypeValueBase CreateConstant<T>(T value)
	{
		string constantName = Lookup.GetTypeData<T>().ConstantName;
		if (Create(constantName) is AbstractValue<T> abstractValue)
		{
			abstractValue.Value = value;
			return abstractValue;
		}
		return null;
	}

	public ITypeValueBase CreateVariable<T>(StringKey variableKey)
	{
		string variableName = Lookup.GetTypeData<T>().VariableName;
		if (Create(variableName) is NGame2.NNodeTree.NTypeValue.NVariable.ValueBase<T> valueBase)
		{
			valueBase.Name = variableKey;
			return valueBase;
		}
		return null;
	}

	public ITypeValueBase CreateByKeyword(params string[] DefaultKeyword)
	{
		ITypeValueBase typeValueBase = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(DefaultKeyword); i++)
		{
			if (DefaultKeyword[i].IndexOf("position", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateConstant(Vector3.zero);
			}
			else if (DefaultKeyword[i].IndexOf("destination", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<Vector3>(VariableKeyword.Destination);
			}
			else if (DefaultKeyword[i].IndexOf("direction", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<Vector3>(VariableKeyword.Direction);
			}
			else if (DefaultKeyword[i].IndexOf("uid", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<long>(StringKey.Empty);
			}
			else if (DefaultKeyword[i].IndexOf("long", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<long>(StringKey.Empty);
			}
			else if (DefaultKeyword[i].IndexOf("vector3", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateConstant(Vector3.zero);
			}
			else if (DefaultKeyword[i].IndexOf("frame", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<float>(VariableKeyword.Frame);
			}
			else if (DefaultKeyword[i].IndexOf("duration", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<float>(VariableKeyword.DurationTime);
			}
			else if (DefaultKeyword[i].IndexOf("transform", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				typeValueBase = CreateVariable<Transform>(StringKey.Empty);
			}
			if (typeValueBase != null)
			{
				return typeValueBase;
			}
		}
		return CreateDefault();
	}
}

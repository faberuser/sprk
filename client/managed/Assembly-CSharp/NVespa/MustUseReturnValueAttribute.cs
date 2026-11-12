using System;

namespace NVespa;

[AttributeUsage(AttributeTargets.Method)]
public class MustUseReturnValueAttribute : Attribute
{
}

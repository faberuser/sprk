using System;

namespace NShared.NUtil;

public class SimpleRNG
{
	private static uint m_w;

	private static uint m_z;

	static SimpleRNG()
	{
		m_w = 521288629u;
		m_z = 362436069u;
	}

	public static void SetSeed(uint u, uint v)
	{
		if (u != 0)
		{
			m_w = u;
		}
		if (v != 0)
		{
			m_z = v;
		}
	}

	public static void SetSeed(uint u)
	{
		m_w = u;
	}

	public static void SetSeedFromSystemTime()
	{
		long num = DateTime.Now.ToFileTime();
		SetSeed((uint)(num >> 16), (uint)(num % 4294967296L));
	}

	public static double GetUniform()
	{
		return ((double)GetUint() + 1.0) * 2.328306435454494E-10;
	}

	public static uint GetUint()
	{
		m_z = 36969 * (m_z & 0xFFFF) + (m_z >> 16);
		m_w = 18000 * (m_w & 0xFFFF) + (m_w >> 16);
		return (m_z << 16) + m_w;
	}

	public static double GetNormal()
	{
		double uniform = GetUniform();
		double uniform2 = GetUniform();
		double num = Math.Sqrt(-2.0 * Math.Log(uniform));
		double a = Math.PI * 2.0 * uniform2;
		return num * Math.Sin(a);
	}

	public static double GetNormal(double mean, double standardDeviation)
	{
		if (standardDeviation <= 0.0)
		{
			throw new ArgumentOutOfRangeException($"Shape must be positive. Received {standardDeviation}.");
		}
		return mean + standardDeviation * GetNormal();
	}

	public static double GetExponential()
	{
		return 0.0 - Math.Log(GetUniform());
	}

	public static double GetExponential(double mean)
	{
		if (mean <= 0.0)
		{
			throw new ArgumentOutOfRangeException($"Mean must be positive. Received {mean}.");
		}
		return mean * GetExponential();
	}

	public static double GetGamma(double shape, double scale)
	{
		if (shape >= 1.0)
		{
			double num = shape - 1.0 / 3.0;
			double num2 = 1.0 / Math.Sqrt(9.0 * num);
			double num3;
			while (true)
			{
				double normal = GetNormal();
				num3 = 1.0 + num2 * normal;
				if (!(num3 <= 0.0))
				{
					num3 = num3 * num3 * num3;
					double uniform = GetUniform();
					double num4 = normal * normal;
					if (uniform < 1.0 - 0.0331 * num4 * num4 || Math.Log(uniform) < 0.5 * num4 + num * (1.0 - num3 + Math.Log(num3)))
					{
						break;
					}
				}
			}
			return scale * num * num3;
		}
		if (shape <= 0.0)
		{
			throw new ArgumentOutOfRangeException($"Shape must be positive. Received {shape}.");
		}
		double gamma = GetGamma(shape + 1.0, 1.0);
		double uniform2 = GetUniform();
		return scale * gamma * Math.Pow(uniform2, 1.0 / shape);
	}

	public static double GetChiSquare(double degreesOfFreedom)
	{
		return GetGamma(0.5 * degreesOfFreedom, 2.0);
	}

	public static double GetInverseGamma(double shape, double scale)
	{
		return 1.0 / GetGamma(shape, 1.0 / scale);
	}

	public static double GetWeibull(double shape, double scale)
	{
		if (shape <= 0.0 || scale <= 0.0)
		{
			throw new ArgumentOutOfRangeException($"Shape and scale parameters must be positive. Recieved shape {shape} and scale{scale}.");
		}
		return scale * Math.Pow(0.0 - Math.Log(GetUniform()), 1.0 / shape);
	}

	public static double GetCauchy(double median, double scale)
	{
		if (scale <= 0.0)
		{
			throw new ArgumentException($"Scale must be positive. Received {scale}.");
		}
		double uniform = GetUniform();
		return median + scale * Math.Tan(Math.PI * (uniform - 0.5));
	}

	public static double GetStudentT(double degreesOfFreedom)
	{
		if (degreesOfFreedom <= 0.0)
		{
			throw new ArgumentException($"Degrees of freedom must be positive. Received {degreesOfFreedom}.");
		}
		double normal = GetNormal();
		double chiSquare = GetChiSquare(degreesOfFreedom);
		return normal / Math.Sqrt(chiSquare / degreesOfFreedom);
	}

	public static double GetLaplace(double mean, double scale)
	{
		double uniform = GetUniform();
		if (!(uniform < 0.5))
		{
			return mean - scale * Math.Log(2.0 * (1.0 - uniform));
		}
		return mean + scale * Math.Log(2.0 * uniform);
	}

	public static double GetLogNormal(double mu, double sigma)
	{
		return Math.Exp(GetNormal(mu, sigma));
	}

	public static double GetBeta(double a, double b)
	{
		if (a <= 0.0 || b <= 0.0)
		{
			throw new ArgumentOutOfRangeException($"Beta parameters must be positive. Received {a} and {b}.");
		}
		double gamma = GetGamma(a, 1.0);
		double gamma2 = GetGamma(b, 1.0);
		return gamma / (gamma + gamma2);
	}
}

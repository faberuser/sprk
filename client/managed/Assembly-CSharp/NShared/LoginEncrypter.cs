using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NVespa;

namespace NShared;

public class LoginEncrypter
{
	public static LoginUserId CreateEncryptedLoginId(string certificateString, string clientAesKey, long salt, LoginUserId loginUserId)
	{
		string id = EncryptData(certificateString, clientAesKey + "," + salt + "," + loginUserId.id);
		string legacyId = string.Empty;
		if (!String_Extension_Method.IsNullOrEmpty(loginUserId.legacyId))
		{
			legacyId = EncryptData(certificateString, clientAesKey + "," + salt + "," + loginUserId.legacyId);
		}
		return new LoginUserId(id, legacyId, loginUserId.accountId);
	}

	private static string EncryptData(string certificateString, string source)
	{
		if (string.IsNullOrEmpty(certificateString))
		{
			return null;
		}
		try
		{
			X509Certificate2 x509Certificate = new X509Certificate2(Encoding.UTF8.GetBytes(certificateString));
			byte[] bytes = Encoding.UTF8.GetBytes(source);
			return Convert.ToBase64String((x509Certificate.PublicKey.Key as RSACryptoServiceProvider).Encrypt(bytes, fOAEP: true));
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
			return null;
		}
	}
}

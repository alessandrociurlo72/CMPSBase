using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace FrmNetCore.Crypto
{
    public static class SignHelper
    {
        public static void SignXml(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Add the key to the SignedXml document.
            signedXml.SigningKey = cert.GetRSAPrivateKey();

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Include the public key of the certificate in the assertion.
            signedXml.KeyInfo = new KeyInfo();
            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(cert, X509IncludeOption.WholeChain));

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        
        }

        public static byte[] SignData(byte[] data, X509Certificate2 cert)
        {
      
            using (HashAlgorithm hasher = SHA1.Create())
            { 
                using (RSA rsa = cert.GetRSAPrivateKey())
                {
                  
                    return rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                }
            }

        }

        public static bool VerifyData(byte[] data, byte[] signature, X509Certificate2 cert)
        {
            
            using (HashAlgorithm hasher = SHA1.Create())
            {
                using (RSA rsa = cert.GetRSAPublicKey())
                {

                    return rsa.VerifyData(data, signature, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                }
            }

        }

        public static string SignBase64(string str64, X509Certificate2 cert)
        {
            byte[] data = Convert.FromBase64String(str64);
            byte[] signeddata = SignData(data, cert);

            return Convert.ToBase64String(signeddata);

        }

        public static bool VerifySignBase64(string tokenID, string sign, X509Certificate2 cert)
        {
            byte[] signdata = Convert.FromBase64String(sign);
            byte[] data = Convert.FromBase64String(tokenID);

            return VerifyData(data, signdata, cert);
        }
    }
    }

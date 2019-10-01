// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using MarginTrading.CommissionService.Core.Settings;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace MarginTrading.CommissionService.SqlRepositories
{
    public static class RsaHelper
    {
        public static string SignData(string data, SignatureSettings settings)
        {
            var rsaKey = GetPrivateKeyFromPemFormat(settings);

            // Create a new XML document.
            var xmlDoc = new XmlDocument {PreserveWhitespace = true};

            // Load an XML string into the XmlDocument object.
            xmlDoc.LoadXml(data);
            
            // Create a SignedXml object.
            var signedXml = new SignedXml(xmlDoc) {SigningKey = rsaKey};

            // Create a reference to be signed. Uri = "" means the whole document
            var reference = new Reference {Uri = ""};

            // Add an enveloped transformation to the reference.
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            var xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
            
            return xmlDoc.OuterXml;
        }

        public static bool ValidateSign(string data, SignatureSettings settings, out string validationError)
        {
            validationError = string.Empty;
            
            if (!settings.ShouldValidateSignature)
            {
                return true;
            }

            var rsaKey = PublicKeyFromPemFormat(settings);

            // Create a new XML document.
            var xmlDoc = new XmlDocument {PreserveWhitespace = true};

            // Load an XML string into the XmlDocument object.
            xmlDoc.LoadXml(data);
            
            // Create a new SignedXml object and pass it
            // the XML document class.
            var signedXml = new SignedXml(xmlDoc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            var nodeList = xmlDoc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                validationError = "Verification failed: No Signature was found in the document.";
                return false;
            }

            // We only support one signature for
            // the entire XML document.  Throw an exception 
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                validationError = "Verification failed: More that one signature was found for the document.";
                return false;
            }

            // Load the first <signature> node.  
            signedXml.LoadXml((XmlElement)nodeList[0]);
            
            // Check the signature and return the result.
            return signedXml.CheckSignature(rsaKey);
        }
        
        private static void GenerateRsaKeyPair(string privateKeyFilePath, string publicKeyFilePath)
        {
            RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator();
            rsaGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            var keyPair = rsaGenerator.GenerateKeyPair();

            using (TextWriter privateKeyTextWriter = new StringWriter())
            {
                PemWriter pemWriter = new PemWriter(privateKeyTextWriter);
                pemWriter.WriteObject(keyPair.Private);
                pemWriter.Writer.Flush();
                File.WriteAllText(privateKeyFilePath, privateKeyTextWriter.ToString());
            }

            using (TextWriter publicKeyTextWriter = new StringWriter())
            {
                PemWriter pemWriter = new PemWriter(publicKeyTextWriter);
                pemWriter.WriteObject(keyPair.Public);
                pemWriter.Writer.Flush();
                File.WriteAllText(publicKeyFilePath, publicKeyTextWriter.ToString());
            }
        }

        private static RSACryptoServiceProvider GetPrivateKeyFromPemFormat(SignatureSettings settings)
        {
            var keyValue = "";

            switch (settings.KeySource)
            {
                case CryptoKeySource.File:
                    keyValue = File.Exists(settings.PrivateKeyPath) ? File.ReadAllText(settings.PrivateKeyPath) : "";
                    break;
                
                case CryptoKeySource.EnvVar:
                    keyValue = Environment.GetEnvironmentVariable(settings.PrivateKeyPath);
                    break;
            }
            
            if (string.IsNullOrEmpty(keyValue))
            {
                if (settings.ShouldGenerateKey)
                {
                    if (settings.KeySource == CryptoKeySource.File)
                    {
                        GenerateRsaKeyPair(settings.PrivateKeyPath, settings.PublicKeyPath);
                        keyValue = File.ReadAllText(settings.PrivateKeyPath);
                    }
                    else
                    {
                        throw new Exception(
                            $"Private key {settings.PrivateKeyPath} does not exist and can not be generated for source type {settings.KeySource}");
                    }
                }
                else
                {
                    throw new Exception($"Private key {settings.PrivateKeyPath} does not exist and not configured to be generated");
                }
            }
            
            using (TextReader privateKeyTextReader = new StringReader(keyValue))
            {
                AsymmetricCipherKeyPair readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();

                RsaPrivateCrtKeyParameters privateKeyParams = ((RsaPrivateCrtKeyParameters)readKeyPair.Private);
                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                RSAParameters parms = new RSAParameters();

                parms.Modulus = privateKeyParams.Modulus.ToByteArrayUnsigned();
                parms.P = privateKeyParams.P.ToByteArrayUnsigned();
                parms.Q = privateKeyParams.Q.ToByteArrayUnsigned();
                parms.DP = privateKeyParams.DP.ToByteArrayUnsigned();
                parms.DQ = privateKeyParams.DQ.ToByteArrayUnsigned();
                parms.InverseQ = privateKeyParams.QInv.ToByteArrayUnsigned();
                parms.D = privateKeyParams.Exponent.ToByteArrayUnsigned();
                parms.Exponent = privateKeyParams.PublicExponent.ToByteArrayUnsigned();

                cryptoServiceProvider.ImportParameters(parms);

                return cryptoServiceProvider;
            }
        }

        private static RSACryptoServiceProvider PublicKeyFromPemFormat(SignatureSettings settings)
        {
            var keyValue = "";

            switch (settings.KeySource)
            {
                case CryptoKeySource.File:
                    keyValue = File.Exists(settings.PublicKeyPath) ? File.ReadAllText(settings.PublicKeyPath) : "";
                    break;
                
                case CryptoKeySource.EnvVar:
                    keyValue = Environment.GetEnvironmentVariable(settings.PublicKeyPath);
                    break;
            }
            
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception($"Public key {settings.PublicKeyPath} does not exist");
            }
            
            using (TextReader publicKeyTextReader = new StringReader(keyValue))
            {
                RsaKeyParameters publicKeyParam = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                RSAParameters parms = new RSAParameters();

                parms.Modulus = publicKeyParam.Modulus.ToByteArrayUnsigned();
                parms.Exponent = publicKeyParam.Exponent.ToByteArrayUnsigned();

                cryptoServiceProvider.ImportParameters(parms);

                return cryptoServiceProvider;
            }
        }
    }
}
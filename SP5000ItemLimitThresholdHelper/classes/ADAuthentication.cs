using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SP5000ItemLimitThresholdHelper.classes
{
    static class ADAuthentication
    {
        static Dictionary<string, X509Certificate2> smartCardCerts = new Dictionary<string, X509Certificate2>();
        static Dictionary<string, X509Certificate2> trustedPeopleCerts = new Dictionary<string, X509Certificate2>();
        static public Dictionary<string, X509Certificate2> GetSmartCardCertifcates()
        {
            if (smartCardCerts.Count != 0)
                return smartCardCerts;

            var myStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            myStore.Open(OpenFlags.ReadOnly);
            int certNo = 0;
            foreach (X509Certificate2 cert in myStore.Certificates)
            {
                if (!cert.HasPrivateKey) continue; // not smartcard for sure
                try
                {
                    var rsa = cert.PrivateKey as RSACryptoServiceProvider;
                    if (rsa == null) continue; // not smart card cert again
                    if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                    {
                        // inspect rsa.CspKeyContainerInfo.KeyContainerName Property
                        // or rsa.CspKeyContainerInfo.ProviderName (your smartcard provider, such as 
                        // "Schlumberger Cryptographic Service Provider" for Schlumberger Cryptoflex 4K
                        // card, etc
                        certNo++;
                        smartCardCerts.Add($"{certNo} - {cert.Issuer} / {cert.PublicKey.Oid.FriendlyName}", cert);
                        //var data = rsa.SignData(); // to confirm presence of private key - to finally authenticate
                    }
                }
                catch
                {

                }
            }
            myStore.Close();
            return smartCardCerts;
        }

        static public Dictionary<string, X509Certificate2> GetTrustedPeopleCertifcates()
        {
            if (trustedPeopleCerts.Count != 0)
                return trustedPeopleCerts;

            var tpStore = new X509Store(StoreName.TrustedPeople, StoreLocation.CurrentUser);
            tpStore.Open(OpenFlags.ReadOnly);
            int certNo = 0;
            foreach (X509Certificate2 cert in tpStore.Certificates)
            {
//                if (cert.HasPrivateKey) continue; // not public for sure
                try
                {
                    certNo++;
                    trustedPeopleCerts.Add($"{certNo} - {cert.SubjectName.Name} / {cert.PublicKey.Oid.FriendlyName}", cert);
                }
                catch
                {

                }
            }
            tpStore.Close();
            return trustedPeopleCerts;
        }

        // imaging this action is called after user authorized by remote server
        //public ActionResult Login()
        //{
        //    // imaging this method gets authorized certificate string 
        //    // from Request.ClientCertificate or even a remote server
        //    var userCer = _certificateManager.GetCertificateString();

        //    // you have own user manager which returns user by certificate string
        //    var user = _myUserManager.GetUserByCertificate(userCer);

        //    if (user != null)
        //    {
        //        // user is valid, going to authenticate user for my App
        //        var ident = new ClaimsIdentity(
        //            new[]
        //            {
        //        // since userCer is unique for each user we could easily
        //        // use it as a claim. If not use user table ID 
        //        new Claim("Certificate", userCer),

        //        // adding following 2 claim just for supporting default antiforgery provider
        //        new Claim(ClaimTypes.NameIdentifier, userCer),
        //        new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"),

        //        // an optional claim you could omit this 
        //        new Claim(ClaimTypes.Name, user.Name),

        //        // populate assigned user's role form your DB 
        //        // and add each one as a claim  
        //        new Claim(ClaimTypes.Role, user.Roles[0].Name),
        //        new Claim(ClaimTypes.Role, user.Roles[1].Name),
        //                // and so on
        //            },
        //            DefaultAuthenticationTypes.ApplicationCookie);

        //        // Identity is sign in user based on claim don't matter 
        //        // how you generated it Identity take care of it
        //        HttpContext.GetOwinContext().Authentication.SignIn(
        //            new AuthenticationProperties { IsPersistent = false }, ident);

        //        // auth is succeed, without needing any password just claim based 
        //        return RedirectToAction("MyAction");
        //    }
        //    // invalid certificate  
        //    ModelState.AddModelError("", "We could not authorize you :(");
        //    return View();
        //}
    }

    class RSACSPSample
    {
        static void TestMain()
        {
            try
            {
                // Create a UnicodeEncoder to convert between byte array and string.
                ASCIIEncoding ByteConverter = new ASCIIEncoding();

                string dataString = "Data to Sign";

                // Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] originalData = ByteConverter.GetBytes(dataString);
                byte[] signedData;

                // Create a new instance of the RSACryptoServiceProvider class 
                // and automatically create a new key-pair.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                // Export the key information to an RSAParameters object.
                // You must pass true to export the private key for signing.
                // However, you do not need to export the private key
                // for verification.
                RSAParameters Key = RSAalg.ExportParameters(true);

                // Hash and sign the data.
                signedData = HashAndSignBytes(originalData, Key);

                // Verify the data and display the result to the 
                // console.
                if (VerifySignedHash(originalData, signedData, Key))
                {
                    Console.WriteLine("The data was verified.");
                }
                else
                {
                    Console.WriteLine("The data does not match the signature.");
                }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("The data was not signed or verified");
            }
        }
        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.  
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Verify the data using the signature.  Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.VerifyData(DataToVerify, new SHA1CryptoServiceProvider(), SignedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BinArchiver
{    
    /// <summary>
    /// Helper class that encrypts a BinFile into a CryptedBinFile using AES.
    /// It can also be used to decrypt (for testing purposes).
    /// </summary>
    public class BinFileCrypter : IDisposable
    {
        public BinFileCrypter(byte[] key)  // Create this for some key.
        {
            this.rijndaelManaged = new RijndaelManaged();
            this.rijndaelManaged.Mode = CipherMode.CBC;
            this.rijndaelManaged.Padding = PaddingMode.PKCS7;
            this.rijndaelManaged.KeySize = key.Length * 8;
            this.rijndaelManaged.Key = key;
        }

        /// <summary>
        /// Convert a BinFile into a CryptedBinFile. 
        /// Only call this once! The iv should be recreated if you want to restart this!
        /// Just create a new object if you want this!
        /// TODO: protobuf-net library does not handle the optional field for some reason, so we must add a restrict value. Value 0 shall be ignored!
        /// </summary>
        /// <param name="binFile">The BinFile that you want to encrypt. Make sure that it's fully filled.</param>
        /// <param name="restrictedToValue">The value that you want to restrict to. 0 shall mean that it's not restricted in our use case.</param>
        public CryptedBinFile convert(BinFile binFile, UInt32 restrictedToValue = 0)
        {
            var binFileStream = new MemoryStream();
            Serializer.Serialize(binFileStream, binFile);

            var binFileBytes = binFileStream.ToArray();
            
            // For this moment we also just write it out as a bytestream, just to keep things simple.
            //Console.WriteLine("FirmwareFile encoded into: " + Convert.ToBase64String(memoryStream.ToArray()));

            this.fillIV(restrictedToValue);
            var initialiszationVector = new byte[16];
            this.rijndaelManaged.IV.CopyTo(initialiszationVector, 0);

            var cryptedBytes = encryptAndExportStream(binFileBytes);

            // Also try to decrypt it for a moment! We should retreive two blocks, it's now 32 byte big. Some zeros should be added on the end... Then we should now how many zeros we have, or how many bytes...
            initialiszationVector.CopyTo(this.rijndaelManaged.IV, 0);
            var decryptedBytes = decryptByteArray(cryptedBytes);

            // TODO: We need to know how many bytes we should copy, on the other hand: we also do know when we are ready parsing the pb, that is also nice!
            // What is best practice?

            CryptedBinFile result = new CryptedBinFile()
            {
                aesInitializationVector = initialiszationVector,    // 16 bytes required, restrict value may also be set to 0, this is the same a not adding it.
                restrictToValue = restrictedToValue,
                cryptedBinFile = cryptedBytes
            };

            return result;
        }

        private byte[] encryptAndExportStream(byte[] input)
        {
            byte[] result = null;

            // Here is some explaining on all this nice stuff: http://stackoverflow.com/a/8779595/619465
            using (var encryptor = this.rijndaelManaged.CreateEncryptor())
            {
                result = encryptor.TransformFinalBlock(input, 0, input.Length);
            }

            return result;
        }

        /// <summary>
        /// Decrypts a CryptedBinFile into a readable BinFile.
        /// </summary>
        /// <param name="cryptedFirmwareFile">The file that you want to decrypt.</param>
        /// <returns>An unencrypted file if all went well. Returns null if the decrypt was not succesfull.</returns>
        public BinFile Decrypt(CryptedBinFile cryptedFirmwareFile)
        {
            this.rijndaelManaged.IV = cryptedFirmwareFile.aesInitializationVector;
            var decrypted = this.decryptByteArray(cryptedFirmwareFile.cryptedBinFile);
            
            // TODO: Return a seperate object that returns the error that happened.
            return Serializer.Deserialize<BinFile>(new MemoryStream(decrypted));
        }

        private byte[] decryptByteArray(byte[] cipherBytes)
        {
            byte[] result = null;

            // Here is some explaining on all this nice stuff: http://stackoverflow.com/a/8779595/619465
            using (var decryptor = this.rijndaelManaged.CreateDecryptor())
            {
                result = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            }

            return result;
        }

        private RijndaelManaged rijndaelManaged;

        private void fillIV(UInt32? restrictValue)
        {
            rijndaelManaged.GenerateIV();

            if (restrictValue.HasValue)
            {
                rijndaelManaged.IV[0] = 0;  // First the high byte or first the low byte?
                rijndaelManaged.IV[1] = 0;
                rijndaelManaged.IV[2] = 0;
                rijndaelManaged.IV[3] = 0;

                var restrictBytes = BitConverter.GetBytes(restrictValue.Value);
                restrictBytes.CopyTo(rijndaelManaged.IV, 0);    // What is the byte order? Create a unit test for this!
            }
        }

        public void Dispose()
        {
            // We should dispose the object that we created.
            this.rijndaelManaged.Dispose();
        }
    }
}

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
    /// Append binary files to an existing archive.
    /// The arguments can be set during object existance so that we can use it in a command line argument loop.
    /// </summary>
    public class ArchiveAppender
    {
        private BinArchive binArchive = null;
        private string archiveFilename = null;
        private int appendCall = 0;

        /// <summary>
        /// Initiates a new instance of the ArchiveAppender type.
        /// Does not take any arguments so that you can use the object to gather the right arguments. Not all are required.
        /// </summary>
        public ArchiveAppender()
        {
        }

        /// <summary>
        /// Opens the archive and loads it into memory (don't use to big files for now!)
        /// </summary>
        /// <returns>True when all went ok.</returns>
        public bool Prepare(string archiveFilename)
        {
            // Ignores the call if it's called for a second time!
            if (this.binArchive == null)
            {
                using (var fileStream = File.OpenRead(archiveFilename))
                {
                    this.binArchive = ProtoBuf.Serializer.Deserialize<BinArchive>(fileStream);
                    this.archiveFilename = archiveFilename;
                }                
            }

            return (this.binArchive != null);
        }

        /// <summary>
        /// Use this to set the file that you want to append, it must exist.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Use this to set the version that you want to add to the appended file.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Use this property to add/remove the typeValues that are going to be saved with the next file that you Append().
        /// </summary>
        public List<UInt32> TypeValues { get; set; }

        /// <summary>
        /// Use this to configure an AES encryption key.
        /// </summary>
        public byte[] Key { get; set; }

        /// <summary>
        /// Use this to restrict the whole stuff to a certain value.
        /// </summary>
        public uint RestrictToValue { get; set; }

        /// <summary>
        /// Appends the file with the properties that you configured on this object.
        /// It will be encrypted if you have set the Key propertie.
        /// </summary>
        /// <returns>True when all went ok, false when something failed for some reason.</returns>
        public bool Append()
        {
            if (this.binArchive != null)
            {
                var binFile = new BinFile()
                {
                    version = this.Version,
                    content = File.ReadAllBytes(this.Filename)   // TODO: Will this always fit in memory? For now we just use very simple and small files.              
                };

                // Unfortunate we can't everything into the object initialiser:
                binFile.typeValue.AddRange(this.TypeValues);
                binFile.size = (uint)binFile.content.Length;

                // Calculate the checksum in a proper way for our file data. We now that we should place 20 bytes into our test file!
                binFile.check = 3141592653;
                binFile.crc = calculateCrc32(binFile.content);
                binFile.sha1 = calculateSha1(binFile.content);     // Should always be 20 bytes...

                binFile.time = Helpers.convertToUnixTimeMilliseconds(File.GetCreationTimeUtc(this.Filename));

                // Now add the binFile!
                if (this.Key == null)
                {
                    this.binArchive.binFiles.Add(binFile);
                }
                else
                {
                    // Encrypt it when a key was given!
                    var binFileCrypter = new BinFileCrypter(this.Key);
                    var cryptedBinFile = binFileCrypter.convert(binFile, this.RestrictToValue);
                    this.binArchive.cryptedBinFiles.Add(cryptedBinFile);
                }

                // Also increase our append call, so that we know that we need to finish it really at the end.
                this.appendCall++;
            }
            else
            {
                throw new InvalidOperationException("Trying to call Append() without calling Prepare() first!");
            }

            return true;
        }

        /// <summary>
        /// Rewrite the whole protocol buffers file if needed. If you don't call this then nothing changes effectively.
        /// </summary>
        /// <returns>True when a new version has been written. False when this was not needed.</returns>
        public bool Finish()
        {
            bool result = false;

            if (binArchive != null && this.appendCall > 0)
            {
                // We can crypt the firmware file, and we cannot crypt it. Supply them both and see if you can get back the same thing twice, would be great.
                File.Delete(this.archiveFilename);
                using (var fileStream = File.Create(this.archiveFilename))
                {
                    ProtoBuf.Serializer.Serialize(fileStream, binArchive);
                }

                this.appendCall = 0;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Calculates the CRC-32 over an byte array.
        /// </summary>
        /// <param name="data">The byte[] that you want to calculate the CRC-32 on</param>
        /// <returns>A 4 bytes large CRC value.</returns>
        public static UInt32 calculateCrc32(byte[] data)
        {
            return Crc32.Compute(data);
        }

        /// <summary>
        /// Calculates the SHA-1 over an byte array.
        /// </summary>
        /// <param name="data">The byte[] that you want to calculate the checksum for.</param>
        /// <returns>A byte[] with the 20 bytes large checksum.</returns>
        public static byte[] calculateSha1(byte[] data)
        {
            var sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

    }
}

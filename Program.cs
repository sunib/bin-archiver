using BinArchiver.Version;
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
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world! This is version: {0}", GitVersionRetreiver.getVersion());

            // TODO: Create something to read the arguments 
            calculateCrc32(@"d:\temp\28.bin");
            CreateArchive(@"d:\temp\28.bin");
            TestArchive(); 
            
            Console.ReadKey();
        }

        public static UInt32 calculateCrc32(string filename)
        {
            Crc32 crc32 = new Crc32();
            UInt32 result;

            using (FileStream fs = File.Open(filename, FileMode.Open)) {
                var hashBytes = crc32.ComputeHash(fs);
                if (hashBytes.Length != 4)
                    throw new InvalidProgramException("Whoops, crc32.ComputeHash() did not returned 4 bytes. I thought that it always would!");

                // Reverse the bytes if needed, ComputeHash() returns big endian!
                if (BitConverter.IsLittleEndian)
                {
                    hashBytes = hashBytes.Reverse().ToArray();
                }
                
                result = BitConverter.ToUInt32(hashBytes, 0);
            }

            Console.WriteLine("CRC-32 is {0:x}", result);
            return result;
        }

        public static UInt32 calculateCrc32(byte[] file)
        {
            return Crc32.Compute(file);
        }


        public static byte[] calculateSha1(byte[] file) 
        {
            var sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(file);     // Should always be 20 bytes...
        }

        public static void CreateArchive(string filename)
        {
            // .ifa instead of .pb -> tar tape archive -> par -> pba -> proto buffer archive -> far firmware archive
            // iar, far, sar, bar, 

            // The output.cs is generated from the .proto file (should include this in the build steps).
            // Copy the .proto to D:\git\protobuf-net\ProtoGen\bin\Debug>protogen
            // Run this command: D:\git\protobuf-net\ProtoGen\bin\Debug>protogen -i:bin_archive.proto -ns:BinArchiver -o:GeneratedProtocolBufferClasses.cs
            // Now copy output.cs to this local project. Will do for the moment.

            var firmwareArchive = new BinArchive();
            firmwareArchive.version = "33-3-dirty"; // Overall test value. Also include a comment or something?

            // Now add an example file as well. In the end this is included in the command line arguemnts. 
            // firmware-archiver simon.bin 33-3            

            var binFile = new BinFile()
            {
                version = "33-3-dirty",
                content = File.ReadAllBytes(filename)   // TODO: Will this always fit in memory? For now we just use very simple and small files.              
            };

            binFile.typeValue.Add(1);  // We can't add this in the object initialiser because it's fixed in the generated code.
            binFile.size = (uint)binFile.content.Length;

            // Calculate the checksum in a proper way for our file data. We now that we should place 20 bytes into our test file!
            binFile.check = 3141592653;
            binFile.crc = calculateCrc32(binFile.content);
            binFile.sha1 = calculateSha1(binFile.content);     // Should always be 20 bytes...

            firmwareArchive.binFiles.Add(binFile);

            var encrypter = new BinFileCrypter(StringToByteArray("76355839cbf628b587afef5fca5c9823402d093b78ecf483fbdfb9efe1814e92"));
            var encryptedFirmwareFile = encrypter.convert(binFile);  // What are we going to do with the spare bytes? We should save the length as well...

            firmwareArchive.cryptedBinFiles.Add(encryptedFirmwareFile);

            

            // We can crypt the firmware file, and we cannot crypt it. Supply them both and see if you can get back the same thing twice, would be great.
            using (var file = File.Create("test1.efa"))
            {
                Serializer.Serialize(file, firmwareArchive);
            }
        }

        public static void TestArchive()
        {
            using (var fileStream = File.OpenRead("test1.efa"))
            {
                var test = Serializer.Deserialize<BinArchive>(fileStream);

                // Do a little work to decrypt our current thingy...
                var encrypter = new BinFileCrypter(StringToByteArray("76355839cbf628b587afef5fca5c9823402d093b78ecf483fbdfb9efe1814e92"));
                var decryptedFirmwareFile = encrypter.Decrypt(test.cryptedBinFiles[0]);

                Console.WriteLine(test);
                Console.WriteLine(decryptedFirmwareFile);

            }
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }

    }
}

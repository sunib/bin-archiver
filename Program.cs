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
            CreateArchive();
            TestArchive(); 
            
            Console.ReadKey();
        }

        public static void CreateArchive()
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

            var firmwareFile = new BinFile()
            {
                version = "33-3-dirty",
                size = 8,
                content = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }
            };

            firmwareFile.typeValue.Add(1);  // We can't add this in the object initialiser because it's fixed in the generated code.

            // Calculate the checksum in a proper way for our file data. We now that we should place 20 bytes into our test file!
            var sha = new SHA1CryptoServiceProvider();
            firmwareFile.sha1 = sha.ComputeHash(firmwareFile.content);     // Should always be 20 bytes...

            firmwareArchive.binFiles.Add(firmwareFile);

            var encrypter = new BinFileCrypter(StringToByteArray("76355839cbf628b587afef5fca5c9823402d093b78ecf483fbdfb9efe1814e92"));
            var encryptedFirmwareFile = encrypter.convert(firmwareFile);  // What are we going to do with the spare bytes? We should save the length as well...

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

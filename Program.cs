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
            // TODO: Add extra option so that the checksums become invalid? Or that you can overwrite them with a own value?
            bool result = true;

            if (args.Length < 2)
            {
                Console.WriteLine("bin-archiver version {0}", GitVersionRetreiver.getVersion());
                Console.WriteLine("Archives one or more binary files into a single file. You can find help and usage examples on: https://github.com/Sunib/bin-archiver");
            }
            else
            {   
                // Using enumerator so that we can read more parameters when we need them inside the loop.
                var enumerator = args.Cast<string>().GetEnumerator();
                ArchiveAppender archiveAppender = new ArchiveAppender();
                string archiveFilename = null;
                result = Process(enumerator, out archiveFilename, archiveAppender);
                
                if (result)
                {
                    if (archiveAppender.Finish())
                    {
                        Console.WriteLine("Succesfully written bytes to {0}", archiveFilename);
                    }
                }
                else
                {
                    Console.WriteLine("Program terminating.");
                }
            }
        }

        static bool Process(IEnumerator<string> parameters, out string archiveFilename, ArchiveAppender archiveAppender)
        {
            bool fileExists = false;
            bool result = true;
            archiveFilename = null;

            while (parameters.MoveNext() && result)
            {
                var arg = parameters.Current;

                if (archiveFilename == null)
                {
                    // Check if the file exists and then start using it!
                    archiveFilename = arg.ToString();
                    fileExists = File.Exists(archiveFilename);
                }
                else
                {
                    if (arg == "-c")
                    {
                        if (parameters.MoveNext())
                        {
                            if (!fileExists)
                            {
                                if (CreateArchive(archiveFilename, parameters.Current))
                                {
                                    fileExists = true;
                                    archiveAppender.Prepare(archiveFilename);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to create archive for unknown reason.");
                                    result = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Cannot create archive, file already exists!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to create archive, version string not given.");
                            result = false;
                        }
                    }
                    else if (arg == "-o")
                    {
                        if (fileExists) 
                        {
                            File.Delete(archiveFilename);
                            fileExists = File.Exists(archiveFilename);

                            if (fileExists)
                            {
                                Console.WriteLine("Failed to remove file, locked?");
                                result = false;
                            }
                        }                        
                    }
                    else if (arg == "-k")
                    {
                        if (parameters.MoveNext())
                        {
                            archiveAppender.Key = Convert.FromBase64String(parameters.Current);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else if (arg == "-r")
                    {
                        if (parameters.MoveNext())
                        {
                            archiveAppender.RestrictToValue = Convert.ToUInt32(parameters.Current);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else if (arg == "-a")
                    {
                        if (parameters.MoveNext())
                        {
                            archiveAppender.Filename = parameters.Current;
                            if (parameters.MoveNext())
                            {
                                archiveAppender.Version = parameters.Current;

                                // Read the type values from the command line until you can't parse it anymore.
                                archiveAppender.TypeValues = new List<UInt32>();
                                bool isReading = true;
                                while (isReading && parameters.MoveNext())
                                {
                                    UInt32 value;
                                    if (UInt32.TryParse(parameters.Current, out value))
                                    {
                                        archiveAppender.TypeValues.Add(value);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Multiple actions in one go not yet supported.");
                                        archiveAppender.TypeValues.Clear();
                                        isReading = false;
                                        result = false;
                                    }
                                }

                                if (archiveAppender.TypeValues.Count > 0)
                                {
                                    archiveAppender.Prepare(archiveFilename);
                                    if (archiveAppender.Append())
                                    {
                                        Console.WriteLine("Appended {0} to {1}", archiveAppender.Filename, archiveFilename);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Failed to append {0} to {1}", archiveAppender.Filename, archiveFilename);
                                        result = false;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Append failed, no valid type values found");
                                    result = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Append failed, version not given");
                                result = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Append failed, filename not given");
                            result = false;
                        }
                    }
                    else if (arg == "-d")
                    {
                        TestArchive(archiveFilename, archiveAppender.Key);
                    }
                    else
                    {
                        Console.WriteLine("Unknown/invalid input parameter {0}", arg);
                    }
                }
            }

            return result;
        }

        public static bool CreateArchive(string filename, string version)
        {
            // The GeneratedPorotocolBufferClasses.cs is generated from the .proto file (should include this in the build steps).
            // Copy the .proto to D:\git\protobuf-net\ProtoGen\bin\Debug
            // Run this command: D:\git\protobuf-net\ProtoGen\bin\Debug>protogen -i:bin_archive.proto -ns:BinArchiver -o:GeneratedProtocolBufferClasses.cs
            // Now copy output.cs to this local project. Will do for the moment.

            var binArchive = new BinArchive();
            binArchive.version = version;
            binArchive.time = Helpers.convertToUnixTimeMilliseconds(DateTime.UtcNow);

            // We can crypt the firmware file, and we cannot crypt it. Supply them both and see if you can get back the same thing twice, would be great.
            using (var file = File.Create(filename))
            {
                Serializer.Serialize(file, binArchive);
            }          

            return true;
        }

        public static void TestArchive(string filename, byte[] key = null)
        {
            // Prepare our encryption object, if we have a key.
            BinFileCrypter binFileCrypter = null;
            if (key != null)
            {
                binFileCrypter = new BinFileCrypter(key);
            }

            // Start reading the file!
            using (var fileStream = File.OpenRead(filename))
            {
                Console.WriteLine("Loading and parsing {0} (size={1})", filename, fileStream.Length);
                var binArchive = Serializer.Deserialize<BinArchive>(fileStream);
                Console.WriteLine("Completed bin-archive parse, contains {0} normal files and {1} crypted files.", binArchive.binFiles.Count, binArchive.cryptedBinFiles.Count);
                Console.WriteLine("Version={0}", binArchive.version);
                Console.WriteLine("Time={0}", Helpers.convertUnixTimeMillisecondsToDateTime(binArchive.time).ToString());
                Console.WriteLine("");

                foreach (var binFile in binArchive.binFiles)
                {
                    PrintBinFile(binFile);
                }

                foreach (var cryptedBinFile in binArchive.cryptedBinFiles)
                {
                    Console.WriteLine("Found crypted file (restrictToValue={0}, IV={1}, size={2}), trying to decrypt...", 
                        cryptedBinFile.restrictToValue, 
                        Convert.ToBase64String(cryptedBinFile.aesInitializationVector),
                        cryptedBinFile.cryptedBinFile.Length);

                    if (binFileCrypter != null)
                    {
                        var binFile = binFileCrypter.Decrypt(cryptedBinFile);
                        if (binFile != null)
                        {
                            PrintBinFile(binFile);
                        }
                        else
                        {
                            Console.WriteLine("Failed to decrypt.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Decrypt failed: No valid -k supplied, ");
                    }
                }
            }
        }

        /// <summary>
        /// Prints some debug info about a bin file.
        /// </summary>
        public static void PrintBinFile(BinFile binFile)
        {
            Console.WriteLine("Valid BinFile found (version={0}, size={1}, time={2})", 
                binFile.version,
                binFile.size,
                Helpers.convertUnixTimeMillisecondsToDateTime(binFile.time).ToString());

            Console.WriteLine("File can be applied to type values: ");
            binFile.typeValues.ForEach(v => Console.Write("{0} ", v));
            Console.Write("\n");
            
            var savedCrc = binFile.crc;
            var calculatedCrc = ArchiveAppender.calculateCrc32(binFile.content);

            var savedSha = Convert.ToBase64String(binFile.sha1);
            var calculatedSha = Convert.ToBase64String(ArchiveAppender.calculateSha1(binFile.content));

            Console.WriteLine("Hashes: CRC-32={0}, SHA-1={1}, Check={2}", 
                savedCrc, 
                savedSha,
                binFile.check);

            Console.WriteLine("Expected hashes: CRC-32={0}, SHA-1={1}, Check=3141592653",
                calculatedCrc,
                calculatedSha);

            if (savedCrc != calculatedCrc)
            {
                Console.WriteLine("CRC ERROR!!!");
            }

            if (savedSha != calculatedSha)
            {
                Console.WriteLine("SHA1 ERROR!!!");
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

        /// <summary>
        /// Test function that reads a complete file and calculates and prints it's CRC.
        /// </summary>
        /// <param name="filename">Path to the filename that you want to read.</param>
        /// <returns>The calculated CRC-32</returns>
        public static UInt32 calculateCrc32(string filename)
        {
            Crc32 crc32 = new Crc32();
            UInt32 result;

            using (FileStream fs = File.Open(filename, FileMode.Open))
            {
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
    }
}

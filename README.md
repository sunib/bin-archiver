bin-archiver
============

Archives one or more binary files into a single file. A file may be encrypted and always includes a version string and one or more `type-value` numbers. Primary intention is a flexible firmware update mechanism that only requires one file. 

The tool uses [protocol buffers](https://github.com/google/protobuf/) to write and read to the archive. The tool is written as a C# command line application. So it's easy to use in an automated environment, like a build server.

Building
--------

The tool can be compiled with visual studio 2013. It depends on the very nice [protobuf-net](https://code.google.com/p/protobuf-net/) library, NuGet should automatically download it when you try to build the project.

I did not placed a compiled version on the internet, if you would like one please let me know.

Encoding files
--------------

The default extension is `efa` (short for: enhanced firmware archive). Other extensions are possible, just append them to the file.

Creating a new empty archive:

bin-archiver.exe -c `archive-name` `archive-version-string`

Please note that the archive version string is never encrypted, the files that you place into the archive may be.

Append an unencrpyted binary file to the archive:

> bin-archiver.exe -a `archive-name` `some-file` `some-file-version-string` `type-value`

The `type-value` is an unsigned 32 bits value that you can use to identify the file that you added. In most situations you would define an enumerator that defines the values. You may add multiple `type-value` numbers if you would like to. This could be easy if you don't want to save the same binary multiple times. This approach gives you the freedom to eventually distribute another binary for one of the added type values in a later file.

So let's do an example. Let's say that I wanted to create an archive that contains a bootloader and a firmware. Together they form version 1.0. I would execute the following commands to make this happen:

> bin-archiver.exe -c "update.efa" "1.0"
> bin-archiver.exe -a "update.efa" "bootloader.bin" "1.0" 1
> bin-archiver.exe -a "update.efa" "firmware.bin" "23" 2

Overall hint: It's comfortable to add the location of bin-archiver.exe to your PATH variable, this allows you to access the tool from everywhere.

Encrpytion
----------

You can also append a file encrypted, just add the extra argument to -a argument.
> -k `key` 

AES is used for encryption, your `key` has to be 128, 192 or 256 bits big (16, 24, 32 bytes respectively). The supplied key must be encoded in [Base64](http://en.wikipedia.org/wiki/Base64). The initialisation vector is automatically generated (random) and added to the archive file, so you only need to 'remember' your secret key in order to decode the file.

You can (optionally!) restrict the encrypted file to entities with a certain value. This command replaces the last 4 bytes of the randomly generated AES initialisation vector with the given unsigned 32 bit `restrict-to-value`:
> -r `restrict-to-value`

You may append encrypted and unecrypted files if you wish, you can even add different restricted versions.

The receiving party should do exeactly the same if `restrict-to-value` is found in the file. Don't add the -r option if you don't want to restrict, the file can then be read by everbody who knows the given `key`.

Decoding files
--------------

The tool is primary intented to encode files, decoding is in the current use case done on the receiving side in an (very nice!) [embedded library](http://koti.kapsi.fi/jpa/nanopb/). I might add full decoding options to this tool later.

For now you can use the -d command to show information about an archive. You will get more informaton if you add the encryption parameters `key` and `restrict-to-value` as described in the previous paragraph. Without these you will see what someone can extract from the file without this information.

Future
------

Just a simple tool, we will see where a ends!

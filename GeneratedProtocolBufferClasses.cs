//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: bin_archive.proto
namespace BinArchiver
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"BinArchive")]
  public partial class BinArchive : global::ProtoBuf.IExtensible
  {
    public BinArchive() {}
    
    private string _version;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"version", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string version
    {
      get { return _version; }
      set { _version = value; }
    }
    private readonly global::System.Collections.Generic.List<BinFile> _binFiles = new global::System.Collections.Generic.List<BinFile>();
    [global::ProtoBuf.ProtoMember(2, Name=@"binFiles", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<BinFile> binFiles
    {
      get { return _binFiles; }
    }
  
    private readonly global::System.Collections.Generic.List<CryptedBinFile> _cryptedBinFiles = new global::System.Collections.Generic.List<CryptedBinFile>();
    [global::ProtoBuf.ProtoMember(3, Name=@"cryptedBinFiles", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<CryptedBinFile> cryptedBinFiles
    {
      get { return _cryptedBinFiles; }
    }
  
    private ulong _time = default(ulong);
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"time", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(ulong))]
    public ulong time
    {
      get { return _time; }
      set { _time = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"BinFile")]
  public partial class BinFile : global::ProtoBuf.IExtensible
  {
    public BinFile() {}
    
    private string _version = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"version", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string version
    {
      get { return _version; }
      set { _version = value; }
    }
    private readonly global::System.Collections.Generic.List<uint> _typeValues = new global::System.Collections.Generic.List<uint>();
    [global::ProtoBuf.ProtoMember(2, Name=@"typeValues", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<uint> typeValues
    {
      get { return _typeValues; }
    }
  
    private uint _check;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"check", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint check
    {
      get { return _check; }
      set { _check = value; }
    }
    private uint _crc;
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"crc", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint crc
    {
      get { return _crc; }
      set { _crc = value; }
    }
    private byte[] _sha1;
    [global::ProtoBuf.ProtoMember(7, IsRequired = true, Name=@"sha1", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] sha1
    {
      get { return _sha1; }
      set { _sha1 = value; }
    }
    private ulong _time = default(ulong);
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"time", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(ulong))]
    public ulong time
    {
      get { return _time; }
      set { _time = value; }
    }
    private uint _size;
    [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name=@"size", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint size
    {
      get { return _size; }
      set { _size = value; }
    }
    private byte[] _content;
    [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name=@"content", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] content
    {
      get { return _content; }
      set { _content = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CryptedBinFile")]
  public partial class CryptedBinFile : global::ProtoBuf.IExtensible
  {
    public CryptedBinFile() {}
    
    private byte[] _aesInitializationVector;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"aesInitializationVector", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] aesInitializationVector
    {
      get { return _aesInitializationVector; }
      set { _aesInitializationVector = value; }
    }
    private uint _restrictToValue = default(uint);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"restrictToValue", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(uint))]
    public uint restrictToValue
    {
      get { return _restrictToValue; }
      set { _restrictToValue = value; }
    }
    private byte[] _cryptedBinFile;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"cryptedBinFile", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] cryptedBinFile
    {
      get { return _cryptedBinFile; }
      set { _cryptedBinFile = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
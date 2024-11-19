class TempFile : IDisposable
{
    public string FilePath { get; private set; }
    public FileStream FileStream{ 
        get{
            if(_fileStream == null)
                _fileStream = OpenStream();
            return _fileStream;
        } 
    }
    private FileStream? _fileStream = null;
    public bool Readonly { get; private set; } = false;

    public TempFile(string? extention = null){
        FilePath = Path.GetTempFileName(); 
        if(extention != null)
            FilePath += "." + extention;
    }

    public TempFile(Stream baseStream, bool writable = false, string? extention = null){
        FilePath = Path.GetTempFileName();
        if(extention != null)
            FilePath += "." + extention;

        using (var newTmp = File.Create(FilePath)){
            baseStream.CopyTo(newTmp);
        }
        Readonly = !writable;
    }

    private FileStream OpenStream(){
        return File.Open(
            FilePath,
            FileMode.OpenOrCreate,
            Readonly ? FileAccess.Read : FileAccess.ReadWrite
        );
    }

    public void Dispose()
    {
        if(_fileStream != null)
            FileStream.Close();
        File.Delete(FilePath);
    }
}
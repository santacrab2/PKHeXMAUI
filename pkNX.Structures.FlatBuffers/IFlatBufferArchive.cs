namespace pkNX.Structures.FlatBuffers;

public interface IFlatBufferArchive<T> where T : class
{
    IList<T> Table { get; set; }
}

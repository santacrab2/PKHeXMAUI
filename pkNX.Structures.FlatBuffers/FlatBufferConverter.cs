

using FlatSharp;

namespace pkNX.Structures.FlatBuffers;

public static class FlatBufferConverter
{

    
    public static byte[][] SerializeFrom<T>(T[] obj) where T : class, IFlatBufferSerializable<T>, new()
    {
        var result = new byte[obj.Length][];
        for (int i = 0; i < result.Length; i++)
        {
            var file = obj[i];
            result[i] = SerializeFrom(file);
        }
        return result;
    }

    public static T DeserializeFrom<T>(byte[] data) where T : class, IFlatBufferSerializable<T>
    {

        return T.LazySerializer.Parse<T>(data);
        
    }
    
    public static byte[] SerializeFrom<T>(T obj) where T : class, IFlatBufferSerializable<T>, new()
    {
        var test = new T();
        var size = test.Serializer.GetMaxSize(obj);
        var data = new byte[size];
        var result = test.Serializer.Write(data,obj);
        if (result != data.Length)
            Array.Resize(ref data, result);
        return data;
    }
}

using System.IO;

namespace AdvancedEdit.Serialization;

public class BinaryDeserializer(BinaryReader reader)
{
    public BinaryReader Reader { get; private set; } = reader;

    public T Deserialize<T>() where T : struct, ISerializable
    {
        var temp = new T();
        temp.Deserialize(Reader);
        return temp;
    }
    public T DeserializeAt<T>(uint position) where T : struct, ISerializable
    {
        Reader.BaseStream.Position = position;
        var temp = new T();
        temp.Deserialize(Reader);
        return temp;
    }

    public T[] DeserializeArrayAt<T>(uint position, uint count) where T : struct, ISerializable
    {
        Reader.BaseStream.Position = position;
        T[] temp = new T[count];
        for (int i = 0; i < count; i++)
        {
            temp[i] = Deserialize<T>();
        }
        return temp;
    }
}

public class BinarySerializer(BinaryWriter writer)
{
    public BinaryWriter Writer { get; private set; } = writer;

    public T DeserializeAt<T>(uint position) where T : struct, ISerializable
    {
        Writer.BaseStream.Position = position;
        var temp = new T();
        temp.Serialize(Writer);
        return temp;
    }
}

public interface ISerializable
{
    public void Serialize(BinaryWriter writer);
    public void Deserialize(BinaryReader reader);
}
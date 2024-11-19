//#define     CONSOLE_LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace NetBase
{

    public struct PacketHeader
    {
        public ushort Size;
        public ushort Type;

        public void Serialize(PacketBase packet)
        {
            packet.Write(Size);
            packet.Write(Type);
        }

        public void Deserialize(PacketBase packet)
        {
            Size = packet.Read<ushort>();
            Type = packet.Read<ushort>();
        }

        public const int HeaderSize = 4; // 2 bytes for PacketSize, 2 bytes for PacketType

        public static PacketHeader FromBytes(byte[] buffer)
        {
            PacketHeader header = new PacketHeader
            {
                Size = BitConverter.ToUInt16(buffer, 0),
                Type = BitConverter.ToUInt16(buffer, 2)
            };
            return header;
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[HeaderSize];
            BitConverter.GetBytes(Size).CopyTo(buffer, 0);
            BitConverter.GetBytes(Type).CopyTo(buffer, 2);
            return buffer;
        }

    }
    public interface IPacketSerializable
    {
        void Serialize(PacketBase packet);
        void Deserialize(PacketBase packet);
    }

    public class PacketBase
    {
        private MemoryStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;

        public PacketBase()
        {
            stream = new MemoryStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }

        public void Write(object value,
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (value == null) return;

            if (value is int)
            {
                writer.Write((int)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((int){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is bool)
            {
                writer.Write((bool)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((bool){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is char)
            {
                writer.Write((char)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((char){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is float)
            {
                writer.Write((float)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((float){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is long)
            {
                writer.Write((long)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((long){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is ushort)
            {
                writer.Write((ushort)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((ushort){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is Enum)
            {
                writer.Write(Convert.ToInt32(value));
#if CONSOLE_LOG
                Console.WriteLine("Write((Enum){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is string)
            {
                writer.Write((string)value);
#if CONSOLE_LOG
                Console.WriteLine("Write((string){0} {1} {2}/{3}", value.ToString(), GetCurrentBufferSize(), sourceFilePath, sourceLineNumber);
#endif
            }
            else if (value is char[])  // char[] 처리 추가
            {
                char[] charArray = (char[])value;
                writer.Write(charArray.Length);
                writer.Write(charArray);
#if CONSOLE_LOG
                Console.WriteLine($"Write((char[]){new string(charArray)}) {GetCurrentBufferSize()} {sourceFilePath}/{sourceLineNumber}");
#endif
            }
            else if (value is IPacketSerializable serializable)
            {
                serializable.Serialize(this);
            }
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (IList)value;
                Int32 count = list.Count;
                writer.Write(count);
#if CONSOLE_LOG
                Console.WriteLine($"Write((int){list.Count}) // List count {GetCurrentBufferSize()} {sourceFilePath}, {sourceLineNumber}");
#endif
                foreach (var item in list)
                {
                    Write(item);
                }
            }
            else if (value is ValueType)  // 값 타입(구조체 포함)에 대한 처리 추가
            {
#if CONSOLE_LOG
                Console.WriteLine($"Write((value){value.ToString()}) // List count {GetCurrentBufferSize()} {sourceFilePath}, {sourceLineNumber}");
#endif
                var fields = value.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    Write(field.GetValue(value));
                }
            }
            else
            {
                throw new ArgumentException("Unsupported type");
            }
        }

        public T Read<T>()
        {
            return (T)ReadObject(typeof(T));
        }

        private object ReadObject(Type type)
        {
            if (type == typeof(int))
            {
                var obj = reader.ReadInt32();
#if CONSOLE_LOG
                Console.WriteLine("Read(({0}){1} {2}", obj.GetType().ToString(), obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(bool))
            {
                var obj = reader.ReadBoolean();
#if CONSOLE_LOG
                Console.WriteLine("Read((bool){0} {1}", obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(char))
            {
                var obj = reader.ReadChar();
#if CONSOLE_LOG
                Console.WriteLine("Read((char){0} {1}", obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(float))
            {
                var obj = reader.ReadSingle();
#if CONSOLE_LOG
                Console.WriteLine("Read(({0}){1} {2}", obj.GetType().ToString(), obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(long))
            {
                var obj = reader.ReadInt64();
#if CONSOLE_LOG
                Console.WriteLine("Read(({0}){1} {2}", obj.GetType().ToString(), obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(ushort))
            {
                var obj = reader.ReadUInt16();
#if CONSOLE_LOG
                Console.WriteLine("Read(({0}){1} {2}", obj.GetType().ToString(), obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type.IsEnum)
            {
                var obj = Enum.ToObject(type, reader.ReadInt32());
#if CONSOLE_LOG
                Console.WriteLine("Read(({0}){1} {2}", obj.GetType().ToString(), obj.ToString(), GetCurrentBufferSize());
#endif
                return obj;
            }
            else if (type == typeof(string))
            {
                var obj = reader.ReadString();
#if CONSOLE_LOG
                Console.WriteLine($"Read((string){obj}) {GetCurrentBufferSize()}");
#endif
                return obj;
            }
            else if (type == typeof(char[]))  // char[] 처리 추가
            {
                int length = reader.ReadInt32();
                var charArray = reader.ReadChars(length);
#if CONSOLE_LOG
                Console.WriteLine($"Read((char[]){new string(charArray)}) {GetCurrentBufferSize()}");
#endif
                return charArray;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                int count = reader.ReadInt32();
#if CONSOLE_LOG
                Console.WriteLine("Read(List Count){0} {1}", count, GetCurrentBufferSize());
#endif
                var list = (IList)Activator.CreateInstance(type);
                var elementType = type.GetGenericArguments()[0];

                for (int i = 0; i < count; i++)
                {
                    list.Add(ReadObject(elementType));
                }

                return list;
            }
            else if (typeof(IPacketSerializable).IsAssignableFrom(type))
            {
                var value = Activator.CreateInstance(type);
                ((IPacketSerializable)value).Deserialize(this);
                return value;
            }
            else if (type.IsValueType)  // 구조체 처리
            {
                var value = Activator.CreateInstance(type);
                foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    field.SetValue(value, ReadObject(field.FieldType));
                }
                return value;
            }
            else
            {
                throw new NotSupportedException($"Cannot read type {type}");
            }
        }

        public byte[] GetPacketData()
        {
            return stream.ToArray();
        }

        public void SetPacketData(byte[] data)
        {
            stream = new MemoryStream(data);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }

        public void ResetStreamPosition()
        {
            stream.Position = 0;
        }
        public int GetCurrentBufferSize()
        {
            return (int)stream.Position;
        }
    }

}

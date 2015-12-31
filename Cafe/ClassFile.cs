using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafe
{
    public class ClassFile
    {
        public uint Magic { get; set; }

        public ushort MinorVersion { get; set; }
        public ushort MajorVersion { get; set; }

        public ConstantPool ConstantPool { get; }

        public ushort AccessFlag { get; set; }
        public ushort ThisClass { get; set; }
        public ushort SuperClass { get; set; }

        public ushort InterfacesCount { get; set; }
        // ??? Interfaces

        public ushort FieldsCount { get; set; }
        // Fields

        public ushort MethodsCount { get; set; }
        // Methods
        
        public ushort AttributesCount { get; set; }
        // Attributes

        public ClassFile()
        {
            ConstantPool = new ConstantPool();
        }
    }

    public class ClassReader
    {
        public ClassFile Read(Stream clasStream)
        {
            using (JavaBinaryReader br = new JavaBinaryReader(clasStream))
            {
                ClassFile cls = new ClassFile();

                cls.Magic = br.ReadUInt32();
                if (cls.Magic != 0xCAFEBABE)
                {
                    throw new InvalidOperationException("Magic value does not ");
                }

                cls.MinorVersion = br.ReadUInt16();
                cls.MajorVersion = br.ReadUInt16();

                int constantCount = br.ReadUInt16();

                for (int i = 0; i < constantCount; i++)
                {
                    ConstantTag tag = (ConstantTag)br.ReadByte();
                    var constant = ReadConstant(br, cls, tag);
                    cls.ConstantPool.Constants.Add(constant);
                }

                return cls;
            }
        }

        private ConstantBase ReadConstant(JavaBinaryReader br, ClassFile cls, ConstantTag tag)
        {
            switch (tag)
            {
                case ConstantTag.Class:
                    return ReadConstantClass(br, cls);
                case ConstantTag.FieldRef:
                    return ReadConstantFieldRef(br, cls);
                case ConstantTag.MethodRef:
                    return ReadConstantMethodRef(br, cls);
                case ConstantTag.InterfaceMethodRef:
                    return ReadConstantInterfaceMethodRef(br, cls);
                case ConstantTag.String:
                    return ReadConstantString(br, cls);
                case ConstantTag.Integer:
                    return ReadConstantInteger(br, cls);
                case ConstantTag.Float:
                    return ReadConstantFloat(br, cls);
                case ConstantTag.Long:
                    return ReadConstantLong(br, cls);
                case ConstantTag.Double:
                    return ReadConstantDouble(br, cls);
                case ConstantTag.NameAndType:
                    return ReadConstantNameAndType(br, cls);
                case ConstantTag.Utf8:
                    return ReadConstantUtf8(br, cls);
                case ConstantTag.MethodHandle:
                    return ReadConstantMethodHandle(br, cls);
                case ConstantTag.MethodType:
                    return ReadConstantMethodType(br, cls);
                case ConstantTag.InvokeDynamic:
                    return ReadConstantInvokeDynamic(br, cls);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
            }
        }

        private ConstantInvokeDynamicInfo ReadConstantInvokeDynamic(JavaBinaryReader br, ClassFile cls)
        {
            int attributeIndex = br.ReadUInt16();
            return new ConstantInvokeDynamicInfo(cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(br.ReadUInt16()));
        }

        private ConstantMethodTypeInfo ReadConstantMethodType(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantMethodTypeInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(br.ReadUInt16()));
        }

        private ConstantMethodHandleInfo ReadConstantMethodHandle(JavaBinaryReader br, ClassFile cls)
        {
            ReferenceKind kind = (ReferenceKind)br.ReadByte();
            return new ConstantMethodHandleInfo(kind, cls.ConstantPool.GetConstant<ConstantBase>(br.ReadUInt16()));
        }

        private ConstantUtf8Info ReadConstantUtf8(JavaBinaryReader br, ClassFile cls)
        {
            int length = br.ReadUInt16();
            var bytes = br.ReadBytes(length);
            return new ConstantUtf8Info(Encoding.UTF8.GetString(bytes));
        }

        private ConstantNameAndTypeInfo ReadConstantNameAndType(JavaBinaryReader br, ClassFile cls)
        {
            int nameIndex = br.ReadUInt16();
            int descriptorIndex = br.ReadUInt16();
            return new ConstantNameAndTypeInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(nameIndex), cls.ConstantPool.GetConstant<ConstantUtf8Info>(descriptorIndex));
        }

        private ConstantDoubleInfo ReadConstantDouble(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantDoubleInfo(BitConverter.ToDouble(br.ReadBytes(8), 0));
        }

        private ConstantLongInfo ReadConstantLong(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantLongInfo(br.ReadInt64());
        }

        private ConstantFloatInfo ReadConstantFloat(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantFloatInfo(BitConverter.ToSingle(br.ReadBytes(4), 0));
        }

        private ConstantIntegerInfo ReadConstantInteger(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantIntegerInfo(br.ReadInt32());
        }

        private ConstantStringInfo ReadConstantString(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantStringInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(br.ReadUInt16()));
        }

        private ConstantInterfaceMethodRefInfo ReadConstantInterfaceMethodRef(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantInterfaceMethodRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(br.ReadUInt16()), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(br.ReadUInt16()));
        }

        private ConstantMethodRefInfo ReadConstantMethodRef(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantMethodRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(br.ReadUInt16()), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(br.ReadUInt16()));
        }

        private ConstantFieldRefInfo ReadConstantFieldRef(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantFieldRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(br.ReadUInt16()), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(br.ReadUInt16()));
        }

        private ConstantClassInfo ReadConstantClass(JavaBinaryReader br, ClassFile cls)
        {
            return new ConstantClassInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(br.ReadUInt16()));
        }
    }

    public class ClassWriter
    {
    }

    public static class DescriptorParser
    {
        private static FieldType ParseFieldType(string descriptor, int startIndex, out int stopIndex)
        {
            stopIndex = startIndex + 1;
            switch (descriptor[startIndex])
            {
                case 'B':
                    return new FieldType(NativeType.Byte);
                case 'C':
                    return new FieldType(NativeType.Char);
                case 'D':
                    return new FieldType(NativeType.Double);
                case 'F':
                    return new FieldType(NativeType.Float);
                case 'I':
                    return new FieldType(NativeType.Int);
                case 'J':
                    return new FieldType(NativeType.Long);
                case 'L':
                    int separatorPos = descriptor.IndexOf(";", startIndex);
                    string className = descriptor.Substring(startIndex + 1, separatorPos - startIndex - 1);
                    stopIndex = separatorPos + 1;
                    return new ObjectType(className);
                case 'S':
                    return new FieldType(NativeType.Short);
                case 'Z':
                    return new FieldType(NativeType.Boolean);
                case '[':
                    return new ArrayType(ParseFieldType(descriptor, startIndex + 1, out stopIndex));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static FieldType ParseFieldType(string descriptor)
        {
            int startIndex = 0;
            int stopIndex;
            return ParseFieldType(descriptor, startIndex, out stopIndex);
        }

        public static MethodDescriptor ParseMethodDescriptor(string descriptor)
        {
            FieldType ret = new FieldType(NativeType.Void);
            List<FieldType> parameters = new List<FieldType>();

            if (descriptor[0] != '(')
            {
                throw new ArgumentException("Descriptor does not start with '('");
            }

            int index = 1;
            int endIndex = -1;
            while (index < descriptor.Length && descriptor[index] != ')')
            {
                var parameter = ParseFieldType(descriptor, index, out endIndex);
                parameters.Add(parameter);
                index = endIndex;
            }

            if (index >= descriptor.Length)
            {
                throw new ArgumentException("Descriptor is not complete and does not contain ')'");
            }

            index++;
            if (index >= descriptor.Length)
            {
                throw new ArgumentException("Descriptor is not complete and does not contain a return value");
            }

            if (descriptor[index] != 'V')
            {
                ret = ParseFieldType(descriptor, index, out endIndex);
            }

            return new MethodDescriptor(parameters.ToArray(), ret);
        }
    }

    public enum NativeType
    {
        Byte,
        Char,
        Double,
        Float,
        Int,
        Long,
        ClassReference,
        Short,
        Boolean,
        Array,
        Void
    }

    public class FieldType
    {
        public NativeType NativeType { get; set; }

        public FieldType(NativeType type)
        {
            NativeType = type;
        }
    }

    public class ObjectType : FieldType
    {
        public string ClassName { get; set; }

        public ObjectType(string className) : base(NativeType.ClassReference)
        {
            ClassName = className;
        }
    }

    public class ArrayType : FieldType
    {
        public FieldType InnerType { get; set; }

        public ArrayType(FieldType innerType) : base(NativeType.Array)
        {
            InnerType = innerType;
        }
    }

    public class MethodDescriptor
    {
        public FieldType ReturnType { get; set; }

        public FieldType[] ParameterTypes { get; set; }

        public MethodDescriptor(FieldType[] parameterTypes, FieldType returnType)
        {
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }
    }

    public enum ConstantTag
    {
        Class = 7,
        FieldRef = 9,
        MethodRef = 10,
        InterfaceMethodRef = 11,
        String = 8,
        Integer = 3,
        Float = 4,
        Long = 5,
        Double = 6,
        NameAndType = 12,
        Utf8 = 1,
        MethodHandle = 15,
        MethodType = 16,
        InvokeDynamic = 18
    }

    public class ConstantPool
    {
        public List<ConstantBase> Constants { get; }

        public ConstantPool()
        {
            Constants = new List<ConstantBase>();
        }

        public T GetConstant<T>(int index) where T : ConstantBase
        {
            return Constants[index - 1] as T;
        }

        public int GetIndex(ConstantBase constant)
        {
            return Constants.IndexOf(constant);
        }
    }

    public abstract class ConstantBase
    {
        public ConstantTag Tag { get; set; }

        public ConstantBase(ConstantTag tag)
        {
            Tag = tag;
        }
    }

    public class ConstantClassInfo : ConstantBase
    {
        public ConstantUtf8Info Name { get; set; }

        public ConstantClassInfo(ConstantUtf8Info name) : base(ConstantTag.Class)
        {
            Name = name;
        }
    }

    public class ConstantFieldRefInfo : ConstantBase
    {
        public ConstantClassInfo Class { get; set; }
        public ConstantNameAndTypeInfo NameAndType { get; set; }

        public ConstantFieldRefInfo(ConstantClassInfo cls, ConstantNameAndTypeInfo nameAndType) : base(ConstantTag.FieldRef)
        {
            Class = cls;
            NameAndType = nameAndType;
        }
    }

    public class ConstantMethodRefInfo : ConstantBase
    {
        public ConstantClassInfo Class { get; set; }
        public ConstantNameAndTypeInfo NameAndType { get; set; }

        public ConstantMethodRefInfo(ConstantClassInfo cls, ConstantNameAndTypeInfo nameAndType) : base(ConstantTag.MethodRef)
        {
            Class = cls;
            NameAndType = nameAndType;
        }
    }

    public class ConstantInterfaceMethodRefInfo : ConstantBase
    {
        public ConstantClassInfo Class { get; set; }
        public ConstantNameAndTypeInfo NameAndType { get; set; }

        public ConstantInterfaceMethodRefInfo(ConstantClassInfo cls, ConstantNameAndTypeInfo nameAndType) : base(ConstantTag.InterfaceMethodRef)
        {
            Class = cls;
            NameAndType = nameAndType;
        }
    }


    public class ConstantStringInfo : ConstantBase
    {
        public ConstantUtf8Info Value { get; set; }

        public ConstantStringInfo(ConstantUtf8Info value) : base(ConstantTag.String)
        {
            Value = value;
        }
    }


    public class ConstantIntegerInfo : ConstantBase
    {
        public int Value { get; set; }

        public ConstantIntegerInfo(int value) : base(ConstantTag.Integer)
        {
            Value = value;
        }
    }


    public class ConstantFloatInfo : ConstantBase
    {
        public float Value { get; set; }

        public ConstantFloatInfo(float value) : base(ConstantTag.Float)
        {
            Value = value;
        }
    }

    public class ConstantLongInfo : ConstantBase
    {
        public long Value { get; set; }

        public ConstantLongInfo(long value) : base(ConstantTag.Long)
        {
            Value = value;
        }
    }

    public class ConstantDoubleInfo : ConstantBase
    {
        public double Value { get; set; }

        public ConstantDoubleInfo(double value) : base(ConstantTag.Double)
        {
            Value = value;
        }
    }

    public class ConstantNameAndTypeInfo : ConstantBase
    {
        public ConstantUtf8Info Name { get; set; }
        public ConstantUtf8Info Descriptor { get; set; }

        public ConstantNameAndTypeInfo(ConstantUtf8Info name, ConstantUtf8Info descriptor) : base(ConstantTag.NameAndType)
        {
            Name = name;
            Descriptor = descriptor;
        }
    }

    public class ConstantUtf8Info : ConstantBase
    {
        public string Value { get; set; }

        public ConstantUtf8Info(string value) : base(ConstantTag.Utf8)
        {
            Value = value;
        }
    }

    public enum ReferenceKind
    {
        GetField = 1,
        GetStatic = 2,
        PutField = 3,
        PutStatic = 4,
        InvokeVirtual = 5,
        NewInvokeSpecial = 8,
        InvokeStatic = 6,
        InvokeSpecial = 7,
        InvokeInterface = 9,
    }

    public class ConstantMethodHandleInfo : ConstantBase
    {
        public ReferenceKind ReferenceKind { get; set; }

        public ConstantBase Reference { get; set; }

        public ConstantMethodHandleInfo(ReferenceKind kind, ConstantBase reference) : base(ConstantTag.MethodHandle)
        {
            ReferenceKind = kind;
            Reference = reference;
        }
    }

    public class ConstantMethodTypeInfo : ConstantBase
    {
        public ConstantUtf8Info Descriptor { get; set; }

        public ConstantMethodTypeInfo(ConstantUtf8Info descriptor) : base(ConstantTag.MethodType)
        {
            Descriptor = descriptor;
        }
    }

    public class ConstantInvokeDynamicInfo : ConstantBase
    {
        //TODO: bootstrap_method_attr_index

        public ConstantNameAndTypeInfo NameAndType { get; set; }

        public ConstantInvokeDynamicInfo(ConstantNameAndTypeInfo nameAndType) : base(ConstantTag.InvokeDynamic)
        {
            NameAndType = nameAndType;
        }
    }
}

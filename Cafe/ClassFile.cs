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

        public AccessFlag AccessFlag { get; set; }
        public ConstantClassInfo ThisClass { get; set; }
        public ConstantClassInfo SuperClass { get; set; }

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
                    throw new InvalidOperationException("Magic value is not valid");
                }

                cls.MinorVersion = br.ReadUInt16();
                cls.MajorVersion = br.ReadUInt16();

                int constantCount = br.ReadUInt16();
                for (int i = 1; i < constantCount; i++)
                {
                    
                    ConstantTag tag = (ConstantTag) br.ReadByte();
                    ConstantBase constant = ReadConstant(br, cls, tag);
                    cls.ConstantPool.Constants.Add(constant);

                    if (constant.Tag == ConstantTag.Double || constant.Tag == ConstantTag.Long)
                    {
                        // If the constant is a double or a long, it takes two slots
                        cls.ConstantPool.Constants.Add(new ConstantInvalidInfo());
                        i++;
                    }
                }

                cls.AccessFlag = (AccessFlag)br.ReadUInt16();

                int thisClassIndex = br.ReadUInt16();
                int superClassIndex = br.ReadUInt16();

                cls.ThisClass = cls.ConstantPool.GetConstant<ConstantClassInfo>(thisClassIndex);
                if (superClassIndex != 0)
                {
                    cls.SuperClass = cls.ConstantPool.GetConstant<ConstantClassInfo>(superClassIndex);
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
            int index = br.ReadUInt16();
            return new ConstantInvokeDynamicInfo(cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(index));
        }

        private ConstantMethodTypeInfo ReadConstantMethodType(JavaBinaryReader br, ClassFile cls)
        {
            int index = br.ReadUInt16();
            return new ConstantMethodTypeInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(index));
        }

        private ConstantMethodHandleInfo ReadConstantMethodHandle(JavaBinaryReader br, ClassFile cls)
        {
            ReferenceKind kind = (ReferenceKind)br.ReadByte();
            int index = br.ReadUInt16();
            return new ConstantMethodHandleInfo(kind, cls.ConstantPool.GetConstant<ConstantBase>(index));
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
            var high = br.ReadUInt32();
            var low = br.ReadUInt32();
            long l = ((long)high << 32) + low;
            double val = BitConverter.Int64BitsToDouble(l);
            return new ConstantDoubleInfo(val);
        }

        private ConstantLongInfo ReadConstantLong(JavaBinaryReader br, ClassFile cls)
        {
            var high = br.ReadUInt32();
            var low = br.ReadUInt32();
            long val = ((long) high << 32) + low;
            return new ConstantLongInfo(val);
        }

        private ConstantFloatInfo ReadConstantFloat(JavaBinaryReader br, ClassFile cls)
        {
            float val = BitConverter.ToSingle(br.ReadBytes(4), 0);
            return new ConstantFloatInfo(val);
        }

        private ConstantIntegerInfo ReadConstantInteger(JavaBinaryReader br, ClassFile cls)
        {
            int val = br.ReadInt32();
            return new ConstantIntegerInfo(val);
        }

        private ConstantStringInfo ReadConstantString(JavaBinaryReader br, ClassFile cls)
        {
            int stringIndex = br.ReadUInt16();
            return new ConstantStringInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(stringIndex));
        }

        private ConstantInterfaceMethodRefInfo ReadConstantInterfaceMethodRef(JavaBinaryReader br, ClassFile cls)
        {
            int classIndex = br.ReadUInt16();
            int nameAndTypeIndex = br.ReadUInt16();
            return new ConstantInterfaceMethodRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(classIndex), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(nameAndTypeIndex));
        }

        private ConstantMethodRefInfo ReadConstantMethodRef(JavaBinaryReader br, ClassFile cls)
        {
            int classIndex = br.ReadUInt16();
            int nameAndTypeIndex = br.ReadUInt16();
            return new ConstantMethodRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(classIndex), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(nameAndTypeIndex));
        }

        private ConstantFieldRefInfo ReadConstantFieldRef(JavaBinaryReader br, ClassFile cls)
        {
            int classIndex = br.ReadUInt16();
            int nameAndTypeIndex = br.ReadUInt16();
            return new ConstantFieldRefInfo(cls.ConstantPool.GetConstant<ConstantClassInfo>(classIndex), cls.ConstantPool.GetConstant<ConstantNameAndTypeInfo>(nameAndTypeIndex));
        }

        private ConstantClassInfo ReadConstantClass(JavaBinaryReader br, ClassFile cls)
        {
            int classIndex = br.ReadUInt16();
            return new ConstantClassInfo(cls.ConstantPool.GetConstant<ConstantUtf8Info>(classIndex));
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
                    int genericPos = descriptor.IndexOf("<", startIndex);
                    int separatorPos = descriptor.IndexOf(";", startIndex);

                    if (genericPos != -1 && separatorPos > genericPos)
                    {
                        int endGeneric;
                        FieldType generic = ParseFieldType(descriptor, genericPos + 1, out endGeneric);
                        if (descriptor[endGeneric] != '>')
                        {
                            throw new InvalidOperationException("Generic does not end with >");
                        }

                        separatorPos = endGeneric + 1;
                        if (descriptor[separatorPos] != ';')
                        {
                            throw new InvalidOperationException("Generic does not end with ;");
                        }
                    }

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

    public enum ConstantTag : byte
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
        InvokeDynamic = 18,

        Invalid = Byte.MaxValue,
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

    public class ConstantInvalidInfo : ConstantBase
    {
        public ConstantInvalidInfo() : base(ConstantTag.Invalid)
        {
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

    [Flags]
    public enum AccessFlag
    {
        Public = 0x0001,
        Final = 0x0010,
        Super = 0x0020,
        Interface = 0x0200,
        Abstract = 0x400,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000
    }
}

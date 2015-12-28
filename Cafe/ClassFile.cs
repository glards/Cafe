﻿using System;
using System.Collections.Generic;
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

        public ushort ConstantPoolSize { get; set; }
        // ??? ConstantPool

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
        }

        public static ClassFile Read(string filename)
        {
            using (JavaBinaryReader br = new JavaBinaryReader(new FileStream(filename, FileMode.Open)))
            {
                ClassFile cls = new ClassFile();

                cls.Magic = br.ReadUInt32();
                if (cls.Magic != 0xCAFEBABE)
                {
                    throw new InvalidOperationException("Magic value does not ");
                }

                cls.MinorVersion = br.ReadUInt16();
                cls.MajorVersion = br.ReadUInt16();

                return cls;
            }
        }
    }

    public static class DescriptorParser
    {

        private static FieldType ParseFieldType(string descriptor, int startIndex, out int stopIndex)
        {
            stopIndex = startIndex + 1;
            switch (descriptor[startIndex])
            {
                case 'B':
                    return new FieldType(NativeTypes.Byte);
                case 'C':
                    return new FieldType(NativeTypes.Char);
                case 'D':
                    return new FieldType(NativeTypes.Double);
                case 'F':
                    return new FieldType(NativeTypes.Float);
                case 'I':
                    return new FieldType(NativeTypes.Int);
                case 'J':
                    return new FieldType(NativeTypes.Long);
                case 'L':
                    int separatorPos = descriptor.IndexOf(";", startIndex);
                    string className = descriptor.Substring(startIndex + 1, separatorPos - 1);
                    stopIndex = separatorPos + 1;
                    return new ObjectType(className);
                case 'S':
                    return new FieldType(NativeTypes.Short);
                case 'Z':
                    return new FieldType(NativeTypes.Boolean);
                case '[':
                    return new ArrayType(ParseFieldType(descriptor.Substring(1)));
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
    }

    public enum NativeTypes
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
        Array
    }

    public class FieldType
    {
        public NativeTypes NativeType { get; set; }

        public FieldType(NativeTypes type)
        {
            NativeType = type;
        }
    }

    public class ObjectType : FieldType
    {
        public string ClassName { get; set; }

        public ObjectType(string className) : base(NativeTypes.ClassReference)
        {
            ClassName = className;
        }
    }

    public class ArrayType : FieldType
    {
        public FieldType InnerType { get; set; }

        public ArrayType(FieldType innerType) : base(NativeTypes.Array)
        {
            InnerType = innerType;
        }
    }
}
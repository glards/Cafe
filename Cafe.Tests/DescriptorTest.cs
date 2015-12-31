using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cafe.Tests
{
    public class DescriptorTest
    {
        [Fact]
        public void FieldDescriptorTest()
        {
            var fieldType = DescriptorParser.ParseFieldType("I");
            Assert.IsType<FieldType>(fieldType);
            Assert.Equal(NativeType.Int, fieldType.NativeType);

            fieldType = DescriptorParser.ParseFieldType("Ljava/lang/Object;");
            Assert.IsType<ObjectType>(fieldType);
            ObjectType objectType = fieldType as ObjectType;
            Assert.NotNull(objectType);
            Assert.Equal(NativeType.ClassReference, objectType.NativeType);
            Assert.Equal("java/lang/Object", objectType.ClassName);

            fieldType = DescriptorParser.ParseFieldType("[[[D");
            Assert.IsType<ArrayType>(fieldType);
            ArrayType arrayType = fieldType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeType.Array, arrayType.NativeType);
            arrayType = arrayType.InnerType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeType.Array, arrayType.NativeType);
            arrayType = arrayType.InnerType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeType.Array, arrayType.NativeType);
            fieldType = arrayType.InnerType;
            Assert.Equal(NativeType.Double, fieldType.NativeType);
        }


        [Fact]
        public void MethodDescriptorTest()
        {
            var methodDescriptor = DescriptorParser.ParseMethodDescriptor("(ID[[JLjava/lang/Thread;[[[F)Ljava/lang/Object;");

            Assert.NotNull(methodDescriptor.ReturnType);
            Assert.NotNull(methodDescriptor.ParameterTypes);
            Assert.Equal(5, methodDescriptor.ParameterTypes.Length);
            
            Assert.Equal(NativeType.Int, methodDescriptor.ParameterTypes[0].NativeType);
            Assert.Equal(NativeType.Double, methodDescriptor.ParameterTypes[1].NativeType);
            Assert.Equal(NativeType.Array, methodDescriptor.ParameterTypes[2].NativeType);
            Assert.Equal(NativeType.ClassReference, methodDescriptor.ParameterTypes[3].NativeType);
            Assert.Equal(NativeType.Array, methodDescriptor.ParameterTypes[4].NativeType);

            var obj = methodDescriptor.ParameterTypes[3] as ObjectType;
            Assert.NotNull(obj);
            Assert.Equal("java/lang/Thread", obj.ClassName);

            Assert.Equal(NativeType.ClassReference, methodDescriptor.ReturnType.NativeType);

            var ret = methodDescriptor.ReturnType as ObjectType;
            Assert.NotNull(ret);
            Assert.Equal("java/lang/Object", ret.ClassName);
        }

        [Fact]
        public void VoidMethodDescriptorTest()
        {
            var voidMethod = DescriptorParser.ParseMethodDescriptor("()V");
            Assert.NotNull(voidMethod.ReturnType);
            Assert.NotNull(voidMethod.ParameterTypes);
            Assert.Equal(0, voidMethod.ParameterTypes.Length);

            Assert.Equal(NativeType.Void, voidMethod.ReturnType.NativeType);

        }

        [Fact]
        public void GenericDescriptorTest()
        {
            var fieldType = DescriptorParser.ParseFieldType("Ljava/util/List<Lnet/minecraft/server/app;>;");
            Assert.IsType<ObjectType>(fieldType);
            ObjectType objectType = fieldType as ObjectType;
            Assert.NotNull(objectType);
            Assert.Equal(NativeType.ClassReference, objectType.NativeType);
            Assert.Equal("java/util/List<Lnet/minecraft/server/app;>", objectType.ClassName);

        }
    }
}

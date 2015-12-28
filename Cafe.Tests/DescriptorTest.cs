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
            Assert.Equal(NativeTypes.Int, fieldType.NativeType);

            fieldType = DescriptorParser.ParseFieldType("Ljava/lang/Object;");
            Assert.IsType<ObjectType>(fieldType);
            ObjectType objectType = fieldType as ObjectType;
            Assert.NotNull(objectType);
            Assert.Equal(NativeTypes.ClassReference, objectType.NativeType);
            Assert.Equal("java/lang/Object", objectType.ClassName);

            fieldType = DescriptorParser.ParseFieldType("[[[D");
            Assert.IsType<ArrayType>(fieldType);
            ArrayType arrayType = fieldType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeTypes.Array, arrayType.NativeType);
            arrayType = arrayType.InnerType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeTypes.Array, arrayType.NativeType);
            arrayType = arrayType.InnerType as ArrayType;
            Assert.NotNull(arrayType);
            Assert.Equal(NativeTypes.Array, arrayType.NativeType);
            fieldType = arrayType.InnerType;
            Assert.Equal(NativeTypes.Double, fieldType.NativeType);
        }
    }
}

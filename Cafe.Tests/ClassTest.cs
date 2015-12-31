using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cafe.Tests
{
    public class ClassTest
    {
        [Fact]
        public void TestClassReading()
        {
            using (FileStream fs = new FileStream(@"ClassFiles\aho.class", FileMode.Open))
            {
                ClassReader reader = new ClassReader();
                var cls = reader.Read(fs);

                var strings = cls.ConstantPool.Constants.OfType<ConstantStringInfo>().ToList();
                var floats = cls.ConstantPool.Constants.OfType<ConstantFloatInfo>().ToList();
                var longs = cls.ConstantPool.Constants.OfType<ConstantLongInfo>().ToList();

                var allStrings = strings.Select(x => x.Value.Value).ToList();

                int count = allStrings.Count;
            }
        }
    }
}

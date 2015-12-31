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
            }
        }
    }
}

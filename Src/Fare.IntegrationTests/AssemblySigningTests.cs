using System.Linq;
using System.Reflection;
using Xunit;

namespace Fare.IntegrationTests
{
    public class AssemblySigningTests
    {
        [Fact]
        public void FareAssemblyIsSigned()
        {
            var assemblyName = typeof(Xeger).GetTypeInfo().Assembly.GetName();
            var publicKeyToken = string.Join("", assemblyName.GetPublicKeyToken().Select(x => x.ToString("x")));            
            
            Assert.Equal("ea68d375bf33a7c8", publicKeyToken);
        }
    }
}
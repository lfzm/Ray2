using Orleans.Runtime;
using Ray2;
using Ray2.Protobuf;
using Ray2.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RaySiloHostBuilderExtensions
    {
        /// <summary>
        /// Add Protobuf serialization
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static IRayBuilder AddProtobuf(this IRayBuilder build)
        {
            build.Services.AddSingletonNamedService<ISerializer, ProtobufSerializer>(Ray2.Protobuf.SerializationType.Protobuf);
            return build;
        }
    }
}

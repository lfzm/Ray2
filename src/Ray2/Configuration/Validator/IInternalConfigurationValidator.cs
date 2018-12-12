using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public interface IInternalConfigurationValidator
    {
        void IsValid(InternalConfiguration configuration);
    }
}

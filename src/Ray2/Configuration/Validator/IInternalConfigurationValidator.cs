using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public interface IInternalConfigurationValidator
    {
        Task IsValid(InternalConfiguration configuration);
    }
}

using System.Threading.Tasks;
using Autofac;

namespace MX.Images
{
    public interface IRootScan
    {
        Task HandleAsync(IContainer container, string path);
    }
}
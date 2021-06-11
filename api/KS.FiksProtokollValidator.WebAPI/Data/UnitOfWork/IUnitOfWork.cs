using System;
using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.Data
{
    public interface IUnitOfWork : IDisposable
    {
        FiksIOMessageDBContext Context { get; }
        bool IsRoot { get; }
        Task SaveChangesAsync();
    }
}

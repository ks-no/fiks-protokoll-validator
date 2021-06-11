using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KS.FiksProtokollValidator.WebAPI.Data
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger _logger;
        private bool _disposed;

        public FiksIOMessageDBContext Context { get; }
        public bool IsRoot { get; }

        internal UnitOfWork(FiksIOMessageDBContext context, ILogger logger, bool isRoot)
        {
            Context = context;
            _logger = logger;
            IsRoot = isRoot;
        }

        public async Task SaveChangesAsync()
        {
            if (Context == null || !IsRoot)
                return;

            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
                return;

            _disposed = true;
        }
    }
}

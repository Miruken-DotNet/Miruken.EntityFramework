namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage;

    [Unmanaged]
    public class UnitOfWork : Handler, IDisposable
    {
        private readonly UnitOfWork _parent;
        private readonly UnitOfWorkAttribute _attribute;
        private readonly TransactionAttribute _transaction;
        private readonly IHandler _handler;
        private readonly Dictionary<Type, IDBContextUnitOfWork> _contexts =
            new Dictionary<Type, IDBContextUnitOfWork>();

        internal UnitOfWork(
            UnitOfWork           parent,
            UnitOfWorkAttribute  attribute,
            TransactionAttribute transaction,
            IHandler             handler)
        {
            _handler    = handler ?? throw new ArgumentNullException(nameof(handler));
            _parent     = parent;
            _attribute  = attribute;
            _transaction = transaction;
        }

        public event Action<UnitOfWork> BeforeCommit;
        public event Func<UnitOfWork, Task> BeforeCommitAsync;

        public event Action<UnitOfWork> AfterCommit;
        public event Func<UnitOfWork, Task> AfterCommitAsync;

        [Provides]
        public T GetContext<T>() where T : IDbContext =>
            GetOrCreateContext<T>().Context;

        [Provides]
        public UnitOfWork<T> ForContext<T>() where T : IDbContext =>
            GetOrCreateContext<T>();

        public void Commit()
        {
            var beforeCommit = BeforeCommit;
            beforeCommit?.Invoke(this);

            var beforeCommitAsync = BeforeCommitAsync;
            if (beforeCommitAsync != null)
            {
                foreach (var before in beforeCommitAsync.GetInvocationList())
                    ((Func<UnitOfWork, Task>)before)(this).GetAwaiter().GetResult();
            }

            foreach (var (_, uow) in _contexts)
                uow.Commit();

            var afterCommit = AfterCommit;
            afterCommit?.Invoke(this);

            var afterCommitAsync = AfterCommitAsync;
            if (afterCommitAsync != null)
            {
                foreach (var after in afterCommitAsync.GetInvocationList())
                    ((Func<UnitOfWork, Task>)after)(this).GetAwaiter().GetResult();
            }
        }

        public async Task CommitAsync()
        {
            var beforeCommit = BeforeCommit;
            beforeCommit?.Invoke(this);

            var beforeCommitAsync = BeforeCommitAsync;
            if (beforeCommitAsync != null)
            {
                foreach (var before in beforeCommitAsync.GetInvocationList())
                    await ((Func<UnitOfWork, Task>)before)(this);
            }

            foreach (var (_, uow) in _contexts)
                await uow.CommitAsync();

            var afterCommit = AfterCommit;
            afterCommit?.Invoke(this);

            var afterCommitAsync = AfterCommitAsync;
            if (afterCommitAsync != null)
            {
                foreach (var after in afterCommitAsync.GetInvocationList())
                    await ((Func<UnitOfWork, Task>)after)(this);
            }
        }

        public void Rollback()
        {
            foreach (var (_, uow) in _contexts)
                uow.Rollback();
        }

        public async Task RollbackAsync()
        {
            foreach (var (_, uow) in _contexts)
                await uow.RollbackAsync();
        }

        public void Dispose()
        {
            foreach (var (_, uow) in _contexts)
            {
                try
                {
                    uow.Dispose();
                }
                catch 
                {
                    // Ignore
                }
            }
        }

        private UnitOfWork<T> GetOrCreateContext<T>(bool create = true)
            where T : IDbContext
        {
            if (_contexts.TryGetValue(typeof(T), out var existing))
                return (UnitOfWork<T>)existing;

            if (!create)
                return _parent?.GetOrCreateContext<T>(false);

            var isolation = _transaction?.Isolation;
            IDbContextTransaction transaction = null;

            if (_attribute?.ForceNew != true)
            {
                var parent = _parent?.GetOrCreateContext<T>();
                if (parent != null)
                {
                    if (_transaction == null) return parent;
                    if (parent.Transaction != null)
                    {
                        return _transaction.Option switch
                        {
                            TransactionOption.RequiresNew => throw new InvalidOperationException(
                                "Inner UnitOfWork required a new transaction.  If this is desired set ForceNew to true."),
                            _ => _transaction.Isolation == null || parent.Isolation == _transaction.Isolation
                                   ? parent : throw new InvalidOperationException(
                                       $"Inner UnitOfWork required Isolation '{_transaction.Isolation}', but the outer transaction has Isolation '{parent.Isolation?.ToString()?? "Default"}'.  If this is desired set ForceNew to true.")
                        };
                    }
                    
                    throw new InvalidOperationException(
                        "Inner UnitOfWork requested a Transaction, but the outer did not.  If this is desired set ForceNew to true.");
                }
            }

            var context = _handler.Create<T>();

            if (_transaction != null)
            {
                switch (_transaction.Option)
                {
                    case TransactionOption.Required:
                    {
                        var parent = _parent?.GetOrCreateContext<T>(false);
                        transaction = RequireTransaction(context, parent, isolation);
                        break;
                    }
                    case TransactionOption.RequiresNew:
                        transaction = CreateTransaction(context, isolation);
                        break;
                }
            }

            var uow = new UnitOfWork<T>(context, transaction, isolation);
            _contexts.Add(typeof(T), uow);
            return uow;
        }

        private static IDbContextTransaction CreateTransaction(
            IDbContext      context,
            IsolationLevel? isolation)
        {
            return isolation != null
                 ? context.Database.BeginTransaction(isolation.Value)
                 : context.Database.BeginTransaction();
        }

        protected IDbContextTransaction RequireTransaction<T>(
            T               context,
            UnitOfWork<T>   parent,
            IsolationLevel? isolation) 
            where T : IDbContext
        {
            if (parent != null && (isolation == null || isolation == parent.Isolation))
            {
                var parentContext = parent.Context;
                var transaction   = parentContext.Database.CurrentTransaction;
                if (transaction != null)
                {
                    try
                    {
                        var dbTransaction = transaction.GetDbTransaction();
                        context.Database.UseTransaction(dbTransaction);
                        return null;
                    }
                    catch
                    {
                        // Only supported for relational databases
                    }
                }
            }
            return CreateTransaction(context, isolation);
        }
    }

    public interface IDBContextUnitOfWork : IDisposable
    {
        IDbContext Context { get; }

        void Commit();
        Task CommitAsync();

        void Rollback();
        Task RollbackAsync();
    }

    public class UnitOfWork<T> : IDBContextUnitOfWork where T : IDbContext
    {
        internal UnitOfWork(
            T                     context,
            IDbContextTransaction transaction,
            IsolationLevel?       isolation = null)
        {
            Context     = context;
            Transaction = transaction;
            Isolation   = isolation;
        }

        public T                     Context     { get; }
        public IDbContextTransaction Transaction { get; }
        public IsolationLevel?       Isolation   { get; }

        IDbContext IDBContextUnitOfWork.Context => Context;

        public event Action<UnitOfWork<T>> BeforeCommit;
        public event Func<UnitOfWork<T>, Task> BeforeCommitAsync;

        public event Action<UnitOfWork<T>> AfterCommit;
        public event Func<UnitOfWork<T>, Task> AfterCommitAsync;

        public EntityEntry<E> Add<E>(E entity, Action complete = null)
            where E: class
        {
            if (complete != null)
                AfterCommit += _ => complete();
            return Context.Add(entity);
        }

        public async ValueTask<EntityEntry<E>> AddAsync<E>(E entity, Func<Task> complete = null)
            where E : class
        {
            if (complete != null)
                AfterCommitAsync += async _ => await complete();
            return await Context.AddAsync(entity);
        }

        public EntityEntry<E> Update<E>(E entity, Action complete = null)
            where E : class
        {
            if (complete != null)
                AfterCommit += _ => complete();
            return Context.Update(entity);
        }

        public EntityEntry<E> Remove<E>(E entity, Action complete = null)
            where E : class
        {
            if (complete != null)
                AfterCommit += _ => complete();
            return Context.Remove(entity);
        }

        public void Commit()
        {
            var beforeCommit = BeforeCommit;
            beforeCommit?.Invoke(this);

            var beforeCommitAsync = BeforeCommitAsync;
            if (beforeCommitAsync != null)
            {
                foreach (var before in beforeCommitAsync.GetInvocationList())
                    ((Func<UnitOfWork<T>, Task>)before)(this).GetAwaiter().GetResult();
            }

            Context.SaveChanges();

            Transaction?.Commit();

            var afterCommit = AfterCommit;
            afterCommit?.Invoke(this);

            var afterCommitAsync = AfterCommitAsync;
            if (afterCommitAsync != null)
            {
                foreach (var after in afterCommitAsync.GetInvocationList())
                    ((Func<UnitOfWork<T>, Task>)after)(this).GetAwaiter().GetResult();
            }
        }

        public async Task CommitAsync()
        {
            var beforeCommit = BeforeCommit;
            beforeCommit?.Invoke(this);

            var beforeCommitAsync = BeforeCommitAsync;
            if (beforeCommitAsync != null)
            {
                foreach (var before in beforeCommitAsync.GetInvocationList())
                    await ((Func<UnitOfWork<T>, Task>)before)(this);
            }

            await Context.SaveChangesAsync();

            if (Transaction != null)
                await Transaction.CommitAsync();

            var afterCommit = AfterCommit;
            afterCommit?.Invoke(this);

            var afterCommitAsync = AfterCommitAsync;
            if (afterCommitAsync != null)
            {
                foreach (var after in afterCommitAsync.GetInvocationList())
                    await ((Func<UnitOfWork<T>, Task>)after)(this);
            }
        }

        public void Rollback()
        {
            var entries = Context.ChangeTracker?.Entries();
            if (entries == null) return;
            foreach (var entry in entries)
                entry.Reload();
        }

        public async Task RollbackAsync()
        {
            var entries = Context.ChangeTracker?.Entries();
            if (entries == null) return;
            foreach (var entry in entries)
                await entry.ReloadAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public static implicit operator T(UnitOfWork<T> uow) => uow.Context;
    }
}

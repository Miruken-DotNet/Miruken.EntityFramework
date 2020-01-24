namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Callback;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage;

    [Unmanaged]
    public class UnitOfWork : Handler, IDisposable
    {
        private readonly IHandler _handler;
        private readonly Dictionary<Type, IDBContextUnitOfWork> _contexts =
            new Dictionary<Type, IDBContextUnitOfWork>();

        public UnitOfWork(IHandler handler, bool beginTransaction)
        {
            _handler = handler ??
                throw new ArgumentNullException(nameof(handler));

            BeginTransaction = beginTransaction;
        }

        public bool BeginTransaction { get; }

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
                    uow.Context.Dispose();
                }
                catch 
                {
                    // Ignore
                }
            }
        }

        private UnitOfWork<T> GetOrCreateContext<T>()
            where T : IDbContext
        {
            if (_contexts.TryGetValue(typeof(T), out var existing))
                return (UnitOfWork<T>)existing;
            var uow = new UnitOfWork<T>(_handler.Create<T>(), BeginTransaction);
            _contexts.Add(typeof(T), uow);
            return uow;
        }
    }

    public interface IDBContextUnitOfWork
    {
        IDbContext Context { get; }

        void Commit();
        Task CommitAsync();

        void Rollback();
        Task RollbackAsync();
    }

    public class UnitOfWork<T> : IDBContextUnitOfWork where T : IDbContext
    {
        private readonly IDbContextTransaction _transaction;

        internal UnitOfWork(T context, bool beginTransaction)
        {
            Context = context;

            if (beginTransaction)
            {
                var database    = Context.Database;
                var transaction = database.CurrentTransaction;
                if (transaction == null)
                    _transaction = database.BeginTransaction();
            }
        }

        public T Context { get; }

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

            _transaction?.Commit();

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

            if (_transaction != null)
                await _transaction.CommitAsync();

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

        public static implicit operator T(UnitOfWork<T> uow) => uow.Context;
    }
}

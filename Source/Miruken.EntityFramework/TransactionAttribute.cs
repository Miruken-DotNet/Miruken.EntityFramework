namespace Miruken.EntityFramework
{
    using System;
    using System.Data;

    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionAttribute : Attribute
    {
        public TransactionAttribute(
            TransactionOption option = TransactionOption.Required)
        {
            Option = option;
        }

        public TransactionAttribute(IsolationLevel isolation,
            TransactionOption option = TransactionOption.Required)
            : this(option)
        {
            Isolation = isolation;
        }

        public TransactionOption Option { get; }

        public IsolationLevel? Isolation { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_Core_Fundamentals.Services
{
    public class Operation : IOperation, IOperationTransient, IOperationScoped, IOperationSingleton, IOperationSingletonInstance
    {
        public Guid OperationId { get; }

        public Operation()
        {
            this.OperationId = Guid.NewGuid();
        }

        public Operation(Guid operationId)
        {
            this.OperationId = operationId;
        }
    }
}

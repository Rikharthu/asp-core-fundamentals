using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ASP_Core_Fundamentals.Services;

namespace ASP_Core_Fundamentals.Controllers
{
    public class DependencyController : Controller
    {

        private readonly OperationService _operationService;
        private readonly IOperationTransient _firstTransientOperation;
        private readonly IOperationTransient _secondTransientOperation;
        private readonly IOperationScoped _firstScopedOperation;
        private readonly IOperationScoped _secondScopedOperation;
        private readonly IOperationSingleton _singletonOperation;
        private readonly IOperationSingletonInstance _singletonInstanceOperation;

        public static int RequestCount { get; set; }

        public DependencyController(OperationService operationService,
            IOperationTransient firstTransientOperation,
            IOperationTransient secondTransientOperation,
            IOperationScoped firstScopedOperation,
            IOperationScoped secondScopedOperation,
            IOperationSingleton singletonOperation,
            IOperationSingletonInstance singletonInstanceOperation)
        {
            _operationService = operationService;

            _firstTransientOperation = firstTransientOperation;
            _secondTransientOperation = secondTransientOperation;

            _firstScopedOperation = firstScopedOperation;
            _secondScopedOperation = secondScopedOperation;

            _singletonOperation = singletonOperation;

            _singletonInstanceOperation = singletonInstanceOperation;
        }

        public IActionResult Index()
        {
            ViewBag.RequestNumber = ++RequestCount;

            // viewbag contains controller-requested services
            ViewBag.FirstTransient = _firstTransientOperation;
            ViewBag.SecondTransient = _secondTransientOperation;
            ViewBag.FirstScoped = _firstScopedOperation;
            ViewBag.SecondScoped = _secondScopedOperation;
            ViewBag.Singleton = _singletonOperation;
            ViewBag.SingletonInstance = _singletonInstanceOperation;

            // operation service has its own requested services
            ViewBag.Service = _operationService;
            return View();
        }

    }
}
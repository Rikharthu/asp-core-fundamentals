using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Routing_Demo.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            // extract data tokens
            ViewBag.locale= (string)RouteData.DataTokens["locale"];
            ViewBag.routeName = (string)RouteData.DataTokens["routeName"];
            return View();
        }
    }
}
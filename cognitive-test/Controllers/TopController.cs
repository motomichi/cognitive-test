using System.Web.Mvc;

namespace cognitive_test.Controllers
{
    public class TopController : Controller
    {
        // GET: Top
        [AllowAnonymous]
        public ActionResult Index()
        {
            @ViewBag.HomeActive = "active";
            @ViewBag.Title = "top";
            return View();
        }
    }
}
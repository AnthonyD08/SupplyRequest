using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login(string? returnUrl)
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            _httpContextAccessor.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            if (returnUrl is not null)
                _httpContextAccessor.HttpContext?.Session.SetString("ReturnUrl", returnUrl);

            return View("Login");
        }

        [Authorize]
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Request");
        }

        [HttpGet]
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View("Privacy");
        }
    }
}

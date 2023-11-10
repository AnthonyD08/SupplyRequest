using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            // Obtener el usuario actual desde la sesión (si está autenticado)
            var userJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (userJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            // Deserializar el usuario desde el JSON almacenado en la sesión
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            // Pasar el modelo "User" a la vista "Perfil/Index"
            return View(user);
        }

        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var userJson = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (userJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var currentUser = JsonConvert.DeserializeObject<User>(userJson);

            if (currentUser is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new UserRepository();

            currentUser = await repo.ReadAsync(new User { UserId = currentUser.UserId });

            if (currentUser is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            if (currentUser.RoleId != 4 && currentUser.UserId != userId)
                return Unauthorized("No tiene permisos para ver este usuario");

            var user = await repo.ReadAsync(new User { UserId = userId });

            if (user is null)
                return NotFound("User not found");

            return Ok(user);
        }
        
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var repo = new UserRepository();

            var isUsernameAvailable = await repo.IsUsernameAvailableAsync(user.Username);

            if (!isUsernameAvailable)
                return BadRequest("Username is not available");

            var success = await repo.CreateAsync(user);

            if (success)
                return Ok("Acccount created successfully");

            return BadRequest("There was an error creating the account");
        }

        [Authorize]
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var repo = new UserRepository();

            var originalUser = await repo.ReadAsync(new User() { UserId = user.UserId });

            if (originalUser is null)
                return BadRequest("There was an error updating the account");

            // No queremos que incluso el administrador pueda hacerse con las contraseñas de los usuarios
            user.Password = originalUser.Password;
            user.PasswordSalt = originalUser.PasswordSalt;

            if (user.Username != originalUser.Username)
            {
                var isUsernameAvailable = await repo.IsUsernameAvailableAsync(user.Username);

                if (!isUsernameAvailable)
                    return BadRequest("Username is not available");
            }

            var success = await repo.UpdateAsync(user);

            if (success)
                return Ok("Account updated successfully");

            return BadRequest("There was an error updating the account");
        }

        // authenticate user
        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> Authenticate(string username, string password)
        {
            var repo = new UserRepository();

            var user = await repo.ReadByUsernameAsync(username);

            if (user is null)
            {
                ModelState.AddModelError("LogOnError", "Usuario o contraseña incorrectos");

                return View("Login");
            }

            if (!user.Active)
            {
                ModelState.AddModelError("LogOnError", "Su cuenta ha sido desactivada");

                return View("Login");
            }

            var success = await repo.AuthenticateAsync(username, password);

            if (!success)
            {
                ModelState.AddModelError("LogOnError", "Usuario o contraseña incorrectos");

                return View("Login");
            }

            // Inside your login action
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, $"{user.RoleId}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            var json = JsonConvert.SerializeObject(user, formatting: Formatting.Indented);

            _httpContextAccessor.HttpContext?.Session.SetString("User", json);

            var redirectUrl = _httpContextAccessor.HttpContext?.Session.GetString("ReturnUrl");
            _httpContextAccessor.HttpContext?.Session.Remove("ReturnUrl");

            if (!string.IsNullOrEmpty(redirectUrl))
                return LocalRedirect(redirectUrl);

            return RedirectToAction("Index", "Request");
        }
        
        [HttpGet]
        [Route("LogOut")]
        public async Task<IActionResult> LogOut()
        {
            // Por alguna razón, clickear el botón de logout automáticamente hace GET a la ruta /User/LogOut
            // y no POST como debería ser, sin la posibilidad de cambiarlo.
            // Por lo tanto vamos a utilizar GET aunque no sea el método más apropiado.

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _httpContextAccessor.HttpContext?.Session.Remove("User");

            return RedirectToAction("Login", "Home");
        }

        [Authorize]
        [HttpPost]
        [Route("Password")]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            oldPassword = Encoding.UTF8.GetString(Convert.FromBase64String(oldPassword));
            newPassword = Encoding.UTF8.GetString(Convert.FromBase64String(newPassword));

            var userJson = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (userJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(userJson);

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new UserRepository();

            var user = await repo.ReadAsync(new User() { UserId = usuarioActual.UserId });

            if (user is null)
                return BadRequest("Ha habido un error cambiando la contraseña");

            var authenticated = await repo.AuthenticateAsync(user.Username, oldPassword);

            if (!authenticated)
                return Unauthorized("Verifique que la contraseña sea la correcta");

            var success = await repo.UpdatePasswordAsync(user.UserId, newPassword);

            if (success)
                return Ok("Contraseña cambiada con éxito");

            return BadRequest("Ha habido un error cambiando la contraseña");
        }

        [HttpGet]
        [Route("CurrentUser")]
        public async Task<IActionResult> CurrentUser()
        {
            var userInSession = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (userInSession is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var user = JsonConvert.DeserializeObject<User>(userInSession);

            if (user is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            return Ok(user);
        }

        [Authorize]
        [HttpGet]
        [Route("Admin/Index")]
        public async Task<IActionResult> AdminIndex()
        {
            var usuarioActual = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var user = JsonConvert.DeserializeObject<User>(usuarioActual);

            if (user is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            if (user.RoleId != 4)
                return Unauthorized("No tiene permisos para acceder a esta página");

            var repo = new UserRepository();

            var users = await repo.ReadAllAsync();

            return View(users);
        }
    }
}

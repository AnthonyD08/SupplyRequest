using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using WebApi.Models;
using WebApi.Repositories;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Ocsp;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [Route("Request")]
    public class RequestController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usuarioActual = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuario = JsonConvert.DeserializeObject<User>(usuarioActual);

            if (usuario is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestRepository();

            var listaRequests = usuario.RoleId switch
            {
                2 => await repo.ReadByBossIdAsync(usuario.BossId),
                3 => await repo.ReadByAccountantIdAsync(usuario.UserId),
                4 => await repo.ReadAllAsync(),
                _ => await repo.ReadByUsernameAsync(usuario.Username)
            };
            
            if (listaRequests is null)
            {
                return View(Array.Empty<Request>());
            }

            ////////////////***********GRAFICOS CHART JS//////////////////////*********/ /
            var random = new Random();

            var etiquetas = new List<string>();
            var colores = new List<string>();
            var valores = new List<string>();

            foreach (var price in listaRequests.GroupBy(e => e.Active)
                .Select(group => new {
                    Active = group.Key,
                    Cantidad = group.Count()
                }).OrderBy(x => x.Active))
            {
                var color = $"#{random.Next(0x1000000):X6}";

                etiquetas.Add(price.Active.ToString());
                valores.Add(price.Cantidad.ToString());
                colores.Add(color);
            }

            ViewBag.Etiquetas = JsonConvert.SerializeObject(etiquetas);
            ViewBag.Valores = JsonConvert.SerializeObject(valores);
            ViewBag.Colores = JsonConvert.SerializeObject(colores);

            ////////////////***********GRAFICOS CHART JS///////////////////////

            return usuario.RoleId switch
            {
                2 => View("BossIndex", listaRequests),
                3 => View("AccountantIndex", listaRequests),
                4 => View("AdminIndex", listaRequests),
                _ => View(listaRequests),
            };
        }

        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRequest(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);
            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestRepository();
            var requestWithDetails = await repo.ReadAsync(new Request { RequestId = requestId });

            if (requestWithDetails is null)
                return NotFound($"El Request: {requestId} no existe.");

            return Ok(requestWithDetails);
        }

        [Authorize]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateRequest([FromBody] Request request)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);
            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            request.Employee = usuarioActual.UserId;
            request.Accountant = null;

            var repo = new RequestRepository();
            var statusRepo = new RequestStatusRepository();
            var requestSuccess = await repo.CreateAsync(request);

            var userRequestList = await repo.ReadByUsernameAsync(usuarioActual.Username);

            if (userRequestList is null)
                return BadRequest("Hubo un error al ingresar el Request.");
            
            var latestRequest = userRequestList.MaxBy(x => x.RequestId);

            if (latestRequest is null)
                return BadRequest("Hubo un error al ingresar el Request.");
            
            var newStatus = new RequestStatus
            {
                RequestId = latestRequest.RequestId,
                UserId = usuarioActual.UserId,
                Date = DateTime.Now,
                Time = DateTime.Now.TimeOfDay,
                Description = "Solicitud creada"
            };

            var statusSuccess = await statusRepo.CreateAsync(newStatus);

            var success = requestSuccess && statusSuccess;

            if (!success) return BadRequest("Hubo un error al ingresar el Request.");

            await Task.Run(async () =>
            {
                var emailController = new EmailController(_httpContextAccessor);
                await emailController.SendTemplate(1, latestRequest.RequestId);
            });

            if (usuarioActual.RoleId != 2 && usuarioActual.RoleId != 4) 
                return Ok("Request creado correctamente");

            await Task.Run(async () =>
            {
                await ApproveRequest(latestRequest.RequestId);
            });

            // En caso de que el usuario sea un jefe queremos que el request pase a estado aprobado por el jefe

            return Ok("Request creado correctamente");
        }

        [Authorize]
        [HttpPost]
        [Route("Approve")]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestRepository();

            var request = await repo.ReadAsync(new Request { RequestId = requestId });

            if (request is null)
                return NotFound("No se ha encontrado ese Request");

            if (!request.Active)
                return UnprocessableEntity("El request no está activo");

            var statusRepo = new RequestStatusRepository();

            bool approved;
            string statusDescription;

            switch (usuarioActual.RoleId)
            {
                case 2:
                    approved = await statusRepo.IsApprovedByBoss(requestId);
                    statusDescription = "Aprobada por el jefe";
                    break;
                case 3:
                    approved = await statusRepo.IsApprovedByAccountant(requestId);
                    statusDescription = "Aprobada por el contador";
                    break;
                case 4:
                    approved = await statusRepo.IsApprovedByBoss(requestId);
                    statusDescription = "Aprobada por el administrador";
                    break;
                default:
                    return Unauthorized("No tiene permisos para aprobar este Request");
            }

            if (approved)
                return UnprocessableEntity("El request ya está aprobado");

            var requestStatus = new RequestStatus()
            {
                RequestId = requestId,
                UserId = usuarioActual.UserId,
                Date = DateTime.Now,
                Time = DateTime.Now.TimeOfDay,
                Description = statusDescription
            };

            var statusSuccess = await statusRepo.CreateAsync(requestStatus);

            if (!statusSuccess) 
                return StatusCode(500, "Error inesperado al escribir el estado del request");

            bool success;

            switch (usuarioActual.RoleId)
            {
                case 2:
                    success = await repo.AssignToAccountantByAmount(request.RequestId);
                    await Task.Run(async () =>
                    {
                        var emailController = new EmailController(_httpContextAccessor);
                        await emailController.SendTemplate(2, request.RequestId);
                    });
                    break;
                case 3:
                    request.Accountant = usuarioActual.UserId;
                    request.Active = false;
                    success = await repo.UpdateAsync(request);
                    await Task.Run(async () =>
                    {
                        var emailController = new EmailController(_httpContextAccessor);
                        await emailController.SendTemplate(4, request.RequestId);
                    });
                    break;
                case 4:
                    success = await repo.AssignToAccountantByAmount(request.RequestId);
                    await Task.Run(async () =>
                    {
                        var emailController = new EmailController(_httpContextAccessor);
                        await emailController.SendTemplate(2, request.RequestId);
                    });
                    break;
                default:
                    return Unauthorized("No tiene permisos para aprobar este Request");
            }

            return Ok(
                success ?
                    $"El Request: {requestId} ha sido aprobado con éxito." :
                    $"El Request: {requestId} no fue aprobado.");

        }

        [Authorize]
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateRequest([FromBody] Request request)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var statusRepo = new RequestStatusRepository();

            if (!request.Active)
                return UnprocessableEntity("El request no está activo");

            var isApproved = await statusRepo.IsApprovedByBoss(request.RequestId);

            if (isApproved)
                return UnprocessableEntity("El request ya está aprobado por el jefe y no se le pueden hacer cambios");

            var repo = new RequestRepository();

            var success = await repo.UpdateAsync(request);

            return Ok(
                success ?
                    $"El Request: {request.RequestId} ha sido modificado con éxito." :
                    $"El Request: {request.RequestId} no fue modificado.");
        }

        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestRepository();
            
            var request = await repo.ReadAsync(new Request { RequestId = requestId });

            if (request is null)
                return NotFound("No se ha encontrado ese Request");

            if (!request.Active)
                return UnprocessableEntity("El request ya está desactivado");

            var requestEditSuccess = await repo.CancelAsync(requestId);

            var statusRepo = new RequestStatusRepository();

            var description = usuarioActual.RoleId switch
            {
                1 => "Cancelada por el usuario",
                2 => "Rechazada por el jefe",
                3 => "Rechazada por el contador",
                4 => "Rechazada por el administrador",
                _ => "Rechazada por un rol desconocido"
            };

            var requestStatus = new RequestStatus()
            {
                RequestId = requestId,
                UserId = usuarioActual.UserId,
                Date = DateTime.Now,
                Time = DateTime.Now.TimeOfDay,
                Description = description
            };  

            var statusSuccess = await statusRepo.CreateAsync(requestStatus);

            var userRepo = new UserRepository();

            var emailTemplate = usuarioActual.RoleId switch
            {
                1 => 6,
                2 => 3,
                3 => 5,
                4 => 3,
                _ => 6
            };

            await Task.Run(async () =>
            {
                var emailController = new EmailController(_httpContextAccessor);
                await emailController.SendTemplate(emailTemplate, request.RequestId);
            });

            return Ok(
                (requestEditSuccess && statusSuccess) ?
                    $"El Request: {requestId} ha sido cancelado con éxito." :
                    $"El Request: {requestId} no fue cancelado.");
        }

        [Authorize]
        [HttpGet]
        [Route("Status/Index")]
        public async Task<IActionResult> StatusIndex(int requestId)
        {
            var usuarioActual = _httpContextAccessor.HttpContext?.Session.GetString("User");

            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuario = JsonConvert.DeserializeObject<User>(usuarioActual);

            if (usuario is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var userRepo = new UserRepository();
            var requestRepo = new RequestRepository();

            var request = await requestRepo.ReadAsync(new Request { RequestId = requestId });

            if (request is null)
                return NotFound("No se ha encontrado ese Request");

            var requestOwner = await userRepo.ReadAsync(new User { UserId = request.Employee });

            if (requestOwner is null)
                return NotFound("No se ha encontrado el usuario que creó el Request");

            // Ahora validamos que el usuario actual tenga permisos para ver el Request. O sea que sea el requestOwner, o el Boss del requestOwner, o el accountant asignado al request
            // Recientemente añadido el rol #4 que es el Sysadmin, que puede ver todos los requests
            if (usuario.UserId != requestOwner.UserId && usuario.UserId != requestOwner.BossId && request.Accountant != usuario.UserId && usuario.RoleId != 4)
                return Unauthorized("No tiene permisos para ver este Request");

            var repo = new RequestStatusRepository();

            var listaRequestStatus = await repo.ReadAllRequestStatus(requestId);

            if (listaRequestStatus is null)
            {
                return View("StatusIndex", Array.Empty<RequestStatus>());
            }

            ////////////////***********GRAFICOS CHART JS//////////////////////*********/ /
            var random = new Random();

            var etiquetas = new List<string>();
            var colores = new List<string>();
            var valores = new List<string>();

            foreach (var description in listaRequestStatus.GroupBy(e => e.Description)
                .Select(group => new
                {
                    Active = group.Key,
                    Cantidad = group.Count()
                }).OrderBy(x => x.Active))
            {
                var color = $"#{random.Next(0x1000000):X6}";

                etiquetas.Add(description.Active.ToString());
                valores.Add(description.Cantidad.ToString());
                colores.Add(color);
            }

            ViewBag.Etiquetas = JsonConvert.SerializeObject(etiquetas);
            ViewBag.Valores = JsonConvert.SerializeObject(valores);
            ViewBag.Colores = JsonConvert.SerializeObject(colores);

            ////////////////***********GRAFICOS CHART JS///////////////////////

            return View("StatusIndex", listaRequestStatus);
        }

        [Authorize]
        [HttpGet]
        [Route("Status/Latest")]
        public async Task<IActionResult> GetLatestStatus(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);
            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestStatusRepository();
            var requestStatus = await repo.ReadLatestAsync(requestId);

            if (requestStatus is null)
                return NotFound($"El Request: {requestId} no existe o no ha recibido actualizaciones de estado.");

            return Ok(requestStatus);
        }

        [Authorize]
        [HttpGet]
        [Route("Status")]
        public async Task<IActionResult> GetStatusList(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);
            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestStatusRepository();
            var requestStatusList = await repo.ReadByRequestAsync(requestId);

            if (requestStatusList is null)
                return NotFound($"El Request: {requestId} no existe o no ha recibido actualizaciones de estado.");

            return Ok(requestStatusList);
        }

        [Authorize]
        [HttpPost]
        [Route("Status")]
        public async Task<IActionResult> CreateStatus(int requestId)
        {
            var usuarioJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
            if (usuarioJson is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var usuarioActual = JsonConvert.DeserializeObject<User>(usuarioJson);
            if (usuarioActual is null)
                return Unauthorized("No se ha iniciado sesión o su sesión ha expirado");

            var repo = new RequestStatusRepository();
            var requestStatus = new RequestStatus
            {
                RequestId = requestId,
                UserId = usuarioActual.UserId,
                Date = DateTime.Now,
                Time = DateTime.Now.TimeOfDay,
                Description = "Solicitud creada"
            };

            var success = await repo.CreateAsync(requestStatus);

            return Ok(
                success ?
                    $"El Request: {requestId} ha sido modificado con éxito." :
                    $"El Request: {requestId} no fue modificado.");
        }
    }
}
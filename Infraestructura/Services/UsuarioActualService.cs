using System.Security.Claims;

namespace LICORERIA.Core.Services
{
    public class UsuarioActualService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioActualService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? ObtenerIdUsuario()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return null;

            return int.Parse(claim.Value);
        }
    }
}
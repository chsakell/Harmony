using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Identity
{
    /// <summary>
    /// Controller for Role operations. Each action needs its administration permission (ADR
    /// 2026-11-27-api-authorization-model, rule 3).
    /// </summary>
    [Route("api/identity/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;

        public RoleController(IRoleService roleService, IAuthorizationService authorizationService)
        {
            _roleService = roleService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Creates a role, or renames one when <see cref="RoleRequest.Id"/> is set; a rename also needs
        /// <see cref="Permissions.Roles.Edit"/>.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Roles.Create)]
        public async Task<IActionResult> Post(RoleRequest request)
        {
            if (!string.IsNullOrEmpty(request.Id)
                && !(await _authorizationService.AuthorizeAsync(User, Permissions.Roles.Edit)).Succeeded)
            {
                return Forbid();
            }

            var response = await _roleService.SaveAsync(request);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Roles.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _roleService.DeleteAsync(id);
            return Ok(response);
        }

        [HttpGet("permissions/{roleId}")]
        [Authorize(Policy = Permissions.RoleClaims.View)]
        public async Task<IActionResult> GetPermissionsByRoleId([FromRoute] string roleId)
        {
            var response = await _roleService.GetAllPermissionsAsync(roleId);
            return Ok(response);
        }

        [HttpPut("permissions/update")]
        [Authorize(Policy = Permissions.RoleClaims.Edit)]
        public async Task<IActionResult> Update(PermissionRequest model)
        {
            var response = await _roleService.UpdatePermissionsAsync(model);
            return Ok(response);
        }
    }
}

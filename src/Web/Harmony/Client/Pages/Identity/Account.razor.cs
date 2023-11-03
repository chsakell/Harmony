using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Security.Claims;

namespace Harmony.Client.Pages.Identity
{
    public partial class Account
    {
        private UserModel _user = new();
        private bool _updating = false;

        private ChangePasswordModel _changePassword = new();
        private bool _updatingPassword = false;

        private bool _passwordVisibility;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        protected async override Task OnInitializedAsync()
        {
            var user = await _authenticationManager.CurrentUser();
            var userId = user.Claims
                    .FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var result = await _userManager.GetAsync(userId) ;

            if(result.Succeeded)
            {
                _user = _mapper.Map<UserModel>(result.Data);
            }
        }

        private async Task Update()
        {

        }

        private async Task ChangePassword()
        {

        }

        private void TogglePasswordVisibility()
        {
            if (_passwordVisibility)
            {
                _passwordVisibility = false;
                _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
                _passwordInput = InputType.Password;
            }
            else
            {
                _passwordVisibility = true;
                _passwordInputIcon = Icons.Material.Filled.Visibility;
                _passwordInput = InputType.Text;
            }
        }
    }
}

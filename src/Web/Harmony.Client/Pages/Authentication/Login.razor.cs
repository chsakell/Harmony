using Harmony.Application.DTO;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace Harmony.Client.Pages.Authentication
{
    public partial class Login
    {
        //private FluentValidationValidator _fluentValidationValidator;
        //private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        private TokenRequest _tokenModel = new();

        protected override async Task OnInitializedAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            if (state != new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())))
            {
                _navigationManager.NavigateTo("/");
            }
        }

        private async Task SubmitAsync()
        {
            var result = await _authenticationManager.Login(_tokenModel);
            if (!result.Succeeded)
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
            else
            {
                await _workspaceManager.InitAsync();

                if (_workspaceManager.SelectWorkspace != null)
                {
                    Navigate(_workspaceManager.SelectedWorkspace);
                }
            }
        }

        private bool _passwordVisibility;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        void TogglePasswordVisibility()
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

        private void Navigate(WorkspaceDto workspace)
        {
            if(string.IsNullOrEmpty(workspace?.Name))
            {
                return;
            }

            var slug = StringUtilities.SlugifyString(workspace.Name);
            _navigationManager.NavigateTo($"workspaces/{workspace.Id}/{slug}");
        }
    }
}
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Users.Commands.UpdatePassword;
using Harmony.Application.Features.Users.Commands.UpdateProfile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Application.Helpers;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Account;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Dialogs;
using Harmony.Shared.Storage;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Security.Claims;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Client.Pages.Identity
{
    public partial class Account
    {
        private UserModel _user = new();
        private bool _updating = false;

        private ChangePasswordModel _changePassword = new();
        private bool _updatingPassword = false;

        private bool _uploadingImage = false;

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
            _updating = true;

            var result = await _accountManager.UpdateProfileAsync(new UpdateProfileCommand()
            {
                Email = _user.Email,
                FirstName = _user.FirstName,
                LastName = _user.LastName
            });

            _updating = false;

            DisplayMessage(result);
        }

        private async Task ChangePassword()
        {
            _updatingPassword = true;

            var result = await _accountManager.ChangePasswordAsync(new UpdatePasswordCommand()
            {
                Password = _changePassword.CurrentPassword,
                NewPassword = _changePassword.NewPassword,
                ConfirmNewPassword = _changePassword.NewPassword
            });

            _updatingPassword = false;

            DisplayMessage(result);
        }

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            const long maxAllowedImageSize = 10000000;
            _uploadingImage = true;
            var file = e.File;

            var extension = Path.GetExtension(file.Name);
            var fileName = file.Name;
            var buffer = new byte[file.Size];
            await file.OpenReadStream(maxAllowedImageSize).ReadAsync(buffer);
            var request = new UploadProfilePictureCommand
            {
                Data = buffer,
                FileName = fileName,
                Extension = extension,
                Type = Domain.Enums.AttachmentType.ProfilePicture
            };

            var result = await _fileManager.UploadProfilePicture(request);

            if (result.Succeeded)
            {
                _user.ProfilePicture = result.Data.ProfilePicture;
            }

            DisplayMessage(result);

            _uploadingImage = false;
        }

        private async Task DeleteAsync()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to remove your profile picture?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var request = new UploadProfilePictureCommand
                {
                    Data = new byte[0],
                    Type = Domain.Enums.AttachmentType.ProfilePicture
                };

                var result = await _fileManager.UploadProfilePicture(request);

                if (result.Succeeded)
                {
                    _user.ProfilePicture = result.Data.ProfilePicture;
                }

                DisplayMessage(result);
            }
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

        private void DisplayMessage(IResult result)
        {
            if (result == null)
            {
                return;
            }

            var severity = result.Succeeded ? Severity.Success : Severity.Error;

            foreach (var message in result.Messages)
            {
                _snackBar.Add(message, severity);
            }
        }
    }
}

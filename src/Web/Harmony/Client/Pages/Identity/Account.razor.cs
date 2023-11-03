using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Application.Helpers;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Account;
using Harmony.Shared.Storage;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
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

        }

        private async Task ChangePassword()
        {

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
                _user.ProfilePictureDataUrl = result.Data.ProfilePicture;
            }

            DisplayMessage(result);

            _uploadingImage = false;
        }

        private async Task DeleteAsync()
        {
            //var parameters = new DialogParameters
            //{
            //    {nameof(Shared.Dialogs.DeleteConfirmation.ContentText), $"{string.Format(_localizer["Do you want to delete the profile picture of {0}"], _profileModel.Email)}?"}
            //};
            //var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            //var dialog = _dialogService.Show<Shared.Dialogs.DeleteConfirmation>("Delete", parameters, options);
            //var result = await dialog.Result;
            //if (!result.Cancelled)
            //{
            //    var request = new UpdateProfilePictureRequest { Data = null, FileName = string.Empty, UploadType = Application.Enums.UploadType.ProfilePicture };
            //    var data = await _accountManager.UpdateProfilePictureAsync(request, UserId);
            //    if (data.Succeeded)
            //    {
            //        await _localStorage.RemoveItemAsync(StorageConstants.Local.UserImageURL);
            //        ImageDataUrl = string.Empty;
            //        _snackBar.Add("Profile picture deleted.", Severity.Success);
            //        _navigationManager.NavigateTo("/account", true);
            //    }
            //    else
            //    {
            //        foreach (var error in data.Messages)
            //        {
            //            _snackBar.Add(error, Severity.Error);
            //        }
            //    }
            //}
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

using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Account;
using Harmony.Shared.Storage;
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
            //var _file = e.File;
            //if (_file != null)
            //{
            //    var extension = Path.GetExtension(_file.Name);
            //    var fileName = $"{UserId}-{Guid.NewGuid()}{extension}";
            //    var format = "image/png";
            //    var imageFile = await e.File.RequestImageFileAsync(format, 400, 400);
            //    var buffer = new byte[imageFile.Size];
            //    await imageFile.OpenReadStream().ReadAsync(buffer);
            //    var request = new UpdateProfilePictureRequest { Data = buffer, FileName = fileName, Extension = extension, UploadType = Application.Enums.UploadType.ProfilePicture };
            //    var result = await _accountManager.UpdateProfilePictureAsync(request, UserId);
            //    if (result.Succeeded)
            //    {
            //        await _localStorage.SetItemAsync(StorageConstants.Local.UserImageURL, result.Data);
            //        _snackBar.Add(_localizer["Profile picture added."], Severity.Success);
            //        _navigationManager.NavigateTo("/account", true);
            //    }
            //    else
            //    {
            //        foreach (var error in result.Messages)
            //        {
            //            _snackBar.Add(error, Severity.Error);
            //        }
            //    }
            //}
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


    }
}

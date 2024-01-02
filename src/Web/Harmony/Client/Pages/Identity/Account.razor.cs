using Harmony.Application.Features.Users.Commands.UpdatePassword;
using Harmony.Application.Features.Users.Commands.UpdateProfile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Client.Infrastructure.Models.Account;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Harmony.Client.Pages.Identity
{
    public partial class Account
    {
        #region profile

        private UserModel _user = new();
        private bool _updating = false;
        private ChangePasswordModel _changePassword = new();
        private bool _updatingPassword = false;
        private bool _uploadingImage = false;

        private bool _passwordVisibility;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        #endregion

        #region notifications

        private HashSet<EmailNotificationType> selectedNotifications = new HashSet<EmailNotificationType>();
        private List<EmailNotificationType> _notifications = Enum.GetValues<EmailNotificationType>().ToList();
        private List<EmailNotificationType> _userNotifications = Enumerable.Empty<EmailNotificationType>().ToList();

        #endregion

        protected async override Task OnInitializedAsync()
        {
            var user = await _authenticationManager.CurrentUser();
            var userId = user.Claims
                    .FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var result = await _userManager.GetAsync(userId) ;
            
            if (result.Succeeded)
            {
                var userNotificationsResult = await _userNotificationManager.GetNotificationsAsync(userId);

                _user = _mapper.Map<UserModel>(result.Data);

                if(userNotificationsResult.Succeeded)
                {
                    selectedNotifications = userNotificationsResult.Data.ToHashSet();
                }
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

        private async Task UpdateNotifications()
        {
            _updating = true;

            var result = await _userNotificationManager.SetNotificationsAsync(selectedNotifications.ToList());

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

        private string GetPresentation(EmailNotificationType type)
        {
            return ToSentenceCase(type.ToString());
        }

        private string GetDescription(EmailNotificationType type)
        {
            switch (type)
            {
                case EmailNotificationType.CardDueDateUpdated:
                    return "A card's due date has been updated. You must be assigned to that card";
                case EmailNotificationType.CardCompleted:
                    return "A card has been moved to a list marked as DONE";
                case EmailNotificationType.MemberAddedToCard:
                    return "You are being assigned to a card";
                case EmailNotificationType.MemberRemovedFromCard:
                    return "You are no longer assigned to a card";
                case EmailNotificationType.CommentAddedToCard:
                    return "Someone added a comment to a card";
                case EmailNotificationType.AttachmentAddedToCard:
                    return "Attachement added to a card";
                case EmailNotificationType.CardAddedToBoard:
                    return "A new card is added to the board";
                case EmailNotificationType.MemberAddedToBoard:
                    return "You are being added to a board";
                case EmailNotificationType.MemberRemovedFromBoard:
                    return "You are no longer member of a board";
                case EmailNotificationType.MemberAddedToWorkspace:
                    return "You become member of a workspace";
                case EmailNotificationType.MemberRemovedFromWorkspace:
                    return "You are no longer member of a workspace";

                default:
                    return string.Empty;
            }
        }

        private void OnSearch(string text)
        {
            if(string.IsNullOrEmpty(text.Trim()))
            {
                _notifications = Enum.GetValues<EmailNotificationType>().ToList();
            }
            else
            {
                var filteredNotifications = new List<EmailNotificationType>();
                foreach(var notification in Enum.GetValues<EmailNotificationType>().AsEnumerable())
                {
                    if(ToSentenceCase(notification.ToString().ToLower())
                        .Contains(text.Trim().ToLower()))
                    {
                        filteredNotifications.Add(notification);
                    }
                }

                _notifications = filteredNotifications;
            }
            //_table.ReloadServerData();
        }

        public static string ToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
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

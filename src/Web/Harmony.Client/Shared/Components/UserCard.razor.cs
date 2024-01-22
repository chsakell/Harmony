using Harmony.Client.Extensions;
using Harmony.Shared.Storage;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Shared.Components
{
    public partial class UserCard
    {
        [Parameter] public string Class { get; set; }

        private string Email { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string EmptyAvatarText { get; set; }

        [Parameter]
        public string ImageDataUrl { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;

            Email = user.GetEmail().Replace(".com", string.Empty);
            FirstName = user.GetFirstName();
            LastName = user.GetLastName();

            if (FirstName.Length > 0 && LastName.Length > 0)
            {
                EmptyAvatarText = $"{FirstName[0]}{LastName[0]}";
            }

            var UserId = user.GetUserId();
            var imageResponse = await _localStorage.GetItemAsync<string>(StorageConstants.Local.UserImageURL);
            if (!string.IsNullOrEmpty(imageResponse))
            {
                ImageDataUrl = imageResponse;
            }
            StateHasChanged();
        }
    }
}
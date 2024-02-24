using Harmony.Client.Extensions;
using Harmony.Client.Infrastructure.Configuration;
using Harmony.Client.Infrastructure.Managers.Identity.Roles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using Polly.Registry;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Shared
{
    public partial class MainBody
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback OnDarkModeToggle { get; set; }

        [Parameter]
        public EventCallback<bool> OnRightToLeftToggle { get; set; }

        private bool _drawerOpen = true;
        MudMenu _menu;
        [Inject] private IRoleManager RoleManager { get; set; }
        [Inject] private ClientConfiguration ClientConfiguration { get; set; }
        [Inject] private ResiliencePipelineProvider<string> ResiliencePipelineProvider { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        private string CurrentUserId { get; set; }
        private string ImageDataUrl { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string Email { get; set; }
        private string EmptyAvatarText { get; set; }

        public async Task ToggleDarkMode()
        {
            await OnDarkModeToggle.InvokeAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            _interceptor.RegisterEvent();

            var pipeline = ResiliencePipelineProvider.GetPipeline(HarmonyRetryPolicy.WaitAndRetry);

            hubConnection = await _hubSubscriptionManager
                .StartAsync(_navigationManager, _localStorage, ClientConfiguration.GatewayUrl,
                pipeline);

            _fileManager.OnUserProfilePictureUpdated += OnUserProfilePictureUpdated;
        }

        private void OnUserProfilePictureUpdated(object? sender, Application.Events.UserProfilePictureUpdated e)
        {
            ImageDataUrl = e.ProfilePicture;
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadDataAsync();
            }
        }

        private void ViewAccount()
        {
            _menu.CloseMenu();
            _navigationManager.NavigateTo("/account/");
        }

        private async Task LoadDataAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user == null) return;
            if (user.Identity?.IsAuthenticated == true)
            {
                if (string.IsNullOrEmpty(CurrentUserId))
                {
                    CurrentUserId = user.GetUserId();
                    FirstName = user.GetFirstName();
                    LastName = user.GetLastName();
                    if (FirstName.Length > 0 && LastName.Length > 0)
                    {
                        EmptyAvatarText = $"{FirstName[0]}{LastName[0]}";
                    }

                    Email = user.GetEmail();

                    var currentUserResult = await _userManager.GetAsync(CurrentUserId);
                    if (currentUserResult.Succeeded && currentUserResult.Data != null)
                    {
                        ImageDataUrl = currentUserResult.Data.ProfilePicture;
                        StateHasChanged();
                    }
                    else if (!currentUserResult.Succeeded || currentUserResult.Data == null)
                    {
                        CurrentUserId = string.Empty;
                        ImageDataUrl = string.Empty;
                        FirstName = string.Empty;
                        LastName = string.Empty;
                        Email = string.Empty;
                        EmptyAvatarText = string.Empty;
                        await _authenticationManager.Logout();
                    }
                }
            }
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        private void Logout()
        {
            var parameters = new DialogParameters
            {
                {nameof(Dialogs.Logout.ContentText), $"{"Are you sure you want to logout?"}"},
                {nameof(Dialogs.Logout.ButtonText), $"{"Logout"}"},
                {nameof(Dialogs.Logout.Color), Color.Error},
                {nameof(Dialogs.Logout.CurrentUserId), CurrentUserId},
                {nameof(Dialogs.Logout.HubConnection), hubConnection}
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

            _dialogService.Show<Dialogs.Logout>("Logout", parameters, options);
        }

        private HubConnection hubConnection;
        public bool IsConnected => hubConnection.State == HubConnectionState.Connected;
    }
}
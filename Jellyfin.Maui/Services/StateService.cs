using System;
using Jellyfin.Maui.Models;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services
{
    /// <inheritdoc />
    public class StateService : IStateService
    {
        private readonly StateModel _state;
        private readonly SdkClientSettings _sdkClientSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateService"/> class.
        /// </summary>
        /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
        public StateService(
            SdkClientSettings sdkClientSettings)
        {
            _sdkClientSettings = sdkClientSettings;
            // TODO load from disk
            _state = new StateModel();
            _sdkClientSettings.BaseUrl = _state.Host;
            _sdkClientSettings.AccessToken = _state.Token;
        }

        /// <inheritdoc />
        public void SetAuthenticationResponse(
            string host,
            AuthenticationResult authenticationResult)
        {
            _sdkClientSettings.BaseUrl = host;
            _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
            _state.Host = host.TrimEnd('/');
            _state.UserDto = authenticationResult.User;
            _state.Token = authenticationResult.AccessToken;
        }

        /// <inheritdoc />
        public void SetUser(UserDto userDto)
        {
            _state.UserDto = userDto;
        }

        /// <inheritdoc />
        public UserDto GetUser()
        {
            return _state.UserDto ?? throw new UnauthorizedAccessException();
        }

        /// <inheritdoc />
        public Guid GetUserId()
        {
            return _state.UserDto?.Id ?? throw new UnauthorizedAccessException();
        }

        /// <inheritdoc />
        public string GetHost()
        {
            return _state.Host ?? throw new UnauthorizedAccessException();
        }

        /// <inheritdoc />
        public StateModel GetState()
        {
            return _state;
        }

        /// <inheritdoc />
        public void ClearState()
        {
            _state.UserDto = null;
            _state.Host = null;
            _state.Token = null;
        }
    }
}
using Barbora.Core.Extensions;
using Barbora.Core.Models;
using Barbora.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace Barbora.Core.Clients
{
    public interface IBarboraApiClient
    {
        void LogIn(Cookie authCookie);

        Task LogInAsync(string email, string password, bool rememberMe);

        Task<AddressResponse> GetAddressAsync();
        Task<ChangeDeliveryAddressResponse> ChangeDeliveryAddressAsync(string deliveryAddressId);
        Task<DeliveriesResponse> GetDeliveriesAsync();
        Task<DeliveriesResponse> ReserveDeliveryTimeSlotAsync(string dayId, string hourId, bool isExpressDeliveryTimeslot);

        event EventHandler<Cookie> OnAuthCookieSet;

        void Dispose();
    }

    public class BarboraApiClient : IBarboraApiClient, IDisposable
    {
        private const string AuthCookieName = ".BRBAUTH";

        private const string ApiAuthScheme = "Basic";
        private const string ApiAuthParameter = "YXBpa2V5OlNlY3JldEtleQ==";

        #region Base URL and endpoints

        private const string BarboraUrl = "https://www.barbora.lt";

        private const string LoginEndpoint = "api/eshop/v1/user/login";
        private const string AddressEndpoint = "api/eshop/v1/user/address";
        private const string ChangeDeliveryAddressEndpoint = "api/eshop/v1/cart/changeDeliveryAddress";
        private const string DeliveriesEndpoint = "api/eshop/v1/cart/deliveries";
        private const string ReserveDeliveryTimeSlotEndpoint = "api/eshop/v1/cart/ReserveDeliveryTimeSlot";

        #endregion

        public event EventHandler<Cookie> OnAuthCookieSet;

        private string Email { get; set; }
        private string Password { get; set; }
        private bool RememberMe { get; set; }

        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;
        private Cookie authCookie;

        public BarboraApiClient()
        {
            cookieContainer = new CookieContainer();

            cookieContainer.Add(new Uri(BarboraUrl), new Cookie("region", "barbora.lt"));

            var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };

            httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(BarboraUrl)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ApiAuthScheme, ApiAuthParameter);
        }

        public BarboraApiClient(string email, string password, bool rememberMe)
            : this()
        {
            ValidateAndSetCredentials(email, password, rememberMe);
        }

        private void SetCredentials(string email, string password, bool rememberMe)
        {
            ValidateAndSetCredentials(email, password, rememberMe);
        }

        private void ValidateAndSetCredentials(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            Email = email;
            Password = password;
            RememberMe = rememberMe;
        }

        /// <summary>
        /// Base GET request method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        private async Task<T> GetAsync<T>(string requestUri)
            where T : class
        {
            await CheckAndLogInAsync();

            var response = await httpClient.GetAsync(requestUri);

            ReadAuthCookie();

            return await response.ProcessResponseAsync<T>();
        }

        /// <summary>
        /// Base PUT request method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<T> PutAsync<T>(string requestUri, FormUrlEncodedContent content)
            where T : class
        {
            await CheckAndLogInAsync();

            var response = await httpClient.PutAsync(requestUri, content);

            ReadAuthCookie();

            return await response.ProcessResponseAsync<T>();
        }

        /// <summary>
        /// Base POST request method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<T> PostAsync<T>(string requestUri, FormUrlEncodedContent content)
            where T : class
        {
            await CheckAndLogInAsync();

            var response = await httpClient.PostAsync(requestUri, content);

            ReadAuthCookie();

            return await response.ProcessResponseAsync<T>();
        }

        #region Authentication

        private void ReadAuthCookie()
        {
            var responseCookies = cookieContainer?.GetCookies(new Uri(BarboraUrl))?.Cast<Cookie>();
            var authCookie = responseCookies?.Where(x => x.Name == AuthCookieName)?.FirstOrDefault();

            if (authCookie == null)
                throw new SecurityException(string.Format("Auth cookie \"{0}\" was not found", AuthCookieName));

            SetAuthCookie(authCookie);
        }

        private void SetAuthCookie(Cookie authCookie)
        {
            // TODO: [refactor these datetime things]
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            var utcTime = authCookie.Expires;
            var utcTimeSpan = TimeSpan.FromTicks(utcTime.Ticks);
            var expiresTimeSpan = utcTimeSpan.Add(-timeZoneInfo.BaseUtcOffset);

            authCookie.Expires = new DateTime(expiresTimeSpan.Ticks);
            // TODO: [refactor till here]

            DebugUtils.WriteLineToDebugConsole(string.Format("Setting new auth cookie. Cookie expires at {0}", authCookie.Expires.ToString("yyyy-MM-dd HH:mm:ss")));

            OnAuthCookieSet?.Invoke(this, authCookie);

            this.authCookie = authCookie;
        }

        private async Task CheckAndLogInAsync()
        {
            if (!(authCookie == null || authCookie.Expires <= DateTime.Now))
                return;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                throw new SecurityException("You're not logged in");
            else
                await LogInAsync(Email, Password, RememberMe);
        }

        #endregion      

        public async Task LogInAsync(string email, string password, bool rememberMe)
        {
            SetCredentials(email, password, rememberMe);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", Email),
                new KeyValuePair<string, string>("password", Password),
                new KeyValuePair<string, string>("rememberMe", RememberMe ? "true" : "false")
            });

            var response = await httpClient.PostAsync(LoginEndpoint, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var loginResponse = await response.ProcessResponseAsync<LoginResponse>();

                if (loginResponse.success)
                    ReadAuthCookie();
            }
            else
                await response.ProcessResponseWhenErrorAsync();
        }

        public void LogIn(Cookie authCookie)
        {
            if (authCookie == null)
                throw new ArgumentNullException("authCookie");

            cookieContainer.Add(new Uri(BarboraUrl), authCookie);

            this.authCookie = authCookie;
        }

        public async Task<AddressResponse> GetAddressAsync()
        {
            return await GetAsync<AddressResponse>(AddressEndpoint);
        }

        public async Task<ChangeDeliveryAddressResponse> ChangeDeliveryAddressAsync(string deliveryAddressId)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("deliveryAddressId", deliveryAddressId),
                new KeyValuePair<string, string>("isWebRequest", "true"),
                new KeyValuePair<string, string>("forceToChangeAddressOnFirstTry", "false")
            });

            return await PutAsync<ChangeDeliveryAddressResponse>(ChangeDeliveryAddressEndpoint, content);
        }

        public async Task<DeliveriesResponse> GetDeliveriesAsync()
        {
            return await GetAsync<DeliveriesResponse>(DeliveriesEndpoint);
        }

        public async Task<DeliveriesResponse> ReserveDeliveryTimeSlotAsync(string dayId, string hourId, bool isExpressDeliveryTimeslot)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("dayId", dayId),
                new KeyValuePair<string, string>("hourId", hourId),
                new KeyValuePair<string, string>("isExpressDeliveryTimeslot", isExpressDeliveryTimeslot ? "true" : "false")
            });

            return await PostAsync<DeliveriesResponse>(ReserveDeliveryTimeSlotEndpoint, content);
        }

        public void Dispose()
        {
            if (httpClient != null)
                httpClient.Dispose();
        }
    }
}
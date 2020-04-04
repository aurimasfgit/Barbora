using Barbora.Core.Extensions;
using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Barbora.Core.Clients
{
    public interface IBarboraApiClient
    {
        Task LogInAsync(string email, string password, bool rememberMe);

        Task<AddressResponse> GetAddressAsync();
        Task<ChangeDeliveryAddressResponse> ChangeDeliveryAddressAsync(string deliveryAddressId);
        Task<DeliveriesResponse> GetDeliveriesAsync();
        Task<DeliveriesResponse> ReserveDeliveryTimeSlotAsync(string dayId, string hourId, bool isExpressDeliveryTimeslot);

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

        private string Email { get; set; }
        private string Password { get; set; }
        private bool RememberMe { get; set; }

        private HttpClient httpClient;
        private CookieContainer cookieContainer;
        private Cookie authCookie;

        public BarboraApiClient()
        {
            cookieContainer = new CookieContainer();

            cookieContainer.Add(new Uri(BarboraUrl), new Cookie("region", "barbora.lt"));
            cookieContainer.Add(new Uri(BarboraUrl), new Cookie("permissionToUseCookies", "true"));

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

        public void SetCredentials(string email, string password, bool rememberMe)
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

            return await response.ProcessResponseAsync<T>();
        }

        #region Authentication

        private void SetAuthCookie(Cookie authCookie)
        {
            // TODO: [refactor this method - this look not very good...]
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            var utcTime = authCookie.Expires;
            var utcTimeSpan = TimeSpan.FromTicks(utcTime.Ticks);
            var expiresTimeSpan = utcTimeSpan.Add(-timeZoneInfo.BaseUtcOffset);

            authCookie.Expires = new DateTime(expiresTimeSpan.Ticks);

            Debug.WriteLine(string.Format("{0} - logging in. Cookie expires: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), authCookie.Expires.ToString("yyyy-MM-dd HH:mm:ss")));

            this.authCookie = authCookie;
        }

        private async Task CheckAndLogInAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                throw new FriendlyException("Neįvestas el. pašto adresas ir / arba slaptažodis");

            // check if login is needed
            if (!(authCookie == null || authCookie.Expired))
                return;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", Email),
                new KeyValuePair<string, string>("password", Password),
                new KeyValuePair<string, string>("rememberMe", RememberMe ? "true" : "false")
            });

            var response = await httpClient.PostAsync(LoginEndpoint, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseCookies = cookieContainer.GetCookies(new Uri(BarboraUrl)).Cast<Cookie>();
                var authCookie = responseCookies.Where(x => x.Name == AuthCookieName).FirstOrDefault();

                if (authCookie == null)
                    throw new Exception(string.Format("Got error while logging in. Cookie \"{0}\" was not found.", AuthCookieName));

                SetAuthCookie(authCookie);
            }
            else
                await response.ProcessResponseWhenErrorAsync();
        }

        #endregion      

        public async Task LogInAsync(string email, string password, bool rememberMe)
        {
            if (!(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)))
                SetCredentials(email, password, rememberMe);

            await CheckAndLogInAsync();
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
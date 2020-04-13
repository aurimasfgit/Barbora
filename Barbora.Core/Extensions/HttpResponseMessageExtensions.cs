using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

namespace Barbora.Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> ProcessResponseAsync<T>(this HttpResponseMessage response)
             where T : class
        {
            if (response.StatusCode == HttpStatusCode.OK)
                return await ProccessResponseWhenOkAsync<T>(response);
            else
                await ProcessResponseWithErrorsAsync(response);

            return null;
        }

        public static async Task ProcessResponseWhenErrorAsync(this HttpResponseMessage response)
        {
            await ProcessResponseWithErrorsAsync(response);
        }

        #region Private methods

        private static async Task<T> ProccessResponseWhenOkAsync<T>(HttpResponseMessage response)
            where T : class
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
                return JsonConvert.DeserializeObject<T>(content);

            return null;
        }

        private static async Task ProcessResponseWithErrorsAsync(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var errorContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(errorContent))
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                var error = errorResponse?.messages?.GetFirstError();

                if (error != null && error.Id == "access_denied")
                    throw new SecurityException(error.message);

                if (!string.IsNullOrEmpty(error?.message))
                    throw new FriendlyException(error?.message);
            }

            throw new Exception(string.Format("Error processing response. Status code: {0}", response.StatusCode.ToString()));
        }

        #endregion
    }
}
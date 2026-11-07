using Shared.Responses;

namespace WebApi.Config
{
    /// <summary>
    /// Public error messages of the API. Exception details never leave the server.
    /// </summary>
    public static class ApiErrorMessages
    {
        public const string InvalidRequest = "The request is invalid.";
        public const string RequestFailed = "The request could not be completed.";
        public const string UnexpectedError = "An unexpected error occurred.";
        public const string ResourceNotFound = "The requested resource was not found.";
        public const string MethodNotAllowed = "The HTTP method is not allowed for this route.";
        public const string UnsupportedMediaType = "The request content type is not supported.";

        /// <summary>
        /// Returns the service message for business failures. Responses that carry an exception use a generic message.
        /// </summary>
        /// <param name="response">Service response that failed.</param>
        public static string FromResponse(Response response) => response.Exception == null ? response.Message : RequestFailed;
    }
}

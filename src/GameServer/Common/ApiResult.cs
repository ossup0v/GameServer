namespace GameServer.Common
{
    public enum ApiResultStatus
    { 
        Ok,
        Error,
        Failed
    }

    public class ApiResult
    {
        public ApiResultStatus Status { get; }
        public string Message { get; }
        public ApiResult(ApiResultStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        public static ApiResult Failed(string message = "") => new ApiResult(ApiResultStatus.Failed, message);
        public static ApiResult Error(string message = "") => new ApiResult(ApiResultStatus.Error, message);
        public static ApiResult Ok() => new ApiResult(ApiResultStatus.Ok);
    }


    public class ApiResult<T> : ApiResult
    {

        public ApiResult(ApiResultStatus status, string message = null) : base(status, message)
        {

        }

        public ApiResult(T value) : base(ApiResultStatus.Ok)
        {
            Value = value;
        }

        public ApiResult(T value, ApiResultStatus status, string message = null) : base(status, message)
        {
            Value = value;
        }


        public T Value { get; set; }

        public static ApiResult<T> OK(T value) { return new ApiResult<T>(value, ApiResultStatus.Ok); }
        public static ApiResult<T> Error(string message) { return new ApiResult<T>(ApiResultStatus.Error, message); }
        public static ApiResult<T> Failed(string message) { return new ApiResult<T>(ApiResultStatus.Failed, message); }
    }
}

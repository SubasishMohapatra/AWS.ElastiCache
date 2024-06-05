using System.Net;

namespace Huron.AWS.Common.Core
{
    public class CachingResponse<T>
    {        
        public CachingResponse() { }
        public CachingResponse(T value, HttpStatusCode HttpStatusCode, string Message)
        {
            this.Result=value;
            this.HttpStatusCode=HttpStatusCode;
            this.Message = Message;
        }

        public string Message { get; set; } = "Successful";

        public HttpStatusCode HttpStatusCode { get; set; }

        public T Result { get; set; } = default!;
    }
}
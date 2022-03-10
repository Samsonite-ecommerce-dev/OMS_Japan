using System;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.ECommerce
{
    public class ECommerceException : Exception
    {
        public ECommerceException() { }
        public ECommerceException(string objErrorCode, string objErrorMessage) : base($"ErrorCode:{objErrorCode},ErrorMesssage:{objErrorMessage}") { }
        public ECommerceException(string objErrorMessage) : base($"ErrorMesssage:{objErrorMessage}") { }
    }
}
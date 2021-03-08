using blazorTest.Shared;
using System;

namespace blazorTest.Server.Exceptions
{
    internal class HttpResponseException : Exception
    {
        public int Status { get; init; } = 500;
        public ErrorType ErrorType { get; init; }
        public object Value { get; init; }
    }
}
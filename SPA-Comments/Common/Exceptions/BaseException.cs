using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions;

public class BaseException : Exception
{
    public string? Place { get; set; }

    public string? Title { get; set; }

    public int? HttpCode { get; set; }

    public BaseException()
    { }

    public BaseException(string message) : base(message)
    { }

    public BaseException(string message, HttpStatusCode code) : base(message)
    {
        HttpCode = (int)code;
    }

    public BaseException(string message, int code) : base(message)
    {
        HttpCode = code;
    }

    public BaseException(string message, Exception innerException) : base(message, innerException)
    { }

    public BaseException(string message, string place, string? title) : base(message)
    {
        Place = place;
        Title = title;
    }

    public BaseException(string message, string place, string? title, HttpStatusCode code) : base(message)
    {
        HttpCode = (int)code;
        Place = place;
        Title = title;
    }

    public BaseException(string message, string place, string? title, int code) : base(message)
    {
        HttpCode = code;
        Place = place;
        Title = title;
    }
}

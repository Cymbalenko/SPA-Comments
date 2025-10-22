using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Exceptions;

/// <summary>
/// Клас, що представляє об'єкт відповіді на помилку
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Код помилки
    /// </summary>
    [JsonIgnore]
    [Description("Код помилки")]
    public HttpStatusCode ResponseCode { get; set; }

    /// <summary>
    /// Заголовок помилки
    /// </summary>
    [Description("Заголовок помилки")]
    public string? Title { get; set; }

    /// <summary>
    /// Місце виникнення помилки
    /// </summary>
    [Description("Місце де виникла помилка (мето чи АПІ)")]
    public string? Place { get; set; }

    /// <summary>
    /// Повідомлення помилки
    /// </summary>
    [Description("Повідомлення помилки")]
    public string? Message { get; set; }

    /// <summary>
    /// Позначає, чи є операція успішною
    /// </summary>
    [Description("Позначає, чи є операція успішною")]
    public bool? Success { get; set; }
}

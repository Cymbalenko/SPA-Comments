using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Captcha;

public interface ICaptchaService
{
    public (byte[] Image, string Text) GenerateCaptcha();
}

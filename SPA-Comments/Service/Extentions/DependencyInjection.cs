using Microsoft.Extensions.DependencyInjection;
using Service.Comment;
using Service.Services.Comment;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Service.Messaging;
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
    public static IServiceCollection AddBlServices(this IServiceCollection services)
    {
        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddScoped<ICommentService, CommentService>(); 

        return services;
    }
}

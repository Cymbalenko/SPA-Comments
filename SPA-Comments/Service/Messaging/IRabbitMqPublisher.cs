using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Messaging;

public interface IRabbitMqPublisher
{
    public Task PublishAsync<T>(string exchange, string routingKey, T message);
}

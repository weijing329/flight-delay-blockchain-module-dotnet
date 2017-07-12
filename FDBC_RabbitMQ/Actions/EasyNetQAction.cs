using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FDBC_Shared.DTO;
using Newtonsoft.Json;

namespace FDBC_RabbitMQ.Actions
{
  public static class EasyNetQActions
  {
    public static async Task OnReceiving_I2B_Request(I2B_Request request)
    {
      dynamic obj = JsonConvert.DeserializeObject(request.task.payload);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FDBC_Shared.DTO;
using Newtonsoft.Json;
using EasyNetQ;

namespace FDBC_RabbitMQ.MessageHandlers
{
  public class EasyNetQMessageHandling
  {
    public static async Task OnReceiving_I2B_Request(IMessage<I2B_Request> msg, MessageReceivedInfo info)
    {
      I2B_Request request = msg.Body;

      switch (request.task.name)
      {
        case "createNewBlockchainPolicy":
          CreatePolicy create_policy = JsonConvert.DeserializeObject<CreatePolicy>(request.task.payload);
          break;

        case "createNewBlockchainFlight":
          CreateFlight create_flight = JsonConvert.DeserializeObject<CreateFlight>(request.task.payload);
          break;

        case "deleteBlockchainFlight":
          DeleteFlight delete_flight = JsonConvert.DeserializeObject<DeleteFlight>(request.task.payload);
          break;

        case "updateBlockchainPolicy":
          UpdatePolicy update_policy = JsonConvert.DeserializeObject<UpdatePolicy>(request.task.payload);
          break;

        default:
          break;
      }
    }

    //public static 
  }
}

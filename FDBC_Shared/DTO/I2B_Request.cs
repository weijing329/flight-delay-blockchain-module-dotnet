using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDBC_Shared.DTO
{
  public class I2B_Request
  {
    public string task_uuid { get; set; }
    public I2B_Request_Task task { get; set; }
  }

  public class I2B_Request_Task
  {
    public string name { get; set; }
    public string payload { get; set; }
  }

}

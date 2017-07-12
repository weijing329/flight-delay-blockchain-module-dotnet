using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDBC_Shared.DTO
{
  public class I2B_Request
  {
    public string task_uuid { get; set; }
    public Task1 task { get; set; }
  }

  public class Task1
  {
    public string name { get; set; }
    public string payload { get; set; }
  }
}

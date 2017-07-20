using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDBC_Shared.DTO
{
  public class B2I_Response
  {
    public string task_uuid { get; set; }
    public B2I_Response_Task task { get; set; }
  }

  public class B2I_Response_Task
  {
    public string name { get; set; }
    public string payload { get; set; }
  }
}

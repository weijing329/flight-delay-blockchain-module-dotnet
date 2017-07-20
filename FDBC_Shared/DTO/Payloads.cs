using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDBC_Shared.DTO
{
  public class CreateFlight
  {
    public int pid { get; set; }
    public int ufid { get; set; }
    public string flight_code { get; set; }
    public string fs_flight_code { get; set; }
    public string departure_airport { get; set; }
    public string arrival_airport { get; set; }
    public int departure_utc_offset_hours { get; set; }
    public int arrival_utc_offset_hours { get; set; }
    public int scheduled_departure_date { get; set; }
    public DateTime scheduled_departure_date_time { get; set; }
    public DateTime scheduled_departure_date_time_local { get; set; }
    public DateTime scheduled_arrival_date_time { get; set; }
    public DateTime scheduled_arrival_date_time_local { get; set; }
    public string hash { get; set; }
  }



  public class CreatePolicy
  {
    public string status { get; set; }
    public bool deleted { get; set; }
    public int version { get; set; }
    public int pid { get; set; }
    public string psn { get; set; }
    public int tenant_id { get; set; }
    public DateTime start_date_time { get; set; }
    public DateTime end_date_time { get; set; }
    public string start_date_time_local { get; set; }
    public string end_date_time_local { get; set; }
    public DateTime created_at { get; set; }
  }


  public class UpdatePolicy
  {
    public int pid { get; set; }
    public string psn { get; set; }
    public int tenant_id { get; set; }
    public DateTime start_date_time { get; set; }
    public DateTime end_date_time { get; set; }
    public string start_date_time_local { get; set; }
    public string end_date_time_local { get; set; }
    public string status { get; set; }
    public object contract_address { get; set; }
    public bool deleted { get; set; }
    public object creation_txhash { get; set; }
    public DateTime created_at { get; set; }
    public int version { get; set; }
  }


  public class DeleteFlight
  {
    public int flight_id { get; set; }
    public int pid { get; set; }
    public int ufid { get; set; }
    public string flight_code { get; set; }
    public string fs_flight_code { get; set; }
    public int departure_utc_offset_hours { get; set; }
    public int arrival_utc_offset_hours { get; set; }
    public string departure_airport { get; set; }
    public string arrival_airport { get; set; }
    public object status { get; set; }
    public string scheduled_departure_date { get; set; }
    public DateTime scheduled_departure_date_time { get; set; }
    public DateTime scheduled_departure_date_time_local { get; set; }
    public object actual_departure_date_time { get; set; }
    public object actual_departure_date_time_local { get; set; }
    public DateTime scheduled_arrival_date_time { get; set; }
    public DateTime scheduled_arrival_date_time_local { get; set; }
    public object actual_arrival_date_time { get; set; }
    public object actual_arrival_date_time_local { get; set; }
    public object cancel_date_time { get; set; }
    public object cancel_date_time_local { get; set; }
    public string hash { get; set; }
    public object contract_address { get; set; }
    public object flight_status_source { get; set; }
    public bool flight_status_fed { get; set; }
    public object flight_status_confirmed_txhash { get; set; }
    public object delay_notification_date_time { get; set; }
    public bool deleted { get; set; }
    public object creation_txhash { get; set; }
    public DateTime created_at { get; set; }
    public int version { get; set; }
  }
}

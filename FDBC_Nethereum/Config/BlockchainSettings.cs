using System;
using System.Collections.Generic;
using System.Text;

namespace FDBC_Nethereum.Config
{
  public class BlockchainSettings
  {
    public string rpcapi_host { get; set; }
    public string default_sender_address { get; set; }
    public string default_sender_password { get; set; }
    public int default_retry_in_ms { get; set; }
    public string flight_contract_abi { get; set; }
    public string flight_contract_bytecode { get; set; }
    public int flight_contract_deploy_gas { get; set; }
    public int flight_contract_set_all_gas { get; set; }
    public string policy_contract_abi { get; set; }
    public string policy_contract_bytecode { get; set; }
    public int policy_contract_deploy_gas { get; set; }
    public int policy_contract_set_all_gas { get; set; }
  }
}

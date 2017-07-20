using System;
using Nethereum.JsonRpc.Client;

namespace FDBC_Nethereum.DotNetWeb3Geth
{
  public class ClientFactory
  {
    public static IClient GetClient()
    {
      //string blockchain_connection_string = "http://localhost:8545";
      string blockchain_connection_string = "http://kflight.fintechtw.com:8545";

      return new RpcClient(new Uri(blockchain_connection_string));
    }
  }
}

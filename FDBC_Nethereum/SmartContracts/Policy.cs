﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Nethereum.Geth;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.ABI.FunctionEncoding.Attributes;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.JsonRpc.Client;

using Nethereum.ABI.Encoders;

using FDBC_Nethereum.DotNetWeb3Geth;
using Newtonsoft.Json;


namespace FDBC_Nethereum.SmartContracts
{
  public class Policy
  {
    private readonly Web3Geth _web3geth;
    private readonly string _default_sender_address;
    private readonly string _default_sender_password;
    private readonly string _test_existing_contract_address;
    private readonly int _default_retry_in_ms;
    private readonly string _contract_abi;
    private readonly string _contract_bytecode;

    public Policy(Web3Geth web3geth)
    {
      _web3geth = web3geth;
      _default_sender_address = "0x6376612e86c6f0be774cfa62e0eb732f87352115";
      _default_sender_password = "123";
      //_default_sender_address = "0x1b8fcde4948de04ab9d67600a145886a3544dfaa";
      //_default_sender_password = "dev_password";

      _default_retry_in_ms = 100;

      _contract_abi = @"[{'constant':false,'inputs':[],'name':'PostInit','outputs':[],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_deleted','outputs':[],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'start_date_time_local','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'status','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'start_date_time','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_status','outputs':[],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_start_date_time_local','outputs':[],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'psn','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'_start_date_time','type':'string'},{'name':'_end_date_time','type':'string'},{'name':'_start_date_time_local','type':'string'},{'name':'_end_date_time_local','type':'string'},{'name':'_status','type':'string'},{'name':'_deleted','type':'string'}],'name':'set_all','outputs':[],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'end_date_time_local','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'deleted','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_end_date_time','outputs':[],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_start_date_time','outputs':[],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'end_date_time','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'task_uuid','type':'string'},{'name':'input','type':'string'}],'name':'set_end_date_time_local','outputs':[],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'tenant_id','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'pid','outputs':[{'name':'','type':'string'}],'payable':false,'type':'function'},{'inputs':[{'name':'task_uuid','type':'string'},{'name':'_pid','type':'string'},{'name':'_psn','type':'string'},{'name':'_tenant_id','type':'string'},{'name':'_start_date_time','type':'string'},{'name':'_end_date_time','type':'string'},{'name':'_start_date_time_local','type':'string'},{'name':'_end_date_time_local','type':'string'}],'payable':false,'type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'policy_contract_address','type':'address'}],'name':'event_new_policy','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_start_date_time','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_end_date_time','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_start_date_time_local','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_end_date_time_local','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_status','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'task_uuid','type':'bytes32'},{'indexed':false,'name':'old_val','type':'string'},{'indexed':false,'name':'new_val','type':'string'}],'name':'event_set_deleted','type':'event'}]";
      _contract_bytecode = "606060405234156200001057600080fd5b604051620027d9380380620027d98339810160405280805182019190602001805182019190602001805182019190602001805182019190602001805182019190602001805182019190602001805182019190602001805190910190505b60008780516200008292916020019062000262565b5060018680516200009892916020019062000262565b506002858051620000ae92916020019062000262565b506003848051620000c492916020019062000262565b506004838051620000da92916020019062000262565b506005828051620000f092916020019062000262565b5060068180516200010692916020019062000262565b506200011f64010000000062000b48620001cb82021704565b876040518082805190602001908083835b602083106200015257805182525b601f19909201916020918201910162000130565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f536004647d57d2f9fffc6c6eb47cd5c07a456689b99ca2b759f12394a11f6a0f30604051600160a060020a03909116815260200160405180910390a25b50505050505050506200030c565b60408051908101604052600981527f6163746976617465640000000000000000000000000000000000000000000000602082015260079080516200021492916020019062000262565b5060408051908101604052600581527f66616c7365000000000000000000000000000000000000000000000000000000602082015260089080516200025e92916020019062000262565b505b565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f10620002a557805160ff1916838001178555620002d5565b82800160010185558215620002d5579182015b82811115620002d5578251825591602001919060010190620002b8565b5b50620002e4929150620002e8565b5090565b6200030991905b80821115620002e45760008155600101620002ef565b5090565b90565b6124bd806200031c6000396000f300606060405236156100ee5763ffffffff7c01000000000000000000000000000000000000000000000000000000006000350416630a2e48f681146100f35780631199fd881461010857806314198b1b1461019d578063200d2ed21461022857806328b35e4f146102b357806329c3c2951461033e5780633266f6ca146103d35780633af8f69b146104685780633f13bc13146104f357806350021af2146106d25780636b35f7c11461075d57806388ece7b1146107e85780638e5cc11e1461087d5780639c1e066f14610912578063a7ed431f1461099d578063d674161514610a32578063f106845414610abd575b600080fd5b34156100fe57600080fd5b610106610b48565b005b341561011357600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f016020809104026020016040519081016040528181529291906020840183838082843750949650610bdb95505050505050565b005b34156101a857600080fd5b6101b0610d68565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b341561023357600080fd5b6101b0610e06565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34156102be57600080fd5b6101b0610ea4565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b341561034957600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f016020809104026020016040519081016040528181529291906020840183838082843750949650610f4295505050505050565b005b34156103de57600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f0160208091040260200160405190810160405281815292919060208401838380828437509496506110cf95505050505050565b005b341561047357600080fd5b6101b061125c565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34156104fe57600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f0160208091040260200160405190810160405281815292919060208401838380828437509496506112fa95505050505050565b005b34156106dd57600080fd5b6101b0611c34565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b341561076857600080fd5b6101b0611cd2565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34156107f357600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f016020809104026020016040519081016040528181529291906020840183838082843750949650611d7095505050505050565b005b341561088857600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f016020809104026020016040519081016040528181529291906020840183838082843750949650611efd95505050505050565b005b341561091d57600080fd5b6101b061208a565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34156109a857600080fd5b61010660046024813581810190830135806020601f8201819004810201604051908101604052818152929190602084018383808284378201915050505050509190803590602001908201803590602001908080601f01602080910402602001604051908101604052818152929190602084018383808284375094965061212895505050505050565b005b3415610a3d57600080fd5b6101b06122b5565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b3415610ac857600080fd5b6101b0612353565b60405160208082528190810183818151815260200191508051906020019080838360005b838110156101ed5780820151818401525b6020016101d4565b50505050905090810190601f16801561021a5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b60408051908101604052600981527f616374697661746564000000000000000000000000000000000000000000000060208201526007908051610b8f9291602001906123f1565b5060408051908101604052600581527f66616c736500000000000000000000000000000000000000000000000000000060208201526008908051610bd79291602001906123f1565b505b565b816040518082805190602001908083835b60208310610c0c57805182525b601f199092019160209182019101610bec565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f7869ee11cc6ef6d36afa40dbb9213081f014daa6b79e7af8fd10f0ee632301c7600883604051604080825283546002600019610100600184161502019091160490820181905281906020820190606083019086908015610cdb5780601f10610cb057610100808354040283529160200191610cdb565b820191906000526020600020905b815481529060010190602001808311610cbe57829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b83811015610d135780820151818401525b602001610cfa565b50505050905090810190601f168015610d405780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26008818051610d629291602001906123f1565b505b5050565b60058054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b60078054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b60038054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b816040518082805190602001908083835b60208310610f7357805182525b601f199092019160209182019101610f53565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f775048f6e5511b48bdba9d7167a14398988fb5f1174516da0fea78bb16a7cc606007836040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156110425780601f1061101757610100808354040283529160200191611042565b820191906000526020600020905b81548152906001019060200180831161102557829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b8381101561107a5780820151818401525b602001611061565b50505050905090810190601f1680156110a75780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26007818051610d629291602001906123f1565b505b5050565b816040518082805190602001908083835b6020831061110057805182525b601f1990920191602091820191016110e0565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207fb54f212ba75dc7d45d15beed8db225a57535927d75224904de4127853ea36ef46005836040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156111cf5780601f106111a4576101008083540402835291602001916111cf565b820191906000526020600020905b8154815290600101906020018083116111b257829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b838110156112075780820151818401525b6020016111ee565b50505050905090810190601f1680156112345780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26005818051610d629291602001906123f1565b505b5050565b60018054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b866040518082805190602001908083835b6020831061132b57805182525b601f19909201916020918201910161130b565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f0a16ab5cf37fa2a5b28c57bf009795e95053cfd0b1093146a54691c4a0d6ae7e6003886040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156113fa5780601f106113cf576101008083540402835291602001916113fa565b820191906000526020600020905b8154815290600101906020018083116113dd57829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b838110156114325780820151818401525b602001611419565b50505050905090810190601f16801561145f5780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a2866040518082805190602001908083835b6020831061149f57805182525b601f19909201916020918201910161147f565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f2fb33a9dbfc9a90951b5e7dec08ebd43484893b4820b0af4805907c93378098060048760405160408082528354600260001961010060018416150201909116049082018190528190602082019060608301908690801561156e5780601f106115435761010080835404028352916020019161156e565b820191906000526020600020905b81548152906001019060200180831161155157829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b838110156115a65780820151818401525b60200161158d565b50505050905090810190601f1680156115d35780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a2866040518082805190602001908083835b6020831061161357805182525b601f1990920191602091820191016115f3565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207fb54f212ba75dc7d45d15beed8db225a57535927d75224904de4127853ea36ef46005866040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156116e25780601f106116b7576101008083540402835291602001916116e2565b820191906000526020600020905b8154815290600101906020018083116116c557829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b8381101561171a5780820151818401525b602001611701565b50505050905090810190601f1680156117475780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a2866040518082805190602001908083835b6020831061178757805182525b601f199092019160209182019101611767565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f73e64057ff2212006e5020dbe82240651058c6797bcbdcec592347b26e17e28f6006856040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156118565780601f1061182b57610100808354040283529160200191611856565b820191906000526020600020905b81548152906001019060200180831161183957829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b8381101561188e5780820151818401525b602001611875565b50505050905090810190601f1680156118bb5780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a2866040518082805190602001908083835b602083106118fb57805182525b601f1990920191602091820191016118db565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f775048f6e5511b48bdba9d7167a14398988fb5f1174516da0fea78bb16a7cc606007846040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156119ca5780601f1061199f576101008083540402835291602001916119ca565b820191906000526020600020905b8154815290600101906020018083116119ad57829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b83811015611a025780820151818401525b6020016119e9565b50505050905090810190601f168015611a2f5780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a2866040518082805190602001908083835b60208310611a6f57805182525b601f199092019160209182019101611a4f565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f7869ee11cc6ef6d36afa40dbb9213081f014daa6b79e7af8fd10f0ee632301c7600883604051604080825283546002600019610100600184161502019091160490820181905281906020820190606083019086908015611b3e5780601f10611b1357610100808354040283529160200191611b3e565b820191906000526020600020905b815481529060010190602001808311611b2157829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b83811015611b765780820151818401525b602001611b5d565b50505050905090810190601f168015611ba35780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26003868051611bc59291602001906123f1565b506004858051611bd99291602001906123f1565b506005848051611bed9291602001906123f1565b506006838051611c019291602001906123f1565b506007828051611c159291602001906123f1565b506008818051611c299291602001906123f1565b505b50505050505050565b60068054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b60088054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b816040518082805190602001908083835b60208310611da157805182525b601f199092019160209182019101611d81565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f2fb33a9dbfc9a90951b5e7dec08ebd43484893b4820b0af4805907c933780980600483604051604080825283546002600019610100600184161502019091160490820181905281906020820190606083019086908015611e705780601f10611e4557610100808354040283529160200191611e70565b820191906000526020600020905b815481529060010190602001808311611e5357829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b83811015611ea85780820151818401525b602001611e8f565b50505050905090810190601f168015611ed55780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26004818051610d629291602001906123f1565b505b5050565b816040518082805190602001908083835b60208310611f2e57805182525b601f199092019160209182019101611f0e565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f0a16ab5cf37fa2a5b28c57bf009795e95053cfd0b1093146a54691c4a0d6ae7e600383604051604080825283546002600019610100600184161502019091160490820181905281906020820190606083019086908015611ffd5780601f10611fd257610100808354040283529160200191611ffd565b820191906000526020600020905b815481529060010190602001808311611fe057829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b838110156120355780820151818401525b60200161201c565b50505050905090810190601f1680156120625780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26003818051610d629291602001906123f1565b505b5050565b60048054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b816040518082805190602001908083835b6020831061215957805182525b601f199092019160209182019101612139565b6001836020036101000a03801982511681845116179092525050509190910192506040915050519081900390207f73e64057ff2212006e5020dbe82240651058c6797bcbdcec592347b26e17e28f6006836040516040808252835460026000196101006001841615020190911604908201819052819060208201906060830190869080156122285780601f106121fd57610100808354040283529160200191612228565b820191906000526020600020905b81548152906001019060200180831161220b57829003601f168201915b5050838103825284818151815260200191508051906020019080838360005b838110156122605780820151818401525b602001612247565b50505050905090810190601f16801561228d5780820380516001836020036101000a031916815260200191505b5094505050505060405180910390a26006818051610d629291602001906123f1565b505b5050565b60028054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b60008054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610dfe5780601f10610dd357610100808354040283529160200191610dfe565b820191906000526020600020905b815481529060010190602001808311610de157829003601f168201915b505050505081565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061243257805160ff191683800117855561245f565b8280016001018555821561245f579182015b8281111561245f578251825591602001919060010190612444565b5b5061246c929150612470565b5090565b61248e91905b8082111561246c5760008155600101612476565b5090565b905600a165627a7a72305820e0179ec8bdcb16f79f408a9bd308110869057f3c5e494c43954bc3931dd882a70029";
    }

    public async Task<string> Create(
      string task_uuid,
      string pid, string psn,
      string tenant_id,
      string start_date_time, string end_date_time,
      string start_date_time_local, string end_date_time_local)
    {
      Web3Geth web3geth = _web3geth;
      string sender_address = _default_sender_address;
      string contract_abi = _contract_abi;
      string contract_bytecode = _contract_bytecode;

      ////====================================
      //// deploy contract

      // unlock for 120 secs

      //bool bool_result = await web3geth.Miner.Start.SendRequestAsync(120);
      string tx_hash = await web3geth.Eth.DeployContract.SendRequestAsync(
        abi: contract_abi,
        contractByteCode: contract_bytecode,
        from: sender_address,
        gas: new HexBigInteger(4700000),
        values: new object[] {
          task_uuid,
          pid, psn,
          tenant_id,
          start_date_time, end_date_time,
          start_date_time_local, end_date_time_local
        });

      int web3_transaction_check_delay_in_ms = _default_retry_in_ms;

      TransactionReceipt receipt = null;
      while (receipt == null)
      {
        await Task.Delay(web3_transaction_check_delay_in_ms);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
      }

      //bool_result = await web3geth.Miner.Stop.SendRequestAsync();

      string stringified_receipt = JsonConvert.SerializeObject(receipt);

      return stringified_receipt;
    }

    public async Task<Tuple<string, string>> SetPolicyAllAttributes(
      string contract_address,
      string task_uuid,
      string start_date_time,
      string end_date_time,
      string start_date_time_local,
      string end_date_time_local,
      string status,
      string deleted
      )
    {
      // SmartContract function doesn't take null as input for string
      start_date_time = start_date_time ?? "";
      end_date_time = end_date_time ?? "";
      start_date_time_local = start_date_time_local ?? "";
      end_date_time_local = end_date_time_local ?? "";
      status = status ?? "";
      deleted = deleted ?? "";

      // Web3
      Web3Geth web3geth = _web3geth;
      string sender_address = _default_sender_address;
      string contract_abi = _contract_abi;

      Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

      string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
      byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

      Function set_function = contract.GetFunction("set_all");

      var wei = new HexBigInteger(0);
      var tx_hash = await set_function.SendTransactionAsync(
        from: sender_address, gas: new HexBigInteger(4700000), value: wei,
        functionInput: new object[] {
          task_uuid,
          start_date_time,
          end_date_time,
          start_date_time_local,
          end_date_time_local,
          status,
          deleted
        });

      int web3_transaction_check_delay_in_ms = _default_retry_in_ms;

      TransactionReceipt receipt = null;
      while (receipt == null)
      {
        await Task.Delay(web3_transaction_check_delay_in_ms);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
      }

      string stringified_receipt = JsonConvert.SerializeObject(receipt);
      string stringified_event_log = "";

      return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    }

    public async Task<Tuple<string, string>> SetPolicyAttribute(string contract_address, string task_uuid, string attribute_name, string attribute_value)
    {
      Web3Geth web3geth = _web3geth;
      string sender_address = _default_sender_address;
      string contract_abi = _contract_abi;

      Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

      string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
      byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

      var set_event = contract.GetEvent($"event_set_{attribute_name}");
      var set_event_filter_by_task_uuid = await set_event.CreateFilterAsync(new[] { task_uuid_sha3_bytes32 });

      Function set_function = contract.GetFunction($"set_{attribute_name}");

      var wei = new HexBigInteger(0);
      var tx_hash = await set_function.SendTransactionAsync(from: sender_address, gas: new HexBigInteger(4700000), value: wei, functionInput: new object[] { task_uuid, attribute_value });

      int web3_transaction_check_delay_in_ms = _default_retry_in_ms;

      TransactionReceipt receipt = null;
      while (receipt == null)
      {
        await Task.Delay(web3_transaction_check_delay_in_ms);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
      }

      var set_event_logs = await set_event.GetFilterChanges<EventSetPolicyAttribute>(set_event_filter_by_task_uuid);

      await web3geth.Eth.Filters.UninstallFilter.SendRequestAsync(set_event_filter_by_task_uuid);

      string stringified_receipt = JsonConvert.SerializeObject(receipt);
      string stringified_event_log = JsonConvert.SerializeObject(set_event_logs.FirstOrDefault());

      return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    }
  }

  public class EventNewPolicy
  {
    [Parameter("bytes32", "task_uuid", 1, true)]
    public byte[] task_uuid { get; set; }

    [Parameter("address", "policy_contract_address", 2, false)]
    public string policy_contract_address { get; set; }
  }

  public class EventSetPolicyAttribute
  {
    [Parameter("bytes32", "task_uuid", 1, true)]
    public byte[] task_uuid { get; set; }

    [Parameter("string", "old_val", 2, false)]
    public string old_val { get; set; }

    [Parameter("string", "new_val", 3, false)]
    public string new_val { get; set; }
  }
}

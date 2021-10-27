using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin.Protocol;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Consensus;
using Stratis.Bitcoin.Features.Api;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.Notifications;
using Stratis.Bitcoin.Features.RPC;
using Stratis.Bitcoin.Features.SmartContracts;
using Stratis.Bitcoin.Features.SmartContracts.PoA;
using Stratis.Bitcoin.Features.SmartContracts.Wallet;
using Stratis.Bitcoin.Networks;
using Stratis.Bitcoin.Utilities;
using Stratis.Features.Collateral;
using Stratis.Features.Collateral.CounterChain;
using Stratis.Features.SQLiteWalletRepository;
using Stratis.Sidechains.Networks;

namespace Opdex.Cirrus
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                IFullNode fullNode = args.Any(a => a.ToLower().Contains($"-{NodeSettings.DevModeParam}"))
                    ? BuildCirrusDevNode(args)
                    : BuildCirrusLiveNode(args);
                await fullNode.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode BuildCirrusDevNode(string[] args)
        {
            string[] devModeArgs = new[] { "-bootstrap=1", "-dbtype=rocksdb", "-defaultwalletname=cirrusdev", "-defaultwalletpassword=password" }.Concat(args).ToArray();
            var network = new CirrusDev();

            var nodeSettings = new NodeSettings(network, protocolVersion: ProtocolVersion.CIRRUS_VERSION, args: devModeArgs)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .AddPoAFeature()
                .UsePoAConsensus(dbType)
                .AddPoAMiningCapability<SmartContractPoABlockDefinition>()
                .UseTransactionNotification()
                .UseBlockNotification()
                .UseApi()
                .UseMempool()
                .AddRPC()
                .AddSmartContracts(options =>
                {
                    options.UseReflectionExecutor();
                    options.UsePoAWhitelistedContracts(true);
                })
                .UseSmartContractWallet()
                .AddSQLiteWalletRepository()
                .Build();

            return node;
        }

        private static IFullNode BuildCirrusLiveNode(string[] args)
        {
            var nodeSettings = new NodeSettings(networksSelector: CirrusNetwork.NetworksSelector, protocolVersion: ProtocolVersion.CIRRUS_VERSION, args: args)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            Console.Title = $"Cirrus Full Node {nodeSettings.Network.NetworkType}";

            DbType dbType = nodeSettings.GetDbType();

            IFullNodeBuilder nodeBuilder = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .UseMempool()
                .AddSmartContracts(options =>
                {
                    options.UseReflectionExecutor();
                    options.UsePoAWhitelistedContracts();
                })
                .AddPoAFeature()
                .UsePoAConsensus(dbType)
                .CheckCollateralCommitment()
                .SetCounterChainNetwork(StraxNetwork.MainChainNetworks[nodeSettings.Network.NetworkType]())
                .UseSmartContractWallet()
                .AddSQLiteWalletRepository()
                .UseTransactionNotification()
                .UseBlockNotification()
                .UseApi()
                .AddRPC();
            // .AddSignalR(options =>
            // {
            //     DaemonConfiguration.ConfigureSignalRForCirrus(options);
            // })
            // .UseDiagnosticFeature();

            return nodeBuilder.Build();
        }
    }
}

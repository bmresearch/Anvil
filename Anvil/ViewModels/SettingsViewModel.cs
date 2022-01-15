using Anvil.Core.ViewModels;
using Anvil.Models;
using Anvil.Services;
using ReactiveUI;
using Solnet.Rpc;
using System;
using System.Collections.Generic;

namespace Anvil.ViewModels
{
    /// <summary>
    /// The model of the settings view.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        // known public rpc urls, maybe move this elsewhere later
        private const string ProjectSerumRpcUrl = "https://solana-api.projectserum.com";
        private const string GenesysGoRpcUrl = "https://ssc-dao.genesysgo.net";

        private ApplicationState _appState;
        private IRpcClientProvider _rpcClientProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appState"></param>
        /// <param name="rpcClientProvider"></param>
        public SettingsViewModel(ApplicationState appState, IRpcClientProvider rpcClientProvider)
        {
            _appState = appState;
            _rpcClientProvider = rpcClientProvider;

            if (_appState.RpcUrl != string.Empty)
            {
                RpcUrl = _appState.RpcUrl;
                switch (_appState.RpcUrl)
                {
                    case ProjectSerumRpcUrl:
                        SelectedClusterOption = "Project Serum";
                        break;
                    case GenesysGoRpcUrl:
                        SelectedClusterOption = "SSC DAO Genesys Go";
                        break;
                    default:
                        CustomRpc = true;
                        SelectedClusterOption = "Custom RPC";
                        break;
                }
            }
            else
            {
                switch (_appState.Cluster)
                {
                    case Cluster.MainNet:
                        SelectedClusterOption = "MainNet";
                        break;
                    case Cluster.TestNet:
                        SelectedClusterOption = "TestNet";
                        break;
                    case Cluster.DevNet:
                        SelectedClusterOption = "DevNet";
                        break;
                }
            }
            ClusterOptions = new List<string>
            {
                "MainNet",
                "TestNet",
                "DevNet",
                "Project Serum",
                "SSC DAO Genesys Go",
                "Custom RPC"
            };

            GetVersionInfo();
        }

        private async void GetVersionInfo()
        {
            var v = await _rpcClientProvider.Client.GetVersionAsync();

            if (v.WasRequestSuccessfullyHandled)
            {
                RpcRequestError = false;
                SolanaCoreVersion = v.Result.SolanaCore;
                SolanaFeatureSet = v.Result.FeatureSet.HasValue ? v.Result.FeatureSet.Value : 0;

                var c = await _rpcClientProvider.Client.GetClusterNodesAsync();

                if (c.WasRequestSuccessfullyHandled)
                {
                    SolanaClusterNodes = c.Result.Count;
                } else
                {
                    SolanaClusterNodes = 0;
                }
            }
            else
            {
                RpcRequestError = true;
            }
        }

        public async void ApplyChanges()
        {
            switch (SelectedClusterOption)
            {
                case "MainNet":
                    _appState.Cluster = Cluster.MainNet;
                    _appState.RpcUrl = string.Empty;
                    _rpcClientProvider.Load(_appState.Cluster);
                    break;
                case "TestNet":
                    _appState.Cluster = Cluster.TestNet;
                    _appState.RpcUrl = string.Empty;
                    _rpcClientProvider.Load(_appState.Cluster);
                    break;
                case "DevNet":
                    _appState.Cluster = Cluster.DevNet;
                    _appState.RpcUrl = string.Empty;
                    _rpcClientProvider.Load(_appState.Cluster);
                    break;
                case "Custom RPC":
                    _appState.RpcUrl = RpcUrl;
                    _rpcClientProvider.Load(_appState.RpcUrl);
                    break;
                case "Project Serum":
                    _appState.RpcUrl = ProjectSerumRpcUrl;
                    _rpcClientProvider.Load(_appState.RpcUrl);
                    break;
                case "SSC DAO Genesys Go":
                    _appState.RpcUrl = GenesysGoRpcUrl;
                    _rpcClientProvider.Load(_appState.RpcUrl);
                    break;
                default:
                    break;
            }
            GetVersionInfo();
        }

        private bool _rpcRequestError;
        public bool RpcRequestError
        {
            get => _rpcRequestError;
            set => this.RaiseAndSetIfChanged(ref _rpcRequestError, value);
        }

        private string _rpcUrl = string.Empty;
        public string RpcUrl
        {
            get => _rpcUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref _rpcUrl, value);
            }
        }

        private string _selectedClusterOption = "MainNet";
        public string SelectedClusterOption
        {
            get => _selectedClusterOption;
            set
            {
                if (value != "Custom RPC")
                {
                    CustomRpc = false;
                }
                else
                {
                    CustomRpc = true;
                }
                this.RaiseAndSetIfChanged(ref _selectedClusterOption, value);
            }
        }

        private bool _customRpc;
        public bool CustomRpc
        {
            get => _customRpc;
            set => this.RaiseAndSetIfChanged(ref _customRpc, value);
        }

        private string _solanaCoreVersion;
        public string SolanaCoreVersion
        {
            get => _solanaCoreVersion;
            set => this.RaiseAndSetIfChanged(ref _solanaCoreVersion, value); 
        }

        private ulong _solanaFeatureSet;
        public ulong SolanaFeatureSet
        {
            get => _solanaFeatureSet;
            set => this.RaiseAndSetIfChanged(ref _solanaFeatureSet, value);
        }

        private int _solanaClusterNodes;
        public int SolanaClusterNodes
        {
            get => _solanaClusterNodes;
            set => this.RaiseAndSetIfChanged(ref _solanaClusterNodes, value);
        }

        public List<string> ClusterOptions { get; }
    }
}

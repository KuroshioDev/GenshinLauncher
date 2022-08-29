using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PU_Test.Common;
using PU_Test.Common.Game;
using PU_Test.Common.Patch;
using PU_Test.Common.Proxy;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PU_Test.ViewModel
{
    partial class MainWindow : ObservableObject
    {

        public ProxyHelper.ProxyController proxyController;

        public MainWindow()
        {
            try
            {
                launcherConfig=JsonConvert.DeserializeObject<LauncherConfig>(File.ReadAllText("config.json"));

                Task.Run(async () =>
                {
                    ServerInfo = await ServerInfoGetter.GetAsync(launcherConfig.ProxyConfig.ProxyServer);

                });
            }
            catch (Exception ex)
            {
                launcherConfig = new LauncherConfig();
                launcherConfig.ProxyConfig = new ProxyConfig(true, "127.0.0.1");
                launcherConfig.ProxyConfig.ProxyPort = "25565";

                launcherConfig.GameInfo = new GameInfo(GameHelper.GameRegReader.GetGameExePath());

                SaveConfig();
            }
            try
            {

                
            }
            catch (Exception ex)
            {

            }
            ShowPatchStatue();

        }

        public void UpdateSI()
        {
            Task.Run(async () =>
            {
                ServerInfo = await ServerInfoGetter.GetAsync(launcherConfig.ProxyConfig.ProxyServer);

            });
        }

        public void ShowPatchStatue()
        {
            switch (new PatchHelper(launcherConfig.GameInfo).GetPatchStatue())
            {
                case PatchHelper.PatchType.None: PatchStatueStr = "官方"; break;
                case PatchHelper.PatchType.All: PatchStatueStr = "已打补丁-ALL"; break;
                case PatchHelper.PatchType.MetaData: PatchStatueStr = "已打补丁-Meta"; break;
                case PatchHelper.PatchType.UserAssemby: PatchStatueStr = "已打补丁-UA"; break;
            }
        }
        public void SaveConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(launcherConfig));

        }

        [ObservableProperty]
        private LauncherConfig launcherConfig;

        [ObservableProperty]
        private string startGameBtnText="开始游戏";

        [ObservableProperty]
        private string patchStatueStr = "未知";

        [ObservableProperty]
        private ServerInfo serverInfo = new ServerInfo();

        private bool IsGameRunning=false;

        [RelayCommand]
        private void StartGame()
        {
            if (new PatchHelper(launcherConfig.GameInfo).GetPatchStatue()==PatchHelper.PatchType.None)
            {
                GameHelper.StartGame(launcherConfig.GameInfo.GameExePath);
                return;
            }
            if (!IsGameRunning)
            {

                if (!CheckCfg())
                {
                    MessageBox.Show("配置项不正确！");
                    return;
                }
                IsGameRunning = true;

                proxyController = new ProxyHelper.ProxyController(host: launcherConfig.ProxyConfig.ProxyServer,port: launcherConfig.ProxyConfig.ProxyPort);
                proxyController.Start();
                StartGameBtnText = "关闭代理";
                GameHelper.StartGame(launcherConfig.GameInfo.GameExePath);
                

            }
            else
            {
                if (proxyController!=null)
                {

                    proxyController.Stop();
                }
                proxyController = null;
                StartGameBtnText = "开始游戏";
                IsGameRunning = false;
            }

        }
        private bool CheckCfg()
        {
            if (launcherConfig.GameInfo!=null)
            {
                return true;
            }
            MessageBox.Show("请设定游戏路径！");
            return false;
        }
    }
}

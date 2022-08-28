using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PU_Test.Common.Game
{
    internal class GameHelper
    {

        public static void StartGame(string filePath)
        {
            if (File.Exists(filePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = filePath,
                    UseShellExecute = true,
                };
                Process.Start(startInfo);
                //Start("cmd.exe",$"/c \"{filePath}\"");
            }
            else
            {
                Debug.Print("找不到启动目标文件！");
            }
        }


        public static class GameRegReader
        {
            /// <summary>
            /// 获取游戏目录，是静态方法
            /// </summary>
            /// <returns></returns>
            public static string GetGamePath()
            {
                try
                {
                    string startpath = "";
                    string launcherpath = GetLauncherPath();
                    #region 获取游戏启动路径，和官方配置一致
                    string cfgPath = Path.Combine(launcherpath, "config.ini");
                    if (File.Exists(launcherpath) || File.Exists(cfgPath))
                    {
                        //获取游戏本体路径
                        using (StreamReader reader = new StreamReader(cfgPath))
                        {
                            string[] abc = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                            foreach (var item in abc)
                            {
                                //从官方获取更多配置
                                if (item.IndexOf("game_install_path") != -1)
                                {
                                    startpath += item.Substring(item.IndexOf("=") + 1);
                                }
                            }
                        }
                    }
                    byte[] bytearr = Encoding.UTF8.GetBytes(startpath);
                    string path = Encoding.UTF8.GetString(bytearr);
                    return path;
                }
                catch
                {
                    return null;
                }
                #endregion
            }
            /// <summary>
            /// 启动器地址
            /// </summary>
            /// <returns></returns>
            public static string GetLauncherPath()
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine;            //打开指定注册表根
                                                                        //获取官方启动器路径
                    string launcherpath = "";
                    try
                    {
                        launcherpath = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神").GetValue("InstallPath").ToString();


                    }
                    catch (Exception)
                    {
                        launcherpath = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Genshin Impact").GetValue("InstallPath").ToString();

                    }

                    byte[] bytepath = Encoding.UTF8.GetBytes(launcherpath);     //编码转换
                    string path = Encoding.UTF8.GetString(bytepath);
                    return path;

                }
                catch
                {
                    return null;
                }
            }

            public static string GetGameExePath()
            {

                var gamepath = GameRegReader.GetGamePath();
                if (gamepath == null)
                {
                    return null;
                }
                var cnpath = gamepath + @"/YuanShen.exe";
                var ospath = gamepath + @"/GenshinImpact.exe";

                if (File.Exists(cnpath))
                {
                    return cnpath;
                }
                else if (File.Exists(ospath))
                {
                    return ospath;
                }
                else
                {
                    return null;
                }
            }


        }
    }
}

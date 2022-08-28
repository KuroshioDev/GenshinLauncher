using Newtonsoft.Json;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static PU_Test.Model.GameInfo;

namespace PU_Test.Common.Patch
{
    internal class PatchHelper
    {
        GameInfo gameInfo;
        const string METADATA_FILE_NAME = "global-metadata.dat";
        const string PKG_VERSION_FILE = "pkg_version";
        public PatchHelper(GameInfo info)
        {
            gameInfo = info;
        }
        private string GetPatchDir()
        {
            var ret = "";
            var gamedir = Path.GetDirectoryName(gameInfo.GameExePath);
            string file_path = Path.Combine(gamedir, "YuanShen_Data", "Managed", "Metadata");
            string file_path_osrel = Path.Combine(gamedir, "GenshinImpact_Data", "Managed", "Metadata");

            if (gameInfo.GetGameType() == GameType.OS)
            {
                file_path = file_path_osrel;
            }
            return file_path;
        }
        public string GetHashFromPkgVer(string filepath)
        {


            var gamedir = Path.GetDirectoryName(gameInfo.GameExePath);

            var lines =File.ReadAllLines(Path.Combine(gamedir,PKG_VERSION_FILE));

            string target = null;
            foreach (var item in lines)
            {
                if (item.Contains(filepath))
                {
                    target = item;
                    break;

                }
            }
            return JsonConvert.DeserializeObject<PkgVersionItem>(target).md5;
        }

        public string GetHashFromFile(string filepath)
        {
            try
            {
                FileStream file = new FileStream(filepath, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
            
        }

        public void BackUpFile(string FILE_NAME)
        {
            var gamedir = Path.GetDirectoryName(gameInfo.GameExePath);

            string official=string.Empty;
            string backup=string.Empty;
            if (File.Exists(FILE_NAME))
            {
                official=GetHashFromPkgVer("Managed/Metadata/global-metadata.dat");

                string currentMd5 = GetHashFromFile(Path.Combine(GetPatchDir(),METADATA_FILE_NAME));

                if (File.Exists(Path.Combine(GetPatchDir(), METADATA_FILE_NAME+".bak")))
                {
                    backup= GetHashFromFile(Path.Combine(GetPatchDir(), METADATA_FILE_NAME+".bak"));
                }

                //官方与备份相同，不用备份
                if (official == backup)
                {
                    return;
                }
                //官方与现存相同
                if (official==currentMd5)
                {
                        //备份
                        File.Copy(FILE_NAME, FILE_NAME + ".bak");


                }
                else
                {
                    throw new Exception("补丁目标不正确：不是官方文件！");

                }
            }
            else
            {
                throw new Exception("找不到pkg_version文件！");
            }
        }

        public void RestoreFile(string FILE_NAME)
        { 
            if (File.Exists( FILE_NAME + ".bak"))
            {
                var backup = GetHashFromFile(Path.Combine(GetPatchDir(), METADATA_FILE_NAME + ".bak"));
                var official=GetHashFromPkgVer("Managed/Metadata/global-metadata.dat");

                if (official!=backup)
                {
                    MessageBox.Show("备份文件不是官方文件，恢复失败！");

                    return;
                }
                
                File.Copy(FILE_NAME + ".bak", FILE_NAME, true);
                MessageBox.Show("成功恢复了备份文件！");
            }
            else
            {
                MessageBox.Show("未找到备份文件！");
            }
        }

        public void PatchMetaData()
        {
            var file_path = GetPatchDir();

            BackUpFile(Path.Combine(file_path, METADATA_FILE_NAME));

            DoPatchMetaData(Path.Combine(file_path, METADATA_FILE_NAME));

            MessageBox.Show("Patch完成!");
        }

        public void UnPatchMetaData()
        {
            var file_path = GetPatchDir();

            RestoreFile(Path.Combine(file_path, METADATA_FILE_NAME));
        }

        #region 执行Patch部分
        //const string METADATA_FILE = @"C:\Users\Swety-PC\Desktop\global-metadata.dat";
        const string dispatchKey_FILE = @"key/dispatchKey.txt";
        const string passwordKey_FILE = @"key/passwordKey.txt";
        //string dispatchKey = File.ReadAllText(METADATA_FILE);
        //string passwordKey = File.ReadAllText(passwordKey_FILE);

        [DllImport("MetadataConverterLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static int decrypt_global_metadata(ref byte data, ulong size);
        [DllImport("MetadataConverterLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static int encrypt_global_metadata(ref byte data, ulong size);

        private static byte[] ReplaceBytes(byte[] src, byte[] old, byte[] new_bytes)
        {
            byte[] dst = src;
            int index = FindBytes(src, old);
            if (index == -1)
            {
                return src;
            }
            if (index >= 0)
            {
                dst = new byte[src.Length - old.Length + new_bytes.Length];

                Buffer.BlockCopy(src, 0, dst, 0, index);

                Buffer.BlockCopy(new_bytes, 0, dst, index, new_bytes.Length);

                Buffer.BlockCopy(
                    src,
                    index + old.Length,
                    dst,
                    index + new_bytes.Length,
                    src.Length - (index + old.Length));
            }
            //i++;
            return dst;
        }

        private static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                {
                    matchIndex = 0;
                }

            }
#if DEBUG
            Debug.Print("FindCount:" + index);
#endif
            return index;
        }

        public static void DoPatchMetaData(string METADATA_FILE)
        {
            byte[] ptrData = File.ReadAllBytes(METADATA_FILE);
            //ulong size = (ulong)ptrData.Length;
            var r = decrypt_global_metadata(ref ptrData[0], (ulong)ptrData.Length);

            //Array.Resize<byte>(ref ptrData, ptrData.Length - 0x4000);

            string datastr = Encoding.Default.GetString(ptrData);

            const string pattern = @"<RSAKeyValue>((.|\n|\r)*?)</RSAKeyValue>";

            var matches = Regex.Matches(datastr, pattern);

            Match originpwdKey = matches[2];
            Match origindisKey = matches[3];

            byte[] dispatchKey = File.ReadAllBytes(dispatchKey_FILE);
            byte[] passwordKey = File.ReadAllBytes(passwordKey_FILE);


            byte[] newptrData = ReplaceBytes(ptrData, Encoding.Default.GetBytes(origindisKey.Value), dispatchKey);
            newptrData = ReplaceBytes(newptrData, Encoding.Default.GetBytes(originpwdKey.Value), passwordKey);

            //Array.Resize<byte>(ref newptrData, newptrData.Length + 0x4000);

            var r1 = encrypt_global_metadata(ref newptrData[0], (ulong)newptrData.Length);


            File.WriteAllBytes(METADATA_FILE, newptrData);
        }

        #endregion

        public enum PatchType
        {
            None,
            MetaData,
            UserAssemby,
            All
        }
        public PatchType GetPatchStatue()
        {
            PatchType result = PatchType.None;

            var dir = GetPatchDir();
            try
            {
                var official = GetHashFromPkgVer("Managed/Metadata/global-metadata.dat");

                var current  = GetHashFromFile(Path.Combine(GetPatchDir(), METADATA_FILE_NAME));

                //不相同即为已 Patch
                //var r1 = !isValidFileContent(Path.Combine(dir, METADATA_FILE_NAME), Path.Combine(dir, METADATA_FILE_NAME + ".bak"));

                if (current!=official)
                {
                    result = PatchType.MetaData;
                }

            }
            catch (Exception ex)
            {

            }

            return result;
        }

        //public static bool isValidFileContent(string filePath1, string filePath2)
        //{
        //    var file1Md5 = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(filePath1));
        //    var file2Md5 = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(filePath2));
        //    return Encoding.Default.GetString( file1Md5) == Encoding.Default.GetString(file2Md5);
        //    ////创建一个哈希算法对象
        //    //using (HashAlgorithm hash = HashAlgorithm.Create())
        //    //{
        //    //    using (FileStream file1 = new FileStream(filePath1, FileMode.Open), file2 = new FileStream(filePath2, FileMode.Open))
        //    //    {
        //    //        byte[] hashByte1 = hash.ComputeHash(file1);//哈希算法根据文本得到哈希码的字节数组
        //    //        byte[] hashByte2 = hash.ComputeHash(file2);
        //    //        string str1 = BitConverter.ToString(hashByte1);//将字节数组装换为字符串
        //    //        string str2 = BitConverter.ToString(hashByte2);
        //    //        return (str1 == str2);//比较哈希码
        //    //    }
        //    //}
        //}
    }
}

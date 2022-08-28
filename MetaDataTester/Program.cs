

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MDT
{


    static class Tester
    {

        static void Main(string[] args)
        {
            PatchMetaData();
        }


        const string METADATA_FILE = @"C:\Users\Swety-PC\Desktop\global-metadata.dat";
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

        public static void PatchMetaData()
        {
            byte[] ptrData = File.ReadAllBytes(METADATA_FILE);
            //ulong size = (ulong)ptrData.Length;
            var r = decrypt_global_metadata(ref ptrData[0], (ulong)ptrData.Length);

            //Array.Resize<byte>(ref ptrData, ptrData.Length - 0x4000);

            string datastr=Encoding.Default.GetString(ptrData);

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


            File.WriteAllBytes("decoded.dat",newptrData);
        }
    }
}


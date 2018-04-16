using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CRCApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //生成新的bin文件
                //先读取bin文件的命名方式 abcd_efgh_17H30_ffe_CRC24_180414.bin
                #region 第一步： 先获取ABCD.txt文件中的内容，以及原bin文件的地址
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var txtFile = Path.Combine(currentDir, "ABCD.txt");
                string binFileName = string.Empty;
                string companyName = string.Empty;
                string project = string.Empty;
                string ic = string.Empty;
                string time = string.Empty;

                using (StreamReader sr = new StreamReader(txtFile))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    int flag = 1;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (flag == 1)
                        {
                            //注意是中文输入法的冒号
                            binFileName = line.Split('：')[1];
                        }
                        if (flag == 2)
                        {
                            companyName = line.Split('：', ':')[1];
                        }
                        if (flag == 3)
                        {
                            project = line.Split('：', ':')[1];
                        }
                        if (flag == 4)
                        {
                            ic = line.Split('：', ':')[1];
                        }
                        if (flag == 6)
                        {
                            time = line.Split('：', ':')[1];
                        }
                        flag++;
                        //Console.WriteLine(line);
                    }
                }

                #endregion

                #region 第二步：生成crc
                //var binFile = Path.Combine(currentDir, "123.bin");
                string binFile = string.Empty;
                //判断是否为绝对路径
                if (Path.IsPathRooted(binFileName))
                {
                    binFile = binFileName;
                }
                else
                {
                    //那就是当前目录下的
                    binFile = Path.Combine(currentDir, binFileName);
                }
                BinaryReader br = new BinaryReader(new FileStream(binFile,
                    FileMode.Open));
                var length = br.BaseStream.Length;
                int crc = 0;
                for (int i = 0; i < length; i++)
                {
                    byte ds = br.ReadByte();
                    for (int j = 0; j < 8; j++)
                    {
                        int cond = (crc ^ ds) & 1;
                        crc >>= 1;
                        if (cond != 0)
                        {
                            crc ^= 0xda6000;
                        }
                        //crc = (crc >> 1) ^ poly[(crc ^ ds ) & 1];
                        ds >>= 1;
                    }
                }

                br.Close();
                #endregion


                #region 第三步：根据ABCD.txt中的内容创建新的bin文件
                byte[] crcBytes = BitConverter.GetBytes(crc);
                string newBinFileWithoutExt = $"{companyName}_{project}_{ic}_ffe_{ByteToHexStr(crcBytes)}_{time}";
                newBinFileWithoutExt = newBinFileWithoutExt.Trim('_');
                newBinFileWithoutExt = newBinFileWithoutExt.Replace("__", "_");
                var newBinFile = $"{newBinFileWithoutExt}.bin";
                File.Copy(binFile, newBinFile, true);
                #endregion

                #region 第四步：将生成crc写入到新bin文件中的指定位置
                BinaryWriter sw = new BinaryWriter(new FileStream(newBinFile, FileMode.OpenOrCreate, FileAccess.ReadWrite));
                sw.Seek(4, SeekOrigin.Begin);
                sw.Write(crcBytes, 0, crcBytes.Length);
                sw.Flush();
                sw.Close(); 
                #endregion

                Console.WriteLine($"crc:{crc.ToString()}");
                Console.WriteLine("写入完毕!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 转换成16进制数
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}

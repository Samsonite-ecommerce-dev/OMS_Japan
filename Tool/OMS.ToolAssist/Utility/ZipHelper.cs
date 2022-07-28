using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace OMS.ToolAssist.Utility
{
    public class ZipHelper
    {
        //默认密钥
        private string ZIPPASSWORD = "za1%^!*%$(OIIL80iu*(!86";

        public ZipHelper()
        {

        }

        public ZipHelper(string pwd)
        {
            ZIPPASSWORD = pwd;
        }

        #region 压缩文件
        /// <summary>
        /// ZIP:压缩单个文件
        /// </summary>
        /// <param name="FileToZip">需要压缩的文件(绝对路径)</param>
        /// <param name="ZipedPath">压缩后的文件路径(绝对路径)</param>
        /// <param name="ZipedFileName">压缩后的文件名称(文件名,默认同源文件同名)</param>
        /// <param name="CompressionLevel">压缩等级(0无 - 9最高,默认5)</param>
        public void ZipFile(string FileToZip, string ZipedPath, string ZipedFileName = "", int CompressionLevel = 5, bool IsEncrypt = true)
        {
            //如果文件没有找到,则报错
            if (!File.Exists(FileToZip))
            {
                throw new FileNotFoundException("File: " + FileToZip + " dose not exist!");
            }

            //文件名称(默认同源文件名称相同)
            string ZipFileName = string.IsNullOrEmpty(ZipedFileName) ? ZipedPath + "\\" + new FileInfo(FileToZip).Name.Substring(0, new FileInfo(FileToZip).Name.LastIndexOf('.')) + ".zip" : ZipedPath + "\\" + ZipedFileName + ".zip";

            using (FileStream ZipFile = File.Create(ZipFileName))
            {
                using (ZipOutputStream ZipStream = new ZipOutputStream(ZipFile))
                {
                    using (FileStream StreamToZip = new FileStream(FileToZip, FileMode.Open, FileAccess.Read))
                    {
                        string fileName = FileToZip.Substring(FileToZip.LastIndexOf("\\") + 1);

                        ZipEntry ZipEntry = new ZipEntry(fileName);

                        if (IsEncrypt)
                        {
                            //压缩文件加密
                            ZipStream.Password = ZIPPASSWORD;
                        }
                        ZipStream.PutNextEntry(ZipEntry);
                        //设置压缩级别
                        ZipStream.SetLevel(CompressionLevel);
                        //缓存大小
                        byte[] buffer = new byte[2048];
                        int sizeRead = 0;
                        try
                        {
                            do
                            {
                                sizeRead = StreamToZip.Read(buffer, 0, buffer.Length);
                                ZipStream.Write(buffer, 0, sizeRead);
                            }
                            while (sizeRead > 0);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            StreamToZip.Close();
                            ZipStream.Finish();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  ZIP:压缩多个文件
        /// </summary>
        /// <param name="ZipedFiles"></param>
        /// <param name="ZipedBeginDir"></param>
        /// <param name="ZipedPath"></param>
        /// <param name="ZipedFileName"></param>
        /// <param name="CompressionLevel"></param>
        /// <param name="IsEncrypt"></param>
        public void ZipFiles(List<FileInfo> ZipedFiles,string ZipedBeginDir, string ZipedPath, string ZipedFileName, int CompressionLevel = 5, bool IsEncrypt = true)
        {
            if (ZipedFiles.Count > 0)
            {
                //文件保存路径
                string ZipFileName = ZipedPath + "\\" + ZipedFileName;
                using (FileStream ZipFile = File.Create(ZipFileName))
                {
                    using (ZipOutputStream s = new ZipOutputStream(ZipFile))
                    {
                        if (IsEncrypt)
                        {
                            //压缩文件加密
                            s.Password = ZIPPASSWORD;
                        }
                        Crc32 crc = new Crc32();
                        //遍历所有的文件
                        foreach (var item in ZipedFiles)
                        {
                            using (FileStream fs = File.OpenRead(item.FullName))
                            {
                                try
                                {
                                    //打开压缩文件
                                    byte[] buffer = new byte[fs.Length];
                                    fs.Read(buffer, 0, buffer.Length);
                                    //压缩文件起始目录
                                    string fileName = "\\"+item.FullName.Substring(item.FullName.IndexOf(ZipedBeginDir));
                                    ZipEntry entry = new ZipEntry(fileName);
                                    entry.DateTime = DateTime.Now;
                                    entry.Size = fs.Length;
                                    crc.Reset();
                                    crc.Update(buffer);
                                    entry.Crc = crc.Value;
                                    s.PutNextEntry(entry);
                                    s.Write(buffer, 0, buffer.Length);

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ZIP：压缩文件夹
        /// </summary>
        /// <param name="DirectoryToZip">需要压缩的文件夹(绝对路径)</param>
        /// <param name="ZipedPath">压缩后的文件路径(绝对路径)</param>
        /// <param name="ZipedFileName">压缩后的文件名称(文件名,默认同源文件夹同名)</param>
        /// <param name="IsEncrypt">是否加密(默认加密)</param>
        public void ZipDirectory(string DirectoryToZip, string ZipedPath, string ZipedFileName = "", bool IsEncrypt = true)
        {
            //如果目录不存在,则报错
            if (!Directory.Exists(DirectoryToZip))
            {
                throw new FileNotFoundException("Directory: " + DirectoryToZip + " dose not exist!");
            }

            //文件名称(默认同源文件名称相同)
            string ZipFileName = string.IsNullOrEmpty(ZipedFileName) ? ZipedPath + "\\" + new DirectoryInfo(DirectoryToZip).Name + ".zip" : ZipedPath + "\\" + ZipedFileName + ".zip";

            using (FileStream ZipFile = File.Create(ZipFileName))
            {
                using (ZipOutputStream s = new ZipOutputStream(ZipFile))
                {
                    if (IsEncrypt)
                    {
                        //压缩文件加密
                        s.Password = ZIPPASSWORD;
                    }
                    ZipErgodic(DirectoryToZip, s, "");
                }
            }
        }

        /// <summary>
        /// 递归遍历目录
        /// </summary>
        private void ZipErgodic(string strDirectory, ZipOutputStream s, string parentPath)
        {
            Crc32 crc = new Crc32();
            if (strDirectory[strDirectory.Length - 1] != Path.DirectorySeparatorChar)
            {
                strDirectory += Path.DirectorySeparatorChar;
            }
            string[] filenames = Directory.GetFileSystemEntries(strDirectory);
            //遍历所有的文件和目录
            foreach (string file in filenames)
            {
                //先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                if (Directory.Exists(file))
                {
                    string pPath = parentPath;
                    pPath += file.Substring(file.LastIndexOf("\\") + 1);
                    pPath += "\\";
                    ZipErgodic(file, s, pPath);
                }
                //否则直接压缩文件
                else
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        try
                        {
                            //打开压缩文件
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Length);
                            string fileName = parentPath + file.Substring(file.LastIndexOf("\\") + 1);
                            ZipEntry entry = new ZipEntry(fileName);
                            entry.DateTime = DateTime.Now;
                            entry.Size = fs.Length;
                            crc.Reset();
                            crc.Update(buffer);
                            entry.Crc = crc.Value;
                            s.PutNextEntry(entry);
                            s.Write(buffer, 0, buffer.Length);

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            fs.Close();
                        }
                    }
                }
            }
        }
        #endregion

        #region 解压文件
        /// <summary>
        /// ZIP:解压一个zip文件
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件(绝对路径)</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="IsDecrypt">是否解密</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public void UnZip(string ZipFile, string TargetDirectory, bool IsDecrypt = true, bool OverWrite = true)
        {
            //如果解压到的目录不存在,则报错
            if (!Directory.Exists(TargetDirectory))
            {
                throw new FileNotFoundException("File: " + TargetDirectory + " dose not exist!");
            }
            //目录结尾
            if (!TargetDirectory.EndsWith("\\")) { TargetDirectory = TargetDirectory + "\\"; }

            using (ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(ZipFile)))
            {
                if (IsDecrypt)
                {
                    zipfiles.Password = ZIPPASSWORD;
                }
                ZipEntry theEntry;

                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "")
                        directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = Path.GetFileName(pathToZip);

                    Directory.CreateDirectory(TargetDirectory + directoryName);

                    if (fileName != "")
                    {
                        if ((File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) || (!File.Exists(TargetDirectory + directoryName + fileName)))
                        {
                            using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zipfiles.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                zipfiles.Close();
            }
        }
        #endregion
    }
}

using FTPForSTEAM.Functions;
using FTPForSTEAM.Models;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using XmlReader = FTPForSTEAM.Functions.XmlReader;

namespace FTPForSTEAM
{
    internal class Program
    {

        static void Main()
        {
            AppSettings settings = Initialization(); //arrange app's settings based on App.config and putting it in AppSettings Model
            var currentDate = DateTime.Now;
            var files = ReadText(settings);
            try
            {
                foreach(List<string> file in files ?? Enumerable.Empty<List<string>>())
                {
                    var item = file[1];
                    var filePath = file[0];
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(item);
                    XmlElement root = doc.DocumentElement;
                    string uInterchange = root.Name;
                    string uShipment = root.LastChild.FirstChild.Name;
                    
                    processingFiles(item, filePath, currentDate, settings);
                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"operation failed because of {e}");
            }
        }
        public static void processingFiles(string item, string filePath, DateTime currentDate, AppSettings settings)
        {
            try
            {
                UniversalInterchange data = XmlReader.LoadUniversalInterchangeDataFromXMLString(item);
                XmlElement xmlElement = data.Body.Any.First(); 
                UniversalShipmentData universalShipment = XmlReader.LoadUniversalShipmentDataFromXMLString(xmlElement.OuterXml);
                var check = universalShipment.Shipment.OrganizationAddressCollection;
                foreach(var shipment in check)
                {
                    if(shipment.OrganizationCode == "STELOGCHA")
                    {
                        throw new Exception("Org.Code STELOGCHA is not what we are looking for");
                    }
                }
                var dataObject = XmlReader.mappingXml(universalShipment);
                var stringwriter = new StringWriter();
                var serialized = new XmlSerializer(typeof(UniversalShipmentData));
                serialized.Serialize(stringwriter, dataObject);
                var currentFile = filePath.Replace(settings.FTPPath.INPUT_FOLDER + "\\", "").Replace(".xml", "") + "-";
                var fileName = currentFile + "converted" + ".xml";

                File.WriteAllText(settings.FTPPath.INPUT_FOLDER + "\\" + fileName, stringwriter.ToString());
                string ConvertedFile = settings.FTPPath.INPUT_FOLDER + "\\" + fileName;
                Console.WriteLine($"file is converted as {ConvertedFile}");

                FtpService.upload(currentDate, ConvertedFile, settings);
                Console.WriteLine($"upload file ok");
                MoveSuccessFile(filePath, item, settings, currentDate);

            }
            catch (Exception e)
            {
                Console.WriteLine($"file is failed inputted because of {e}");
                MoveErrorFile(filePath, item, settings, currentDate);

            }
        }
        
        public static void MoveErrorFile(string filePath, string xmlString, AppSettings appSettings, DateTime currentDate)
        {
            var currentFile = filePath.Replace(appSettings.FTPPath.INPUT_FOLDER + "\\", "").Replace(".xml", "") + "-";
            var fileName = currentFile + currentDate.ToString("yyyyMMddHHMMss.fff") + ".xml";
            File.Move(filePath, appSettings.FTPPath.INPUT_ERROR_FOLDER + "\\" + fileName);
            fileName = currentFile + currentDate.ToString("yyyyMMddHHMMss.fff") + "-Message" + ".txt";
            var pathError = appSettings.FTPPath == null ? "" : appSettings.FTPPath.INPUT_ERROR_FOLDER;
            File.WriteAllText(pathError + "\\" + fileName, xmlString);

        }
        public static void MoveSuccessFile(string filePath, string xmlString, AppSettings appSettings, DateTime currentDate)
        {
            var currentFile = filePath.Replace(appSettings.FTPPath.INPUT_FOLDER + "\\", "").Replace(".xml", "") + "-";
            string fileName = currentFile + currentDate.ToString("yyyyMMddHHMMss.fff") + ".txt";
            var path = appSettings.FTPPath == null ? "" : appSettings.FTPPath.INPUT_ARCHIVE_FOLDER;
            File.WriteAllText($"{path}\\{fileName}", xmlString);
            currentFile = filePath.Replace(appSettings.FTPPath.INPUT_FOLDER + "\\", "").Replace(".xml", "") + "-";
            fileName = currentFile + currentDate.ToString("yyyyMMddHHMMss.fff") + ".xml";
            File.Move(filePath, appSettings.FTPPath.INPUT_ARCHIVE_FOLDER + "\\" + fileName);

        }
        public static List<List<string>> ReadText(AppSettings appSettings)
        {
            if (appSettings.FTPPath != null)
            {
                var listFile = Directory.GetFiles(appSettings.FTPPath.INPUT_FOLDER, "*.xml*", SearchOption.TopDirectoryOnly);

                if (listFile.Length > 0)
                {
                    List<List<string>> fileDataList = new List<List<string>>();

                    foreach (var file in listFile.ToList())
                    {
                        List<string> dataList = new List<string>
                        {
                            file,
                            File.ReadAllText(file)
                        };
                        fileDataList.Add(dataList);
                    }
                    return fileDataList;
                }

            }
            return null;
        }
        public static AppSettings Initialization()
        {
            Console.WriteLine("Getting App Settings Data");
            if (ConfigurationManager.AppSettings != null)
            {
                var ftp = ConfigurationManager.AppSettings["FTP_FOLDER"];
                var input = ConfigurationManager.AppSettings["INPUT_FOLDER"];
                var inputArchive = ConfigurationManager.AppSettings["INPUT_ARCHIVE_FOLDER"];
                var inputError = ConfigurationManager.AppSettings["INPUT_ERROR_FOLDER"];
                var inputLog = ConfigurationManager.AppSettings["INPUT_LOG_FOLDER"];
                bool logEnable = ConfigurationManager.AppSettings["LOG_ENABLED"] == "True";
                if (ftp != null)
                {
                    var inputPath = ftp + "\\" + input;
                    if (input != null &&
                        input.Contains(':'))
                    {
                        inputPath = input;
                    }

                    var inputArchivePath = inputPath + "\\" + inputArchive;
                    if (inputArchive != null
                        && inputArchive.Contains(':'))
                    {
                        inputArchivePath = inputArchive;
                    }

                    var inputErrorPath = inputPath + "\\" + inputError;
                    if (inputError != null
                        && inputError.Contains(':'))
                    {
                        inputErrorPath = inputError;
                    }

                    var inputLogPath = inputPath + "\\" + inputLog;
                    if (inputLog != null
                        && inputLog.Contains(':'))
                    {
                        inputLogPath = inputLog;
                    }


                    var ftpPath = new FTPPath
                    {
                        FTP_FOLDER = ftp,
                        INPUT_ARCHIVE_FOLDER = inputArchivePath,
                        INPUT_ERROR_FOLDER = inputErrorPath,
                        INPUT_FOLDER = inputPath,
                        INPUT_LOG_FOLDER = inputLogPath,
                    };


                    var host = ConfigurationManager.AppSettings["FTP_HOST"];
                    var ftpUser = ConfigurationManager.AppSettings["FTP_USER"];
                    var ftpPassword = ConfigurationManager.AppSettings["FTP_PASSWORD"];
                    var folderDestination = ConfigurationManager.AppSettings["FTP_FOLDER_DESTINATION"];
                    bool transferMode = ConfigurationManager.AppSettings["IS_FTP_ASCII"] == "True";

                    var ftpServer = new FTPServer
                    {
                        FOLDER_DESTINATION = folderDestination,
                        USER = ftpUser,
                        PASSWORD = ftpPassword,
                        HOST = host,
                        IS_FTP_ASCII = transferMode,
                    };

                    var config = new AppSettings
                    {
                        FTPPath = ftpPath,
                        FTPServer = ftpServer,
                    };

                    if (logEnable)
                    {
                        return config;
                    }
                }
            }
            return null;
        }
    }
}

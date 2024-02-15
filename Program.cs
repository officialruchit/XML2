using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DeviceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlFilePath;

            if (args.Length == 1)
            {
                xmlFilePath = args[0];
            }
            else
            {
                Console.WriteLine("Error: Invalid input. Program usage is as below.");
                Console.WriteLine("[DeviceUtil.exe] [XML file path]");
                Console.WriteLine("DeviceUtil.exe : Name of the executable file");
                Console.WriteLine("If anyone changes the name of the EXE, then the executable file name in usage should change accordingly.");
                Console.WriteLine("Terminate program.");
                return;
            }

            // XML file validation
            if (!File.Exists(xmlFilePath))
            {
                Console.WriteLine("Error: File not exist. Please provide a valid file path.");
                Console.WriteLine("Terminate program.");
                return;
            }

            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                Console.WriteLine("Error: Given file is not an XML file. The file extension is wrong.");
                Console.WriteLine("Terminate program.");
                return;
            }

            // Validate XML format and parse devices
            Dictionary<string, Dictionary<string, string>> devices;
            try
            {
                devices = ParseXml(xmlFilePath);
            }
            catch (XmlException ex)
            {
                Console.WriteLine("Error: Invalid XML format. " + ex.Message);
                Console.WriteLine("Terminate program.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: An unexpected error occurred while parsing XML. " + ex.Message);
                Console.WriteLine("Terminate program.");
                return;
            }

            while (true)
            {
                Console.WriteLine("\nPlease select an option:");
                Console.WriteLine("[1] Show all devices");
                Console.WriteLine("[2] Search devices by serial number");
                Console.WriteLine("[3] Exit");

                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        ShowDevices(devices);
                        break;
                    case "2":
                        Console.Write("Enter serial number of the device: ");
                        string serialNumber = Console.ReadLine().Trim();
                        SearchDevice(devices, serialNumber);
                        break;
                    case "3":
                        Console.WriteLine("Program terminated.");
                        return;
                    default:
                        Console.WriteLine("Error: Invalid input. Please choose from the above options.");
                        break;
                }
            }
        }

        static Dictionary<string, Dictionary<string, string>> ParseXml(string filePath)
        {
            Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            int deviceIndex = 1;
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                Dictionary<string, string> deviceInfo = new Dictionary<string, string>();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "CommSetting")
                    {
                        foreach (XmlNode settingNode in childNode.ChildNodes)
                        {
                            deviceInfo[childNode.Name + "_" + settingNode.Name] = settingNode.InnerText;
                        }
                    }
                    else
                    {
                        deviceInfo[childNode.Name] = childNode.InnerText;
                    }
                }

                // Validate device information
                if (!IsValidDevice(deviceInfo))
                {
                    Console.WriteLine("Error: Invalid device information. Please refer below details.");
                    Console.WriteLine($"Device index: {deviceIndex}");

                    foreach (var entry in deviceInfo)
                    {
                        Console.WriteLine($"{entry.Key}: {entry.Value}");
                    }
                    Console.WriteLine();

                    // Only show error for the first invalid device
                    break;
                }

                devices[node.Attributes["SrNo"].Value] = deviceInfo;
                deviceIndex++;
            }

            return devices;
        }

        static bool IsValidDevice(Dictionary<string, string> deviceInfo)
        {
            // Check if IP Address is in the correct format
            if (!IsValidIPAddress(deviceInfo["Address"]))
                return false;

            // Additional validation checks can be added here

            return true;
        }

        static bool IsValidIPAddress(string ipAddress)
        {
            // Basic validation for IPv4 address format
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }



        static void ShowDevices(Dictionary<string, Dictionary<string, string>> devices)
        {
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", "No", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            int i = 1;
            foreach (var device in devices)
            {
                Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", i++, device.Key, device.Value["Address"], device.Value["DevName"], device.Value["ModelName"], device.Value["Type"], device.Value["CommSetting_PortNo"], device.Value["CommSetting_UseSSL"], device.Value["CommSetting_Password"]);
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
        }


        static void SearchDevice(Dictionary<string, Dictionary<string, string>> devices, string serialNumber)
        {
            if (devices.ContainsKey(serialNumber))
            {
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", serialNumber, devices[serialNumber]["Address"], devices[serialNumber]["DevName"], devices[serialNumber]["ModelName"], devices[serialNumber]["Type"], devices[serialNumber]["CommSetting_PortNo"], devices[serialNumber]["CommSetting_UseSSL"], devices[serialNumber]["CommSetting_Password"]);
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Device not found.");
            }
        }
    }
}

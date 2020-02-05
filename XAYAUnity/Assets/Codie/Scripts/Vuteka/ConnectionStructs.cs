using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vuteka
{
    public struct ConnectionInfo
    {
        public string ip;
        public string port;
        public string endpointPath;
        public string username;
        public string userpassword;
        
        public ConnectionInfo(string _ip, string _port, string path, string user, string password)
        {
            ip = _ip;
            port = _port;
            username = user;
            userpassword = password;
            endpointPath = path;
        }

        public string GetHTTPCompatibleIP()
        {
            return string.Format("http://{0}", ip);
        }

        public string GetCURL()
        {
            return string.Format("http://{0}:{1}@{2}:{3}", username, userpassword, ip, port);
        }

        public string GetHTTPCompatibleURL(bool withPath)
        {
            if(withPath)
                return string.Format("http://{0}:{1}{2}", ip, port, endpointPath);
            else
                return string.Format("http://{0}:{1}", ip, port);
        }
    }

    public struct StateProcessorPathInfo
    {
        public string basePath { get; set; }
        private string libraryPath;
        private string databasePath;
        private string errorLogPath;

        public StateProcessorPathInfo(string _basePath, string libPath, string dbPath, string logPath)
        {
            basePath = _basePath;
            libraryPath = libPath;
            databasePath = dbPath;
            errorLogPath = logPath;
        }

        public string library { get { return basePath + libraryPath; } }
        public string database { get { return basePath + databasePath; } }
        public string logs { get { return basePath + errorLogPath; } }
    }
}
// Copyright 2007-2011 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Model
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Internal;
    using log4net;
    using Stact;
    using Magnum.Extensions;


    public class ProcessShelfReference :
        ShelfReference
    {
        static readonly ILog _logger = LogManager.GetLogger("Topshelf.Model.ProcessShelfReference");
        readonly UntypedChannel _controllerChannel;
        readonly ProcessStartInfo _processSettings;

        readonly string _serviceName;
        readonly string _serviceDirectory;
        readonly IsolationLevel _isolationLevel;
        OutboundChannel _channel;
        bool _disposed;
        HostChannel _hostChannel;
        Process _process;
        int _pid;
        readonly string _commandLineFormat = "shelf -uri:{0} -pipe:{1} -bootstrapper:{2}";
        public ProcessShelfReference(string serviceName, string serviceDirectory, IsolationLevel isolationLevel, UntypedChannel controllerChannel)
        {
            _serviceName = serviceName;
            _serviceDirectory = serviceDirectory;
            _isolationLevel = isolationLevel;
            _controllerChannel = controllerChannel;

            //need to path the host.exe correctly.
            var pathToExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "topshelf.host.exe");
            _processSettings = new ProcessStartInfo(pathToExe)
                {
                    WorkingDirectory = _serviceDirectory
                };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Send<T>(T message)
        {
            if(_channel != null)
            {
                _logger.WarnFormat("Unable to send service message due to null channel, service = {0}, message type = {1}",
                    _serviceName, typeof(T).ToShortTypeName());
                return;
            }

            _channel.Send(message);
        }

        public void LoadAssembly(AssemblyName assemblyName)
        {
            //TODO: is this needed?
        }

        public void Create()
        {
            CreateShelfInstance(null);
        }
        public void Create([NotNull] string bootstrapperType)
        {
            CreateShelfInstance(bootstrapperType);
        }

        void CreateShelfInstance(string bootstrapperType)
        {
            _logger.DebugFormat("[{0}] Creating Host Channel", _serviceName);

            _hostChannel = HostChannelFactory.CreateShelfControllerHost(_controllerChannel, _serviceName);

            _logger.InfoFormat("[{0}] Created Host Channel: {1}({2})", _serviceName, _hostChannel.Address, _hostChannel.PipeName);

            _logger.DebugFormat("Creating Shelf Instance: {0}", _serviceName);

            Type shelfType = typeof(Shelf);

            _processSettings.Arguments = _commandLineFormat.FormatWith(_hostChannel.Address, _hostChannel.PipeName, bootstrapperType);

            _process = Process.Start(_processSettings);
            _pid = _process.Id;
        }

        public void CreateShelfChannel(Uri address, string pipeName)
        {
            _logger.DebugFormat("[{0}] Creating shelf proxy: {1} ({2})", _serviceName, address, pipeName);

            _channel = new OutboundChannel(address, pipeName);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if(disposing)
            {
                if(_hostChannel != null)
                {
                    _hostChannel.Dispose();
                    _hostChannel = null;
                }

                if(_channel != null)
                {
                    _channel.Dispose();
                    _channel = null;
                }

                if(_process != null)
                {
                    _process.Dispose();
                    _process = null;
                    _pid = -1; //is this ok?
                }
            }
        }

        public void Unload()
        {
            //TODO: should call dispose?
            if(_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
        }
    }
}
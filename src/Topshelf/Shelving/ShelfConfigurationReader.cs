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
namespace Topshelf.Shelving
{
    using System;
    using System.Xml.Linq;
    using Magnum.Extensions;
    using Model;


    public class ShelfConfigurationReader
    {
        public ConfigurationOptions LoadShelfOptions(string basePath, string serviceName)
        {
            var fileName = "{0}.config".FormatWith(serviceName);
            var serviceDirectory = System.IO.Path.Combine(basePath, serviceName);
            var pathToConfigFile = System.IO.Path.Combine(serviceDirectory, fileName);

            var xml = XElement.Load(pathToConfigFile);
            var sc = xml.Element("ShelfConfiguration");
            var bootstrapper = sc.Attribute("Bootstrapper").Value;
            var isolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel), sc.Attribute("IsolationLevel").Value, true);

            return new ConfigurationOptions
                {
                    Bootstrapper = bootstrapper,
                    IsolationLevel = isolationLevel,
                    ServiceDirectory  = serviceDirectory
                };
        }
    }
}
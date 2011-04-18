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
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using Exceptions;
	using Magnum.Extensions;
	using Model;


	public class ShelfConfigurationReader
	{
		public ConfigurationOptions LoadShelfOptions(string basePath, string serviceName)
		{
			try
			{
				string configurationFileName = "{0}.config".FormatWith(serviceName);

				string serviceDirectory = Path.Combine(basePath, serviceName);
				string pathToConfigFile = Path.Combine(serviceDirectory, configurationFileName);

				XElement xml = XElement.Load(pathToConfigFile);

				XElement shelfConfiguration = xml.Descendants("ShelfConfiguration")
					.Single();

				string bootstrapper = shelfConfiguration.Attributes("Bootstrapper")
					.Select(x => x.Value)
					.Single();

				IsolationLevel isolationLevel = shelfConfiguration.Attributes("IsolationLevel")
					.Select(x => (IsolationLevel)Enum.Parse(typeof(IsolationLevel), x.Value, true))
					.DefaultIfEmpty(IsolationLevel.AppDomain)
					.Single();

				return new ConfigurationOptions
					{
						Bootstrapper = bootstrapper,
						IsolationLevel = isolationLevel,
						ServiceDirectory = serviceDirectory
					};
			}
			catch (Exception ex)
			{
				throw new ConfigurationException("The shelf configuration could not be read for service: " + serviceName, ex);
			}
		}
	}
}
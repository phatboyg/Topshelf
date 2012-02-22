﻿// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Builders
{
	using System;
	using System.Diagnostics;
	using Common.Logging;
	using Hosts;


	public class HelpBuilder :
		HostBuilder
	{
		static readonly ILog _logger = LogManager.GetLogger(typeof(HelpBuilder));
		readonly ServiceDescription _description;

		public HelpBuilder(ServiceDescription description)
		{
			_description = description;
		}

		public ServiceDescription Description
		{
			get { return _description; }
		}

		public Host Build()
		{
			return new HelpHost(_description);
		}

		public void Match<T>(Action<T> callback)
			where T : class, HostBuilder
		{
			if (callback != null)
			{
				if (typeof(T).IsAssignableFrom(GetType()))
					callback(this as T);
			}
			else
			{
				_logger.Warn("Match{{T}} called with callback of null. If you are running the host "
					+ "in debug mode, the next log message will print a stack trace.");
#if DEBUG
				_logger.Warn(new StackTrace());
#endif
			}
		}
	}
}
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
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Internal;
    using log4net;


    public class ProcessShelfBuilder :
        HostBuilder
    {
        static readonly ILog _log = LogManager.GetLogger("Topshelf.Builders.ProcessShelfBuilder");
        readonly ServiceDescription _description;
        Uri _uri;
        string _pipe;
        string _bootstrapper;

        public ProcessShelfBuilder([NotNull]ServiceDescription description, Uri uri, string pipe, string bootstrapper)
        {
            _description = description;
            _uri = uri;
            _pipe = pipe;
            _bootstrapper = bootstrapper;
        }

        public ServiceDescription Description
        {
            get { return _description; }
        }

        public Host Build()
        {
            return new ProcessShelfHost(_uri, _pipe, _bootstrapper);
        }

        public void Match<T>([NotNull] Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentException("callback");

            if (typeof(T).IsAssignableFrom(GetType()))
                callback(this as T);
        }
    }
}
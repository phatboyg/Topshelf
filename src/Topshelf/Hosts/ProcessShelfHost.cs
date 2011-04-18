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
namespace Topshelf.Hosts
{
    using System;
    using Model;


    public class ProcessShelfHost : 
        Host
    {
        readonly Uri _uri;
        readonly string _pipe;
        readonly string _bootstrapper;

        public ProcessShelfHost(Uri uri, string pipe, string bootstrapper)
        {
            _uri = uri;
            _pipe = pipe;
            _bootstrapper = bootstrapper;
        }

        public void Run()
        {
            var type = Type.GetType(_bootstrapper);
        	using (var shelf = new Shelf(type, _uri, _pipe))
        	{
				// TODO this probably needs to use an AppDomainShelfReference instead of creating
				// the shelf directly so that it will run the same as if it were in the host
				// but don't think we need a full coordinator as that would be pretty heavy and is 
				// already in the shelf.cs
				// likely use some type of event notification when the domain unloads to exit the
				// process, otherwise, waitonsingleevent until the child app domain exits then
				// exit the process


        	}
        }
    }
}
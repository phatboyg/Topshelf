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
namespace Topshelf.Specs.XmlPoke
{
	using Exceptions;
	using Magnum.TestFramework;
	using Model;
    using NUnit.Framework;
    using Shelving;


	[Scenario]
	public class When_reading_a_shelf_configuration
	{
		ConfigurationOptions _options;

		[When]
		public void Reading_a_shelf_configuration()
		{
			var reader = new ShelfConfigurationReader();
			_options = reader.LoadShelfOptions(".", "xmlpoke");
		}

		[Then]
		public void Should_load_the_bootstrapper_type()
		{
			_options.Bootstrapper.ShouldEqual("Topshelf.Specs.TestAppDomainBootstrapper, TopShelf.Specs");
		}

		[Then]
		public void Should_load_the_isolation_level()
		{
			_options.IsolationLevel.ShouldEqual(IsolationLevel.Process);
		}
	}

    [TestFixture]
    public class Reading_an_invalid_shelf_configuration
    {
    	[Test]
    	public void Should_throw_an_exception()
    	{
			var reader = new ShelfConfigurationReader();
			Assert.Throws<ConfigurationException>(() => reader.LoadShelfOptions(".", "broken"));
		}
    }
}
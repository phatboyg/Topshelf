namespace Topshelf.Specs.XmlPoke
{
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class XmlPoking_Specs
    {
        [Test]
        public void Bob()
        {
            var poker = new XmlPoker();
            var opts = poker.LoadOptionsFromFile(".\\xmlpoke\\play.xml");


            opts.Bootstrapper.ShouldEqual("Topshelf.Specs.TestAppDomainBootstrapper, TopShelf.Specs");
            opts.IsolationLevel.ShouldEqual("AppDomain");
        }



    }

    public class XmlPoker
    {
        public ConfigurationOptions LoadOptionsFromFile(string fileName)
        {

            var xml = XElement.Load(fileName);
            var sc = xml.Element("ShelfConfiguration");
            var bootstrapper = sc.Attribute("Bootstrapper");
            var isolationLevel = sc.Attribute("IsolationLevel");

            return new ConfigurationOptions()
                {
                    Bootstrapper = bootstrapper.Value,
                    IsolationLevel = isolationLevel.Value
                };
        }

    }

    public class ConfigurationOptions
    {
        public string Bootstrapper { get; set; }
        public string IsolationLevel { get; set; }
    }
}
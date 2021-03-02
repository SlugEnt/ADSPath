using NUnit.Framework;
using SlugEnt;

namespace Test_ADSPath
{
	public class Test_ADSPath
	{
		[SetUp]
		public void Setup()
		{
		}

		[TestCase("CN=slug,OU=animals,DC=some,DC=local", "DC=some,DC=local")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local", "dc=some,dc=local")]
		[TestCase("LDAP:", "")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "dc=some,dc=local")]
		[TestCase("LDAP://ou=people,dc=some,dc=local", "dc=some,dc=local")]
		[TestCase("LDAP:", "")]
		[TestCase("LDAP://server.some.local", "")]
		[TestCase("LDAP://server.some.local:65000", "")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local", "dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local", "dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local", "dc=some,dc=local")]
		[Test]
		public void DistinguishedName_Suffix_Success (string original, string expSuffix) {
			//string dn = "CN=scott,OU=people,dc=some,dc=local";
			//string expSuffix = "dc=some,dc=local";

			ADSPath adsPath = new ADSPath(original);

			Assert.AreEqual(expSuffix,adsPath.Suffix,"A10: ");
		}


		[TestCase("CN=slug,OU=animals,DC=some,DC=local", "")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local", "")]
		[TestCase("LDAP:","LDAP:")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "LDAP:")]
		[TestCase("LDAP://ou=people,dc=some,dc=local", "LDAP:")]
		[TestCase("LDAP://server.some.local", "LDAP://server.some.local")]
		[TestCase("LDAP://server.some.local:65000", "LDAP://server.some.local:65000")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local", "LDAP://server.some.local:65000")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local", "LDAP://server.some.local:65000")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local", "LDAP://server.some.local:65000")]
		[Test]
		public void ADSPath_Prefix_Success (string path, string expPrefix) {
			ADSPath adsPath = new ADSPath(path);
			Assert.AreEqual(expPrefix,adsPath.Prefix,"A10");
		}



		[TestCase("CN=slug,OU=animals,DC=some,DC=local", "CN=slug,OU=animals")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local", "cn=scott,ou=people")]
		[TestCase("LDAP:", "")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "cn=mary,ou=people")]
		[TestCase("LDAP://ou=people,dc=some,dc=local", "ou=people")]
		[TestCase("LDAP://server.some.local", "")]
		[TestCase("LDAP://server.some.local:65000", "")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local", "OU=people")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local", "cn=mary smith,OU=people")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local", "o=Petes Pizza,OU=people")]
		[Test]
		public void ADSPath_DistinguishedName_Success(string path, string expDN)
		{
			ADSPath adsPath = new ADSPath(path);
			Assert.AreEqual(expDN, adsPath.DN, "A10");
		}



		[TestCase("CN=slug,OU=animals,DC=some,DC=local")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local")]
		[TestCase("LDAP:")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local")]
		[TestCase("LDAP://ou=people,dc=some,dc=local")]
		[TestCase("LDAP://server.some.local")]
		[TestCase("LDAP://server.some.local:65000")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local")]
		[Test]
		public void ADSPath_BuildPath_Success(string path)
		{
			ADSPath adsPath = new ADSPath(path);

			string rebuild = adsPath.BuildFullPath();
			Assert.AreEqual(path, rebuild, "A10");
		}





		[TestCase("CN=slug,OU=animals,DC=some,DC=local", "OU=animals")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local", "ou=people")]
		[TestCase("LDAP:", "")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "ou=people")]
		[TestCase("LDAP://ou=people,dc=some,dc=local", "")]
		[TestCase("LDAP://server.some.local", "")]
		[TestCase("LDAP://server.some.local:65000", "")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local", "")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local", "OU=people")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local", "OU=people")]
		[TestCase("cn=mary,ou=people,ou=us,ou=California,ou=San Diego,dc=some,dc=local", "ou=people,ou=us,ou=California,ou=San Diego")]
		[Test]
		public void GetParentDN (string path, string expected) {
			ADSPath adsPath = new ADSPath(path);
			string parent = adsPath.GetParentDN();

			Assert.AreEqual(path,adsPath.Path,"A10: ");
			Assert.AreEqual(expected,parent,"A20: ");
		}


		[TestCase("CN=slug,OU=animals,DC=some,DC=local", "OU=animals,DC=some,DC=local")]
		[TestCase("cn=scott,ou=people,dc=some,dc=local", "ou=people,dc=some,dc=local")]
		[TestCase("LDAP:", "LDAP:")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "LDAP://ou=people,dc=some,dc=local")]
		[TestCase("LDAP://ou=people,dc=some,dc=local", "LDAP://dc=some,dc=local")]
		[TestCase("LDAP://server.some.local", "LDAP://server.some.local")]
		[TestCase("LDAP://server.some.local:65000", "LDAP://server.some.local:65000")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local", "LDAP://server.some.local:65000/dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/cn=mary smith,OU=people,dc=some,dc=local", "LDAP://server.some.local:65000/OU=people,dc=some,dc=local")]
		[TestCase("LDAP://server.some.local:65000/o=Petes Pizza,OU=people,dc=some,dc=local", "LDAP://server.some.local:65000/OU=people,dc=some,dc=local")]
		[TestCase("cn=mary,ou=people,ou=us,ou=California,ou=San Diego,dc=some,dc=local", "ou=people,ou=us,ou=California,ou=San Diego,dc=some,dc=local")]
		[Test]
		public void GetParentADSPath(string path, string expected)
		{
			ADSPath adsPath = new ADSPath(path);
			ADSPath parent = adsPath.GetParent();
			

			//Assert.AreEqual(path, adsPath.Path, "A10: ");
			Assert.AreEqual(expected, parent.Path, "A20: ");
		}


		[TestCase("cn=scott,ou=people,dc=some,dc=local", "ou=hired","ou=hired,ou=people,dc=some,dc=local")]
// TODO - what to do...Not sure we can get a valid child		[TestCase("LDAP:", "LDAP:")]
		[TestCase("LDAP://cn=mary,ou=people,dc=some,dc=local", "OU=giveraise,OU=high performer","LDAP://OU=giveraise,OU=high performer,ou=people,dc=some,dc=local")]
		[TestCase("LDAP://dc=some,dc=local", "ou=greatchild","LDAP://ou=greatchild,dc=some,dc=local")]
// TODO what to do... Not sure this is valide		[TestCase("LDAP://server.some.local", "ou=firstone","LDAP://server.some.local/ou=firstone")]
		[TestCase("LDAP://server.some.local:65000", "ou=first","LDAP://server.some.local:65000/ou=first")]
		[TestCase("LDAP://server.some.local:65000/OU=people,dc=some,dc=local","ou=second", "LDAP://server.some.local:65000/ou=second,OU=people,dc=some,dc=local")]
		[Test]
		public void GetChildADSPath (string path, string child, string expected) {
			ADSPath adsPath = new ADSPath(path);
			ADSPath childAdsPath = adsPath.NewChildADSPath(child);
			Assert.AreEqual(expected,childAdsPath.Path,"A10: ");
		}



		[Test]
		public void ToString () {
			string path = "LDAP://server.some.local:65000/OU=people/OU=US,DC=some,DC=local";
			ADSPath adsPath = new ADSPath(path);

			Assert.AreEqual(path, adsPath.ToString(),"A10:");
		}



		[TestCase("cn=scott,ou=people,dc=some,dc=local", "people")]
		[TestCase("ou=people,ou=us,ou=North America,dc=some,dc=local", "people")]
		//[TestCase()]
		[Test]
		public void ShortName (string path, string expected) {
			ADSPath adsPath = new ADSPath(path);
			Assert.AreEqual(expected,adsPath.ShortName(),"A10: ");
		}
	}

}

﻿using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Test_ADSPath")]
namespace SlugEnt
{

	/// <summary>
	/// Represents an Active Directory LDAP ADSPath object.  Provides the means to 
	/// </summary>
	public class ADSPath {
		private string _path;
		// The starting and ending index positions of the DistinguishedName part of the path.
		private int dnStart;
		private int dnEnd;


		public ADSPath (string adsPath) {
			Path = adsPath;

			// We need to operate on an all lower case version of the string, but will always return the original parts!
			string adsPathLower = Path.ToLower();

			GetPrefix(adsPathLower);
			GetSuffix(adsPathLower);
			GetDistinguishedName(adsPathLower);
		}


		/// <summary>
		/// The full ADSPath.
		/// </summary>
		public string Path {
			get { return _path; }
			private set {
				_path = value;
			}
		}


		/// <summary>
		/// Everything up to the first cn= or ou= part of the ADSPath
		/// </summary>
		public string Prefix { get; private set; }


		/// <summary>
		/// Returns the trailing part of the ADSPath, or everything after (including the first) dc=
		/// </summary>
		public string Suffix { get; private set; }


		/// <summary>
		/// The distinguished name part of this ADSPath
		/// </summary>
		public string DN { get; private set; }



		/// <summary>
		/// Gets the Prefix of the ADSPath.  This is everything up to the first ou= or cn=
		/// </summary>
		/// <param name="adsPathLower"></param>
		private void GetPrefix (string adsPathLower) {
			string [] endMarkers = new [] { "/cn=", "/ou=",  "/o=", "/dc="};
			string [] endMarkersNoSlash = new [] { "cn=", "ou=",  "o="};

			int end = -1;
			foreach ( string marker in endMarkers ) {
				end = adsPathLower.IndexOf(marker);
				if (end != -1 ) break;
			}

			// Did not find a marker with leading slash
			if ( end == -1 ) {
				// Search for just the marker - maybe its an ADSPath that has no server component
				int start = -1;
				foreach (string marker in endMarkersNoSlash)
				{
					start = adsPathLower.IndexOf(marker);
					if (start != -1) break;
				}

				if ( start == -1 ) {
					Prefix = Path;
					dnStart = Path.Length;
					return;
				}

				Prefix = "";
				dnStart = start;
				return;
			}


			Prefix = Path.Substring(0, end);
			Prefix = Prefix.TrimEnd('/');

			// Skip the leading slash
			dnStart = end + 1;
		}



		/// <summary>
		/// Gets the ADSPath suffix, which is the dc= part
		/// </summary>
		/// <param name="adsPathLower"></param>
		private void GetSuffix (string adsPathLower) {
			// Looks for the first DC= to mark the start of the suffix
			int start = adsPathLower.IndexOf("/dc=");
			if (start == -1)
				start = adsPathLower.IndexOf(",dc=");

			if ( start == -1 ) {
				Suffix = "";
				dnEnd = Path.Length;
				return;
			}

			// Skip first comma
			Suffix = Path.Substring(start + 1);
			dnEnd = start;
		}


		/// <summary>
		/// Gets the Distinguished Name part of this ADSPath
		/// </summary>
		/// <param name="adsPathLower"></param>
		private void GetDistinguishedName (string adsPathLower) {
			
			if (( dnEnd == dnStart ) || dnEnd < dnStart )
				DN = "";
			else  {
				int len = dnEnd - dnStart;
				DN = Path.Substring(dnStart, len);
			}
		}


		/// <summary>
		/// Returns the position within the value of the next RDN.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		internal int IndexOfNextMarker (string value, int startingPositon = 0) {
			string[] markers = new[] { ",ou=", ",cn=", ",o=" };
			int index = -1;

			foreach (string marker in markers)
			{
				index = DN.ToLower().IndexOf(marker,startingPositon);
				if (index > -1) break;
			}

			return index;
		}


		/// <summary>
		/// Returns the parent Distinguished Name
		/// </summary>
		/// <returns></returns>
		internal string GetParentDN () {
			/*string [] markers = new [] {",ou=", ",cn=", ",o="};
			int start = -1;
			foreach ( string marker in markers ) {
				start = DN.ToLower().IndexOf(marker);
				if ( start > -1 ) break;
			}

			if ( start == -1 ) return string.Empty;
			*/

			int start = IndexOfNextMarker(DN.ToLower());
			if (start == -1) return string.Empty;
			// Skip Comma
			start++;
			return DN.Substring(start);
		}


		/// <summary>
		/// Returns the name portion only of the left most RDN. So in OU=Tampa,OU=Florida,dc=some,dc=local, it would return Tampa.
		/// </summary>
		/// <returns></returns>
		public string ShortName () {
			if ( DN == string.Empty ) return "";

			
			// TODO THERE is a BETTER WAY, we just need to find the start and end indexes, way less string copying
			int realStartIndex = 0;

			string subDN = DN.ToLower();
			
			// Does it start with a CN=?  If so we drop that.
			if ( subDN.StartsWith("cn=") ) {
				realStartIndex =  IndexOfNextMarker(subDN);

				// This should never happen
				if (realStartIndex == -1) throw new ApplicationException("Trying to remove CN=, Could not find the next RDN marker in the path - " + subDN);
			}

			// Find the first = after the RealStarting Index (ie, bypassing the cn=
			int nameStartIndex = DN.IndexOf('=',realStartIndex);
			if ( nameStartIndex == -1 ) 
				throw new ApplicationException("ShortName:  Error locating the start of the name."); 

			// Find the next marker
			int endingIndex = IndexOfNextMarker(subDN,nameStartIndex);
			int start = nameStartIndex + 1;
			int length = 0;
			string name;
			if ( endingIndex != -1 ) {
				length = endingIndex - start;
				name = DN.Substring(start, length);
			}
			else 
				name =  DN.Substring(start);
			
			return name;
		}


		/// <summary>
		/// Returns the full ADSPath of the parent of this object
		/// </summary>
		/// <returns></returns>
		public ADSPath GetParent () {
			// Build a new parent, using the same prefix and suffix as This object.  Just replace the parentDN
			string parentDN = GetParentDN();
			string fullPath = BuildFullPath(Prefix, parentDN, Suffix);
			ADSPath obj = new ADSPath(fullPath);
			return obj;
		}


		/// <summary>
		/// Builds a new ADSPath child container that has a parent of the current container.  The CN will be dropped of the current path name.
		/// <para>Example:  Current Path = LDAP://ou=office,dc=some,dc=local   New Child LDAP://ou=New Jersey,ou=office,dc=some,dc=local</para>
		/// </summary>
		/// <param name="childPart">The child container of the current object.  In format:  OU=child or OU=grandchild,OU=child</param>
		/// <returns></returns>
		public ADSPath NewChildADSPath (string childPart) {
			// Validate the childPart
			string childPartLC = childPart.ToLower();
			if (! (childPartLC.StartsWith("ou=") || childPartLC.StartsWith("o=")))
				throw new ArgumentException("Invalid ChildPart.  Child part must start with OU= or O=.  You cannot access a child by specifiying cn= either.");

			if ( childPartLC.EndsWith(",") ) childPart = childPart.TrimEnd(',');

			
			// Need to strip leading CN= off.
			string childDN = "";
			string dnLC = DN.ToLower();
			int start = -1;
			if ( dnLC.StartsWith("cn=") ) {
				start = dnLC.IndexOf(",ou=");
				if ( start == -1 ) start = dnLC.IndexOf(",dc=");
				if ( start > 0 )
					childDN = DN.Substring(start);
				else {
					childDN = DN;
				}
			}
			else
				childDN = DN;

			if ( childDN.Length == 0 ) childDN = childPart;
			else if (childDN.StartsWith(","))
				childDN = childPart + childDN;
			else {
				childDN = childPart + "," + childDN;
			}


//			string childDN = childPart + "," + DN;

			string childPath = BuildFullPath(Prefix, childDN, Suffix);
			ADSPath childADSPath = new ADSPath(childPath);
			return childADSPath;
		}


		/// <summary>
		/// Builds a Complete ADSPath from the 3 provided elements.
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="dn"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		internal string BuildFullPath (string prefix, string dn, string suffix) {
			bool suffixEmpty = suffix == string.Empty ? true : false;
			bool dnEmpty = dn == string.Empty ? true : false;
			bool prefixEmpty = prefix == string.Empty ? true : false;

			StringBuilder sb = new StringBuilder(1024);
			if (!prefixEmpty)
			{
				sb.Append(prefix);
				if (dnEmpty && suffixEmpty) return sb.ToString();

				if (prefix.ToLower() == "ldap:")
					sb.Append("//");
				else if (!dnEmpty) sb.Append("/");
				else if (!suffixEmpty) sb.Append("/");
			}

			if (!dnEmpty)
			{
				sb.Append(dn);
			}

			if (!suffixEmpty)
			{
				if (!dnEmpty) sb.Append(",");
				sb.Append(suffix);
			}

			return sb.ToString();

		}


		/// <summary>
		/// Builds the Complete path for the current ADSPath object.
		/// </summary>
		/// <returns></returns>
		public string BuildFullPath () {
			return BuildFullPath(Prefix, DN, Suffix);
		}


		/// <summary>
		/// Pretty Print!
		/// </summary>
		/// <returns></returns>
		public override string ToString () { return Path; }
	}
}

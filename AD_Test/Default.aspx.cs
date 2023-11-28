using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AD_Test
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

			bool isAuthenticated = ValidateCredentials(TextBox1.Text, TextBox2.Text, TextBox3.Text);

			if (isAuthenticated)
			{
				Literal1.Text = "Authentication successful";
			}
			else
			{
				Literal1.Text = "Authentication failed";
			}
		}

		private bool ValidateCredentials(string domain, string username, string password)
		{
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
			{
				// Validate the credentials
				return context.ValidateCredentials(username, password);
			}
		}

	}
}
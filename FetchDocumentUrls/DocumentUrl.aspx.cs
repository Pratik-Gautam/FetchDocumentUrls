using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security;
using Microsoft.SharePoint.Client;
using System.Configuration;
using System.Web.UI.HtmlControls;

namespace FetchDocumentUrls
{
    public partial class DocumentUrl : System.Web.UI.Page
    {
        #region environmental variables
        string siteUrl = ConfigurationManager.AppSettings.Get("siteUrl");
        string userName = ConfigurationManager.AppSettings.Get("userName");
        string password = ConfigurationManager.AppSettings.Get("password");
        string rootSiteUrl = ConfigurationManager.AppSettings.Get("rootSiteUrl");
        string documentLibrary = "DocumentContainer";
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //Creating client context
            using (ClientContext context = GetClientContext(siteUrl, userName, password))
            {
                //Creating web object
                Web web = context.Web;
                List list = web.Lists.GetByTitle(documentLibrary);
                context.Load(list.RootFolder.Files);
                context.ExecuteQuery();
                //Got file collection from document library post authentication
                Microsoft.SharePoint.Client.FileCollection files = list.RootFolder.Files;


                //iterating the file collection to create the html
                context.Load(web.RootFolder);
                context.ExecuteQuery();

                foreach(Microsoft.SharePoint.Client.File file in files)
                {
                    HtmlAnchor anchor = new HtmlAnchor();
                    anchor.HRef = rootSiteUrl +file.ServerRelativeUrl;
                    anchor.Title = file.Name;
                    anchor.InnerText = file.Name;
                    anchor.Target = "_Parent";
                    documents.Controls.Add(anchor);

                    documents.Controls.Add(new LiteralControl("<br />"));
                }
            }
        }

        protected void btnGetDocument_Click(object sender, EventArgs e)
        {
            string documentName = txtDocumentName.Text.Trim();
            if(!String.IsNullOrWhiteSpace(documentName))
            {
                using (ClientContext context = GetClientContext(siteUrl, userName, password))
                {

                    searchedDocuments.Controls.Clear();

                    //Creating web object
                    Web web = context.Web;
                    context.Load(web);
                    context.ExecuteQuery();

                    string documentRelativePath = "/sites/Dev1/" + documentLibrary + "/" + txtDocumentName.Text;

                    Microsoft.SharePoint.Client.File searchedFile = web.GetFileByServerRelativeUrl(documentRelativePath);
                    context.Load(searchedFile);
                    context.ExecuteQuery();

                    if (searchedFile.Exists)
                    {
                        HtmlAnchor anchor = new HtmlAnchor();
                        anchor.HRef = rootSiteUrl + searchedFile.ServerRelativeUrl;
                        anchor.Title = searchedFile.Name;
                        anchor.InnerText = searchedFile.Name;
                        anchor.Target = "_Parent";
                        searchedDocuments.Controls.Add(anchor);
                    }
                    else
                    {
                        searchedDocuments.Controls.Add(new LiteralControl("File doesnt exist"));
                    }

                }
            }
        }

        /// <summary>
        /// Mehtod to create client context readily for reuse
        /// </summary>
        /// <returns></returns>
        public static ClientContext GetClientContext(string url, string username, string passwordString)
        {
            ClientContext ctx;
            using (ClientContext clientContext = new ClientContext(url))
            {
                SecureString password = new SecureString();
                foreach (char c in passwordString) password.AppendChar(c);
                clientContext.Credentials = new SharePointOnlineCredentials(username, password);
                ctx = clientContext;
            }

            return ctx;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

namespace spMapper2
{
    public partial class map : System.Web.UI.Page
    {
        private void ProcessList(List spList, HtmlGenericControl list)
        {
            //HtmlGenericControl subSiteList = new HtmlGenericControl("ul");
            // HtmlGenericControl subSiteListItem = new HtmlGenericControl("li");
            HtmlGenericControl subSiteList = new HtmlGenericControl("li");
            //subSiteListItem.InnerText = spList.Title;
            //subSiteList.Controls.Add(subSiteListItem);
            subSiteList.InnerText = spList.Title;
            list.Controls.Add(subSiteList);
        }

        private void ProcessSite(Site site, HtmlGenericControl siteItem)
        {
            HtmlGenericControl subSiteList = new HtmlGenericControl("ul");
            HtmlGenericControl subSiteItem = new HtmlGenericControl("li");
            HtmlGenericControl innerDiv = new HtmlGenericControl("div");
            innerDiv.InnerText = site.Title;
            subSiteItem.Attributes["class"] = "mapperSite";

            //subSiteItem.Attributes["onclick"] = "$(this).slideToggle();";
            siteItem.Controls.Add(innerDiv);
            subSiteList.Controls.Add(subSiteItem);
            siteItem.Controls.Add(subSiteList);


            foreach (List spList in site.Lists)
            {
                ProcessList(spList, subSiteItem);
            }

            foreach (Site subSite in site.SubSites)
            {
                ProcessSite(subSite, subSiteItem);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"C:\Users\Rebecca\Documents\XMLFiles\SiteXML\SiteXML\root.xml");

            XmlElement topSite = (XmlElement)xml.SelectSingleNode("/sitecontents/site");

            Site site = new spMapper2.Site(topSite, null, null);

            HtmlGenericControl div = new HtmlGenericControl("div");
            mapPanel.Controls.Add(div);
            
            ProcessSite(site, div);
        }
    }
}
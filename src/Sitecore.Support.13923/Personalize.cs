using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.Dialogs.Personalize;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Managers;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using System.Collections.Generic;
using Sitecore.Globalization;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  public class Personalize : Sitecore.Shell.Applications.WebEdit.Commands.Personalize
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      ItemUri itemUri = UriHelper.ParseQueryString();
      if (itemUri != (ItemUri)null)
      {
        Item item = Database.GetItem(itemUri);
        if (item != null && !WebEditUtil.CanDesignItem(item))
        {
          SheerResponse.Alert("The action cannot be executed because of security restrictions.", Array.Empty<string>());
          return;
        }
        // Patch 
        if ((item.TemplateName == "Page" || item.TemplateName == "Home") && item.Fields["Page Design"] != null)
        {
          string tempForm = WebUtil.GetFormValue("scLayout");
          string tempUid = ShortID.Decode(context.Parameters["uniqueId"]);
          string tempXml = this.ConvertToXml(tempForm);
          if (tempUid != null && tempXml != null)
          {
            if (!tempXml.Contains(tempUid))// All these if() checks whether or not it is a partial design without using SXA extension methods.
            {
              SheerResponse.Alert("This rendering is a Partial Design. Please apply Personalization on Partial Design instead.");
              return;
            }
          }
        }
        // End Patch
      }
      string formValue = WebUtil.GetFormValue("scLayout");
      Assert.IsNotNullOrEmpty(formValue, "Layout Definition");
      string value = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
      Assert.IsNotNullOrEmpty(value, "device ID");
      string value2 = ShortID.Decode(context.Parameters["uniqueId"]);
      Assert.IsNotNullOrEmpty(value2, "Unique ID");
      string value3 = this.ConvertToXml(formValue);
      Assert.IsNotNullOrEmpty(value3, "convertedLayoutDefition");
      WebUtil.SetSessionValue("PEPesonalization", value3);
      NameValueCollection nameValueCollection = new NameValueCollection();
      nameValueCollection["deviceId"] = value;
      nameValueCollection["uniqueId"] = value2;
      if (itemUri != (ItemUri)null)
      {
        nameValueCollection["contextItemUri"] = itemUri.ToString();
      }
      Context.ClientPage.Start(this, "Run", nameValueCollection);
    }
    protected virtual string ConvertToXml(string layout)
    {
      Assert.ArgumentNotNull(layout, "layout");
      return WebEditUtil.ConvertJSONLayoutToXML(layout);
    }
  }
  public class UriHelper
  {
    public static ItemUri ParseQueryString()
    {
      Database database = Context.Database;
      if (database == null)
      {
        return null;
      }
      string queryString = WebUtil.GetQueryString("id", null);
      if (string.IsNullOrEmpty(queryString))
      {
        return null;
      }
      string queryString2 = WebUtil.GetQueryString("db", database.Name);
      string queryString3 = WebUtil.GetQueryString("url");
      string value = "0";
      if (!string.IsNullOrEmpty(queryString3))
      {
        SafeDictionary<string> safeDictionary = WebUtil.ParseQueryString(queryString3);
        value = (string.IsNullOrEmpty(((SafeDictionary<string, string>)safeDictionary)["sc_version"]) ? "0" : ((SafeDictionary<string, string>)safeDictionary)["sc_version"]);
      }
      Language current = default(Language);
      if (!Language.TryParse(WebUtil.GetQueryString("la"), out current))
      {
        current = Language.Current;
      }
      return new ItemUri(ID.Parse(queryString), current, Data.Version.Parse(value), queryString2);
    }
  }
}
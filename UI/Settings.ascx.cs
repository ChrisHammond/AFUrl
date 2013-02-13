using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Tabs;
using iFinity.DNN.Modules.UrlMaster;
using iFinity.DNN.Modules.UrlMaster.Entities;

namespace DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.UI
{
    /// <summary>
    /// This is the code-behind for the Settings.ascx control.  This inherits from the standard .NET UserControl, but also implements the ModuleProvider specific IProviderSettings.
    /// This control will be loaded by the Portal Urls page.  It is optional for module providers, but allows users to control module settings via the interface, rather than 
    /// having to set options via web.config settings.  The writing / reading of the items from the configuration file is handled by the Url Master module, and doesn't need to 
    /// be implemented.
    /// </summary>
    public partial class Settings : System.Web.UI.UserControl, IProviderSettings
    {
        private int _portalId;
        #region controls
        protected Label lblHeader;
        protected TextBox txtUrlPath;
        protected DropDownList ddlNoDnnPagePathTab;
        #endregion
        #region Web Form Designer Generated Code
        //[System.Diagnostics.DebuggerStepThrough]
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }

        #endregion
        #region events code
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //note page load runs after LoadSettings(); because of dynamic control loading
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        #endregion
        #region content methods
        private void LocalizeControls()
        {
            lblHeader.Text = DotNetNuke.Services.Localization.Localization.GetString("Header.Text");

            //only publicly available non-admin or host tabs are used.
            TabController tc = new TabController();
            //load tabs into drop down list            
            TabCollection tabs = tc.GetTabsByPortal(_portalId);
            //loop through the tabs and add the ones that aren't admin or host tabs
            ddlNoDnnPagePathTab.Items.Clear();
            string firstItem = DotNetNuke.Services.Localization.Localization.GetString("NoTabSelected.Text", this.LocalResourceFile);
            ddlNoDnnPagePathTab.Items.Add(new ListItem(firstItem, "-1"));
            foreach (TabInfo tab in tabs.Values)
            {
                if (tab.IsSuperTab == false)//TODO: filter admin tabs as well?
                    ddlNoDnnPagePathTab.Items.Add(new ListItem(tab.TabName, tab.TabID.ToString()));
            }
            
        }
        #endregion
        #region IProviderSettings Members
        /// <summary>
        /// LoadSettings is called when the module control is first loaded onto the page
        /// </summary>
        /// <remarks>
        /// This method shoudl read all the custom properties of the provider and set the controls
        /// of the page to reflect the current settings of the provider.
        /// </remarks>
        /// <param name="provider"></param>
        public void LoadSettings(iFinity.DNN.Modules.UrlMaster.ModuleFriendlyUrlProvider provider)
        {
            //build list of controls
            if (!IsPostBack)
                LocalizeControls();
            //take all the values from the provider and show on page
            //check type safety before cast
            if (provider.GetType() == typeof(ActiveForumsModuleProvider))
            {
                ActiveForumsModuleProvider moduleProvider = (ActiveForumsModuleProvider)provider;
                
                txtUrlPath.Text = moduleProvider.UrlPath;
                
                string selTabId = moduleProvider.NoDnnPagePathTabId.ToString();
                ddlNoDnnPagePathTab.SelectedValue = selTabId;
                if (ddlNoDnnPagePathTab.SelectedIndex == -1)//not found
                {
                    ddlNoDnnPagePathTab.SelectedIndex = 0;//first item
                }

            }
        }
        /// <summary>
        /// UpdateSettings is called when the 'update' button is clicked on the interface.
        /// This should take any values from the page, and set the individual properties on the 
        /// instance of the module provider.
        /// </summary>
        /// <param name="provider"></param>
        public void UpdateSettings(iFinity.DNN.Modules.UrlMaster.ModuleFriendlyUrlProvider provider)
        {
            //check type safety before cast
            if (provider.GetType() == typeof(ActiveForumsModuleProvider))
            {
                //take values from the page and set values on provider    
                ActiveForumsModuleProvider moduleProvider = (ActiveForumsModuleProvider)provider;
                
                moduleProvider.UrlPath = txtUrlPath.Text;
                
                string rawTabId = ddlNoDnnPagePathTab.SelectedValue;
                int selTabId;
                if (int.TryParse(rawTabId, out selTabId))
                {
                    if (selTabId > 0)
                        moduleProvider.NoDnnPagePathTabId = selTabId;
                    else
                        moduleProvider.NoDnnPagePathTabId = 0;
                }
                
            }

        }
        
        public System.Web.UI.Control Control
        {
            get { return this;  }
        }

        public string ControlPath
        {
            get { return base.TemplateSourceDirectory; }
        }

        public string ControlName
        {
            get { return "ProviderSettings";  }
        }

        public string LocalResourceFile
        {
            get { return "DesktopModules/DotNetNuke.ActiveForumsFriendlyUrlProvider/App_LocalResources/Settings.ascx.resx"; }
        }
        public int PortalId
        {
            get { return _portalId; }
            set { _portalId = value; }
        }

        #endregion

    }
}

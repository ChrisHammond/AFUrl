using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DotNetNuke.Entities.Modules;
using DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Entities;

namespace DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Data
{
    internal abstract class DataProvider
    {
        #region Shared/Static Methods

        // singleton reference to the instantiated object 
        static DataProvider objProvider = null;

        // constructor
        static DataProvider()
        {
            CreateProvider();
        }

        // dynamically create provider
        private static void CreateProvider()
        {
            //don't need run-time instancing when using built-in sqlDataProvider class
            objProvider = new Data.SqlDataProvider();
        }

        // return the provider
        public static DataProvider Instance()
        {
            return objProvider;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Returns list of Urls related to your module
        /// </summary>
        /// <param name="tabId">The tabid the module is on.</param>
        /// <param name="entries">out parameter returns a zero-n list of ActiveForums Urls</param>
        /// <remarks>Example only - this call can be anything you like.</remarks>
        internal abstract void GetActiveForumsGroups(int tabId, out FriendlyUrlInfoCol urls);
        internal abstract void GetActiveForumsForums(int tabId, out FriendlyUrlInfoCol urls);
        internal abstract void GetActiveForumsThreads(int tabId, out FriendlyUrlInfoCol urls);
        internal abstract void GetActiveForumsReplies(int tabId, out FriendlyUrlInfoCol urls);

        #endregion

        internal FriendlyUrlInfo GetItemUrl(int itemId, int tabId)
        {
            throw new NotImplementedException();
        }
    }
}

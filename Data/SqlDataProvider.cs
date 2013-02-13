using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Microsoft.ApplicationBlocks.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Modules;

using DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Entities;

namespace DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Data
{
    internal class SqlDataProvider : DataProvider 
    {
        //TODO : set DB naming prefix values for module.        
        private const string ModuleQualifier = "";
        private const string OwnerQualifier = "";

        #region Private Members
        private const string ProviderType = "data";
        private ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private string _connectionString;
        private string _providerPath;
        private string _objectQualifier;
        private string _databaseOwner;
        #endregion
        #region Constructor
        /// <summary>
        /// Constructs new SqlDataProvider instance
        /// </summary>
        internal SqlDataProvider()
        {
            //Read the configuration specific information for this provider
            Provider objProvider = (Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider];

            //Read the attributes for this provider
            //Get Connection string from web.config
            _connectionString = Config.GetConnectionString();

            if (_connectionString.Length == 0)
            {
                // Use connection string specified in provider
                _connectionString = objProvider.Attributes["connectionString"];
            }

            _providerPath = objProvider.Attributes["providerPath"];

            //override the standard dotNetNuke qualifier with a iFinity one if it exists
            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if ((_objectQualifier != "") && (_objectQualifier.EndsWith("_") == false))
            {
                _objectQualifier += "_";
            }
            else
                if (_objectQualifier == null) _objectQualifier = "";

            _objectQualifier += OwnerQualifier;

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if ((_databaseOwner != "") && (_databaseOwner.EndsWith(".") == false))
            {
                _databaseOwner += ".";
            }
        }
        #endregion
        #region Properties

        /// <summary>
        /// Gets and sets the connection string
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
        }
        /// <summary>
        /// Gets and sets the Provider path
        /// </summary>
        public string ProviderPath
        {
            get { return _providerPath; }
        }
        /// <summary>
        /// Gets and sets the Object qualifier
        /// </summary>
        public string ObjectQualifier
        {
            get { return _objectQualifier; }
        }
        /// <summary>
        /// Gets and sets the database ownere
        /// </summary>
        public string DatabaseOwner
        {
            get { return _databaseOwner; }
        }

        #endregion
        #region private members
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the fully qualified name of the stored procedure
        /// </summary>
        /// <param name="name">The name of the stored procedure</param>
        /// <returns>The fully qualified name</returns>
        /// -----------------------------------------------------------------------------
        private string GetFullyQualifiedName(string name)
        {
            return DatabaseOwner + ObjectQualifier + ModuleQualifier + name;
        }
        #endregion

        #region abstract overridden properties
        internal override void GetActiveForumsGroups(int tabId, out FriendlyUrlInfoCol urls)
        {
            urls = new FriendlyUrlInfoCol();
            var dr = SqlHelper.ExecuteReader(_connectionString, CommandType.Text,
                                    "select * from " + GetFullyQualifiedName("dnntoaf_groups"));
            while (dr.Read())
            {
                var url = new FriendlyUrlInfo();
                if (!Convert.IsDBNull(dr["newGroupId"]))
                    url.newId = Convert.ToInt32(dr["newGroupId"]);
                if (!Convert.IsDBNull(dr["oldGroupId"]))
                    url.itemId = (int)dr["oldGroupId"];
                urls.Add(url);
            }
            dr.Close();
            dr.Dispose();
        }

        internal override void GetActiveForumsForums(int tabId, out FriendlyUrlInfoCol urls)
        {
            urls = new FriendlyUrlInfoCol();
            var dr = SqlHelper.ExecuteReader(_connectionString, CommandType.Text,
                                    "select * from " + GetFullyQualifiedName("dnntoaf_forums"));
            while (dr.Read())
            {
                var url = new FriendlyUrlInfo();
                if (!Convert.IsDBNull(dr["newForumId"]))
                    url.newId= Convert.ToInt32(dr["newForumId"]);
                if (!Convert.IsDBNull(dr["oldForumId"]))
                    url.itemId = (int)dr["oldForumId"];
                urls.Add(url);
            }
            dr.Close();
            dr.Dispose();
        }

        internal override void GetActiveForumsThreads(int tabId, out FriendlyUrlInfoCol urls)
        {
            urls = new FriendlyUrlInfoCol();
            var dr = SqlHelper.ExecuteReader(_connectionString, CommandType.Text,
                                    "select * from " + GetFullyQualifiedName("dnntoaf_topics"));
            while (dr.Read())
            {
                var url = new FriendlyUrlInfo();
                if (!Convert.IsDBNull(dr["newTopicId"]))
                    url.newId = Convert.ToInt32(dr["newTopicId"]);
                if (!Convert.IsDBNull(dr["oldPostId"]))
                    url.itemId = (int)dr["oldPostId"];
                urls.Add(url);
            }
            dr.Close();
            dr.Dispose();
        }

        internal override void GetActiveForumsReplies(int tabId, out FriendlyUrlInfoCol urls)
        {
            urls = new FriendlyUrlInfoCol();
            var dr = SqlHelper.ExecuteReader(_connectionString, CommandType.Text,
                                    "select oldpostid, newreplyid, nr.topicid, nr.ContentId from "
                                    + GetFullyQualifiedName("dnntoaf_replies") + " r join "
                                    + GetFullyQualifiedName("ActiveForums_Replies") + " nr on (r.newreplyid=nr.replyid)");
            while (dr.Read())
            {
                var url = new FriendlyUrlInfo();
                if (!Convert.IsDBNull(dr["newReplyId"]))
                    url.newId = Convert.ToInt32(dr["newReplyId"]);
                if (!Convert.IsDBNull(dr["oldPostId"]))
                    url.itemId = (int)dr["oldPostId"];
                if (!Convert.IsDBNull(dr["TopicId"]))
                    url.newParentId = Convert.ToInt32(dr["TopicId"]);
                if (!Convert.IsDBNull(dr["ContentId"]))
                    url.newContentId = Convert.ToInt32(dr["ContentId"]);
                urls.Add(url);
            }
            dr.Close();
            dr.Dispose();
        }


        #endregion
    }
}

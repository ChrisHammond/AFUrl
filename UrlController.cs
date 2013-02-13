using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DotNetNuke.Common.Utilities;
using iFinity.DNN.Modules.UrlMaster;
using DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Entities;

namespace DotNetNuke.ActiveForumsModuleFriendlyUrlProvider
{
    internal static class UrlController
    {
        //keys used for cache entries for Urls and Querystrings
        private const string FriendlyUrlIndexKey = "DotNetNuke_ActiveForums_Urls_Tab{0}";
        private const string QueryStringIndexKey = "DotNetNuke_ActiveForums_QueryString_Tab{0}";

        /// <summary>
        /// Checks for, and adds to the indexes, a missing item.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="tabId"></param>
        /// <param name="portalId"></param>
        /// <param name="provider"></param>
        /// <param name="options"></param>
        /// <param name="messages"></param>
        /// <returns>Valid path if found</returns>
        internal static string CheckForMissingItemId(int itemId, int tabId, int portalId, ActiveForumsModuleProvider provider, FriendlyUrlOptions options, ref List<string> messages)
        {
            string path = null;
            FriendlyUrlInfo friendlyUrl = Data.DataProvider.Instance().GetItemUrl(itemId, tabId);
            messages.Add("itemId not found : " + itemId.ToString() + " Checking Item directly");
            if (friendlyUrl != null)
            {
                messages.Add("itemId found : " + itemId.ToString() + " Rebuilding indexes");
                //call and get the path
                path = UrlController.MakeItemFriendlyUrl(friendlyUrl, provider, options);
                //so this entry did exist but wasn't in the index.  Rebuild the index
                UrlController.RebuildIndexes(tabId, portalId, provider, options);
            }
            return path;
        }

        /// <summary>
        /// Creates a Friendly Url For the Item
        /// </summary>
        /// <param name="friendlyUrl">Object containing the relevant properties to create a friendly url from</param>
        /// <param name="provider">The active module provider</param>
        /// <param name="options">THe current friendly Url Options</param>
        /// <returns></returns>
        private static string MakeItemFriendlyUrl(FriendlyUrlInfo friendlyUrl, ActiveForumsModuleProvider provider, FriendlyUrlOptions options)
        {
            //calls back up the module provider to utilise the CleanNameForUrl method, which creates a safe Url using the current Url Master options.
            string friendlyUrlPath = provider.CleanNameForUrl(friendlyUrl.urlFragment, options);
            return friendlyUrlPath;
        }
        /// <summary>
        /// Returns a friendly url index from the cache or database.
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="portalId"></param>
        /// <param name="ActiveForumsModuleProvider"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static Hashtable GetFriendlyUrlIndex(int tabId, int portalId, ActiveForumsModuleProvider provider, FriendlyUrlOptions options)
        {
            string furlCacheKey = GetFriendlyUrlIndexKeyForTab(tabId);
            Hashtable friendlyUrlIndex = DataCache.GetCache<Hashtable>(furlCacheKey);
            if (friendlyUrlIndex == null)
            {
                string qsCacheKey = GetQueryStringIndexCacheKeyForTab(tabId);
                Hashtable queryStringIndex = null;
                //build index for tab
                BuildUrlIndexes(tabId, portalId, provider, options, out friendlyUrlIndex, out queryStringIndex);
                StoreIndexes(friendlyUrlIndex, furlCacheKey, queryStringIndex, qsCacheKey);
            }
            return friendlyUrlIndex;

        }
        /// <summary>
        /// Return the index of all the querystrings that belong to friendly urls for the specific tab.
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        internal static Hashtable GetQueryStringIndex(int tabId, int portalId, ActiveForumsModuleProvider provider, FriendlyUrlOptions options, bool forceRebuild)
        {
            string qsCacheKey = GetQueryStringIndexCacheKeyForTab(tabId);
            Hashtable queryStringIndex = DataCache.GetCache<Hashtable>(qsCacheKey);
            if (queryStringIndex == null || forceRebuild)
            {
                string furlCacheKey = GetFriendlyUrlIndexKeyForTab(tabId);
                Hashtable friendlyUrlIndex = null;
                //build index for tab
                BuildUrlIndexes(tabId, portalId, provider, options, out friendlyUrlIndex, out queryStringIndex);
                StoreIndexes(friendlyUrlIndex, furlCacheKey, queryStringIndex, qsCacheKey);
            }
            return queryStringIndex;
        }

        private static void BuildUrlIndexes(int tabId, int portalId, ActiveForumsModuleProvider provider, FriendlyUrlOptions options, out Hashtable friendlyUrlIndex, out Hashtable queryStringIndex)
        {
            friendlyUrlIndex = new Hashtable();
            queryStringIndex = new Hashtable();
            //call database procedure to get list of 
            FriendlyUrlInfoCol itemUrls = null;
            Data.DataProvider.Instance().GetActiveForumsGroups(tabId, out itemUrls);
            if (itemUrls != null)
            {
                //build up the dictionary for Groups
                foreach (FriendlyUrlInfo itemUrl in itemUrls)
                {
                    //friendly url index - look up by entryid, find Url
                    string furlKey = "groupi" + itemUrl.itemId.ToString();
                    
                    //string furlValue = MakeItemFriendlyUrl(itemUrl, provider, options);
                    if (friendlyUrlIndex.ContainsKey(furlKey) == false)
                        friendlyUrlIndex.Add(furlKey, "afg/" + itemUrl.newId);

                    ////querystring index - look up by url, find querystring for the item
                    //string qsKey = furlValue.ToLower();//the querystring lookup is the friendly Url value - but converted to lower case
                    //string qsValue = "&afg=" + itemUrl.itemId.ToString();//the querystring is just the entryId parameter

                    ////when not including the dnn page path into the friendly Url, then include the tabid in the querystring
                    //if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                    //    qsValue = "?TabId=" + tabId.ToString() + qsValue;
                    //if (queryStringIndex.ContainsKey(qsKey) == false)
                    //    queryStringIndex.Add(qsKey, qsValue);

                    ////if the options aren't standard, also add in some other versions that will identify the right entry but will get redirected
                    //if (options.PunctuationReplacement != "")
                    //{
                    //    FriendlyUrlOptions altOptions = options.Clone();
                    //    altOptions.PunctuationReplacement = "";//how the urls look with no replacement
                    //    string altQsKey = MakeItemFriendlyUrl(itemUrl, provider, altOptions).ToLower();//keys are always lowercase
                    //    if (altQsKey != qsKey)
                    //    {
                    //        if (queryStringIndex.ContainsKey(altQsKey) == false)
                    //            queryStringIndex.Add(altQsKey, qsValue);
                    //    }
                    //}
                }
            }

            Data.DataProvider.Instance().GetActiveForumsForums(tabId, out itemUrls);
            if (itemUrls != null)
            {
                //build up the dictionary for Groups
                foreach (FriendlyUrlInfo itemUrl in itemUrls)
                {
                    //friendly url index - look up by entryid, find Url
                    string furlKey = "forumi" + itemUrl.itemId.ToString();
                    //string furlValue = MakeItemFriendlyUrl(itemUrl, provider, options);
                    if (friendlyUrlIndex.ContainsKey(furlKey) == false)
                        friendlyUrlIndex.Add(furlKey, "aff/" + itemUrl.newId);

                    ////querystring index - look up by url, find querystring for the item
                    //string qsKey = furlValue.ToLower();//the querystring lookup is the friendly Url value - but converted to lower case
                    //string qsValue = "&aff=" + itemUrl.itemId.ToString();//the querystring is just the entryId parameter

                    ////when not including the dnn page path into the friendly Url, then include the tabid in the querystring
                    //if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                    //    qsValue = "?TabId=" + tabId.ToString() + qsValue;
                    //if (queryStringIndex.ContainsKey(qsKey) == false)
                    //    queryStringIndex.Add(qsKey, qsValue);

                    ////if the options aren't standard, also add in some other versions that will identify the right entry but will get redirected
                    //if (options.PunctuationReplacement != "")
                    //{
                    //    FriendlyUrlOptions altOptions = options.Clone();
                    //    altOptions.PunctuationReplacement = "";//how the urls look with no replacement
                    //    string altQsKey = MakeItemFriendlyUrl(itemUrl, provider, altOptions).ToLower();//keys are always lowercase
                    //    if (altQsKey != qsKey)
                    //    {
                    //        if (queryStringIndex.ContainsKey(altQsKey) == false)
                    //            queryStringIndex.Add(altQsKey, qsValue);
                    //    }
                    //}
                }
            }
            Data.DataProvider.Instance().GetActiveForumsThreads(tabId, out itemUrls);
            if (itemUrls != null)
            {
                //build up the dictionary for Groups
                foreach (FriendlyUrlInfo itemUrl in itemUrls)
                {
                    //friendly url index - look up by entryid, find Url
                    string furlKey = "threadi" + itemUrl.itemId.ToString();
                    //string furlValue = MakeItemFriendlyUrl(itemUrl, provider, options);
                    if (friendlyUrlIndex.ContainsKey(furlKey) == false)
                        friendlyUrlIndex.Add(furlKey, "aft/" + itemUrl.newId);

                    ////querystring index - look up by url, find querystring for the item
                    //string qsKey = furlValue.ToLower();//the querystring lookup is the friendly Url value - but converted to lower case
                    //string qsValue = "&aft=" + itemUrl.itemId.ToString();//the querystring is just the entryId parameter

                    ////when not including the dnn page path into the friendly Url, then include the tabid in the querystring
                    //if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                    //    qsValue = "?TabId=" + tabId.ToString() + qsValue;
                    //if (queryStringIndex.ContainsKey(qsKey) == false)
                    //    queryStringIndex.Add(qsKey, qsValue);

                    ////if the options aren't standard, also add in some other versions that will identify the right entry but will get redirected
                    //if (options.PunctuationReplacement != "")
                    //{
                    //    FriendlyUrlOptions altOptions = options.Clone();
                    //    altOptions.PunctuationReplacement = "";//how the urls look with no replacement
                    //    string altQsKey = MakeItemFriendlyUrl(itemUrl, provider, altOptions).ToLower();//keys are always lowercase
                    //    if (altQsKey != qsKey)
                    //    {
                    //        if (queryStringIndex.ContainsKey(altQsKey) == false)
                    //            queryStringIndex.Add(altQsKey, qsValue);
                    //    }
                    //}
                }
            }


            Data.DataProvider.Instance().GetActiveForumsReplies(tabId, out itemUrls);
            if (itemUrls != null)
            {
                //build up the dictionary for Groups
                foreach (FriendlyUrlInfo itemUrl in itemUrls)
                {
                    //friendly url index - look up by entryid, find Url
                    string furlKey = "replyi" + itemUrl.itemId.ToString();
                    //string furlValue = MakeItemFriendlyUrl(itemUrl, provider, options);
                    if (friendlyUrlIndex.ContainsKey(furlKey) == false)
                        friendlyUrlIndex.Add(furlKey, "aft/" + itemUrl.newParentId + "#" + itemUrl.newContentId);

                    ////querystring index - look up by url, find querystring for the item
                    //string qsKey = furlValue.ToLower();//the querystring lookup is the friendly Url value - but converted to lower case
                    //string qsValue = "&aft=" + itemUrl.itemId.ToString();//the querystring is just the entryId parameter

                    ////when not including the dnn page path into the friendly Url, then include the tabid in the querystring
                    //if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                    //    qsValue = "?TabId=" + tabId.ToString() + qsValue;
                    //if (queryStringIndex.ContainsKey(qsKey) == false)
                    //    queryStringIndex.Add(qsKey, qsValue);

                    //if the options aren't standard, also add in some other versions that will identify the right entry but will get redirected
                    //if (options.PunctuationReplacement != "")
                    //{
                    //    FriendlyUrlOptions altOptions = options.Clone();
                    //    altOptions.PunctuationReplacement = "";//how the urls look with no replacement
                    //    string altQsKey = MakeItemFriendlyUrl(itemUrl, provider, altOptions).ToLower();//keys are always lowercase
                    //    if (altQsKey != qsKey)
                    //    {
                    //        if (queryStringIndex.ContainsKey(altQsKey) == false)
                    //            queryStringIndex.Add(altQsKey, qsValue);
                    //    }
                    //}
                }
            }

        }

        /// <summary>
        /// REbuilds the two indexes and re-stores them into the cache
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="portalId"></param>
        /// <param name="provider"></param>
        /// <param name="options"></param>
        private static void RebuildIndexes(int tabId, int portalId, ActiveForumsModuleProvider provider, FriendlyUrlOptions options)
        {
            Hashtable queryStringIndex = null;
            Hashtable friendlyUrlIndex = null;
            string qsCacheKey = GetQueryStringIndexCacheKeyForTab(tabId);
            string furlCacheKey = GetFriendlyUrlIndexKeyForTab(tabId);
            //build index for tab
            BuildUrlIndexes(tabId, portalId, provider, options, out friendlyUrlIndex, out queryStringIndex);
            StoreIndexes(friendlyUrlIndex, furlCacheKey, queryStringIndex, qsCacheKey);
        }
        /// <summary>
        /// Store the two indexes into the cache
        /// </summary>
        /// <param name="friendlyUrlIndex"></param>
        /// <param name="friendlyUrlCacheKey"></param>
        /// <param name="queryStringIndex"></param>
        /// <param name="queryStringCacheKey"></param>
        private static void StoreIndexes(Hashtable friendlyUrlIndex, string friendlyUrlCacheKey, Hashtable queryStringIndex, string queryStringCacheKey)
        {
            TimeSpan expire = new TimeSpan(24, 0, 0);
            DataCache.SetCache(friendlyUrlCacheKey, friendlyUrlIndex, expire);
            DataCache.SetCache(queryStringCacheKey, queryStringIndex, expire);
        }

        /// <summary>
        /// Return the caceh key for a tab index
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns></returns>
        private static string GetFriendlyUrlIndexKeyForTab(int tabId)
        {
            return string.Format(FriendlyUrlIndexKey, tabId);
        }
        private static string GetQueryStringIndexCacheKeyForTab(int tabId)
        {
            return string.Format(QueryStringIndexKey, tabId);
        }
    }
}

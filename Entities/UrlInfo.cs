using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.Entities
{
    internal class FriendlyUrlInfo
    {
        public int itemId;
        public string urlFragment;
        public int newId;
        public int newParentId;
        public int newContentId;

    }
    internal class FriendlyUrlInfoCol : List<FriendlyUrlInfo>
    {
    }
}

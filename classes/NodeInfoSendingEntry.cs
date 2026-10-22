using System;

namespace meshbridge.classes
{
    internal class NodeInfoSendingEntry
    {
        public DateTimeOffset SeenTime { get; set; }

        public DateTimeOffset? SentTime { get; set; }

        public NodeInfoData NodeInfo { get; set; }

        public NodeInfoSendingEntry(NodeInfoData nodeInfo)
        {
            SeenTime = DateTimeOffset.Now;
            NodeInfo = nodeInfo;
        }
    }
}

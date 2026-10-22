namespace meshbridge.classes
{
    public record NodeInfoData
    {
        public required uint NodeId;

        public required string ShortName;

        public required string LongName;

        public NodeInfoData() { }

        public NodeInfoData(uint nodeId, string shortName) : this(nodeId, shortName, shortName) { }

        public NodeInfoData(uint nodeId, string shortName, string longName)
        {
            NodeId = nodeId;
            ShortName = shortName;
            LongName = longName;
        }
    }
}

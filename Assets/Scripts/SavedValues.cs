namespace xUnityTools.WinnerPick
{
    [System.Serializable]
    public class SavedValues
    {
        public string proposalId;
        public string APIKey;
        public string[] bannedAddresses;
        public float waitTime = 0.05f;
    }
}
namespace xUnityTools.WinnerPick
{
    //Used to deserialize response data
    [System.Serializable]
    public class PollData
    {
        public PollOptionData[] data;
    }


    [System.Serializable]
    public class PollOptionData
    {
        public string tx;
        public int pollOption;
        public string upvotes;
        public object downvotes;
        public string createdAt;
        public UserData user;
    }


    [System.Serializable]
    public class UserData
    {
        public string address;
        public string username;
    }
}



using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace xUnityTools.WinnerPick
{
    public class WinnerPick : MonoBehaviour
    {
        const string healthAPI = "https://api.peerme.io/v1/health";
        const string startText = "Good luck!";


        [SerializeField] GameObject congrats;
        [SerializeField] GameObject selectWinner;
        [SerializeField] GameObject settings;
        [SerializeField] InputField addressText;
        [SerializeField] InputField apiTokenInput;
        [SerializeField] InputField proposalIDInput;
        [SerializeField] InputField walletDisplayTimeInput;
        [SerializeField] InputField excludedWalletsInput;
        [SerializeField] Text version;


        private SavedValues savedValues;


        private void Start()
        {
            //PlayerPrefs.DeleteAll();
            //need to force the wrap mode from code - probably a Unity editor bug
            addressText.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;

            //load save
            LoadSavedValues();

            //set default values
            addressText.text = startText;
            congrats.SetActive(false);
            settings.SetActive(false);
            selectWinner.SetActive(true);
            version.text = $"Made by XUnityTools v.{Application.version}";
        }


        /// <summary>
        /// Open/Close the settings panel
        /// </summary>
        /// <param name="show"></param>
        public void ShowSettings(bool show)
        {
            settings.SetActive(show);
            selectWinner.SetActive(!show);
            if (!show)
            {
                SaveValues();
            }
        }


        /// <summary>
        /// Start winner selection process
        /// </summary>
        public void SelectWinner()
        {
            StartCoroutine(CallAPI(healthAPI, APIStatusOK));
        }


        /// <summary>
        /// Load and populate settings data
        /// </summary>
        private void LoadSavedValues()
        {
            Storage storage = new Storage();
            savedValues = storage.Load();
            apiTokenInput.text = savedValues.APIKey;
            proposalIDInput.text = savedValues.proposalId;
            walletDisplayTimeInput.text = savedValues.waitTime.ToString();
            excludedWalletsInput.text = "";
            if (savedValues.bannedAddresses != null)
            {
                for (int i = 0; i < savedValues.bannedAddresses.Length; i++)
                {
                    excludedWalletsInput.text += savedValues.bannedAddresses[i] + "\n";
                }
            }
        }


        /// <summary>
        /// Save the values from input fields into PlayerPrefs
        /// </summary>
        private void SaveValues()
        {
            savedValues.APIKey = apiTokenInput.text;
            savedValues.proposalId = proposalIDInput.text;

            float waitTime;
            if (float.TryParse(walletDisplayTimeInput.text, out waitTime))
            {
                savedValues.waitTime = waitTime;
            }
            else
            {
                addressText.text = "Time parse error";
            }


            string[] wallets = excludedWalletsInput.text.Split("\n");
            savedValues.bannedAddresses = wallets;
            Storage storage = new Storage();
            storage.Save(savedValues);
            addressText.text = startText;
            congrats.SetActive(false);
        }


        /// <summary>
        /// If the API status is good, load voters
        /// </summary>
        /// <param name="message"></param>
        private void APIStatusOK(string message)
        {
            addressText.text = "Loading voters data";
            StartCoroutine(CallAPI($"https://api.peerme.io/v1/proposals/{savedValues.proposalId}/votes", VotersLoaded));
        }


        /// <summary>
        /// Collect voters data
        /// </summary>
        /// <param name="message">json containing all voters data</param>
        private void VotersLoaded(string message)
        {
            List<string> votingAddresses = new List<string>();
            PollData pollData = JsonUtility.FromJson<PollData>(message);

            PollOptionData[] pollOptionData = pollData.data;

            //store each user wallet, except the banned ones
            foreach (PollOptionData data in pollOptionData)
            {
                if (!savedValues.bannedAddresses.Contains(data.user.address))
                {
                    votingAddresses.Add(data.user.address);
                }
            }

            StartCoroutine(PickWinner(votingAddresses));
        }


        /// <summary>
        /// Displays all addresses on screen and picks a random one
        /// </summary>
        /// <param name="votingAddresses"></param>
        /// <returns></returns>
        private IEnumerator PickWinner(List<string> votingAddresses)
        {
            foreach (string address in votingAddresses)
            {
                addressText.text = address;
                yield return new WaitForSeconds(savedValues.waitTime);
            }

            yield return new WaitForSeconds(savedValues.waitTime);
            int winnerIndex = Random.Range(0, votingAddresses.Count);
            addressText.text = votingAddresses[winnerIndex];
            congrats.SetActive(true);
        }


        //does not work in WebGL :(
        public void Copy()
        {
            GUIUtility.systemCopyBuffer = addressText.text;
        }


        /// <summary>
        /// Method for calling PeerMe APIs
        /// </summary>
        /// <param name="url"></param>
        /// <param name="completeMethid">called if call is succesful</param>
        /// <returns></returns>
        IEnumerator CallAPI(string url, UnityAction<string> completeMethid)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {savedValues.APIKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 401)
                {
                    addressText.text = $"{request.error} \n Check your API Token";
                }
                else
                {
                    addressText.text = $"{request.url} \n {request.error}";
                }
            }
            else
            {
                completeMethid?.Invoke(request.downloadHandler.text);
            }
        }
    }
}
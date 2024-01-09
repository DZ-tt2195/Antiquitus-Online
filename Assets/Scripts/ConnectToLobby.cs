using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class ConnectToLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] Button localPlay;
    [SerializeField] TMP_InputField username;

    public void Start()
    {
        localPlay.onClick.AddListener(LocalPlay);
        Application.targetFrameRate = 60;
        if (PlayerPrefs.HasKey("Username"))
            username.text = PlayerPrefs.GetString("Username");
    }

    void LocalPlay()
    {
        if (username.text != "")
        {
            PlayerPrefs.SetString("Username", username.text);
            SceneManager.LoadScene("2. Game");
        }
    }

    public void Join(string region)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("1. Lobby");
    }
}

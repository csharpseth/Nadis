using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIServerBrowser : MonoBehaviour
{
    public Transform listContainer;
    public GameObject serverElementPrefab;

    private void Start()
    {
        DarwinAPIManager.GetServers(Populate);
    }

    public TextMeshProUGUI[] ServerButtonSchema()
    {
        
        GameObject temp = Instantiate(serverElementPrefab, listContainer);
        TextMeshProUGUI[] texts = new TextMeshProUGUI[temp.transform.childCount];
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i] = temp.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
        }

        return texts;

    }
    public void ClearList()
    {
        int size = listContainer.childCount;
        for (int i = 0; i < size; i++)
        {
            Destroy(listContainer.GetChild(i).gameObject);
        }
    }

    public void Populate(ServerData[] data) 
    {
        ClearList();
        
        if(data != null)
        {


            for (int i = 0; i < data.Length; i++)
            {
                TextMeshProUGUI[] texts = ServerButtonSchema();
                texts[0].text = data[i].displayName;
                texts[1].text = data[i].remoteIP + ":" + data[i].port;

            }
        }else
        {
            Debug.Log("Unable To Fetch Servers");
        }

    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class YandexTimeAPI
{
	const string syncUrl = "https://yandex.com/time/sync.json";

	public static IEnumerator LoadSync()
	{
		using (UnityWebRequest request = UnityWebRequest.Get(syncUrl))
		{
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.ProtocolError ||
				request.result == UnityWebRequest.Result.ConnectionError)
				Debug.LogError(request.error);
			else
			{
				Debug.Log("Loaded:\n" + request.downloadHandler.text);
				var sync = YandexTimeSync.CreateFromJSON(request.downloadHandler.text);
				Debug.Log("time:\n" + sync.time);
				TimeManager.Instance.SetTime(sync.Date);
			}
		}
	}
}

using System;
using UnityEngine;

[Serializable]
public class YandexTimeSync
{
	public long time;
	public DateTime Date => new DateTime(1970, 1, 1).AddMilliseconds(time);

	public static YandexTimeSync CreateFromJSON(string json)
	{
		return JsonUtility.FromJson<YandexTimeSync>(json);
	}
	//1726913461521
	//2147483647
}

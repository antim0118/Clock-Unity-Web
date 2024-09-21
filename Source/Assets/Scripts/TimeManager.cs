using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [SerializeField] RectTransform HoursArrow, MinutesArrow, SecondsArrow;
    [SerializeField] InputField TimeText;
    [SerializeField] float ClockSpeed = 1.0f;

	GraphicRaycaster m_Raycaster;
	PointerEventData m_PointerEventData;
	EventSystem m_EventSystem;
    RaycastResult raycastResult;

	DateTime time;

    void Awake() => Instance = this;

	// Start is called before the first frame update
	void Start()
    {
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
		m_EventSystem = FindObjectOfType<EventSystem>();

		StartCoroutine(YandexTimeAPI.LoadSync());
    }

    float updateCounter = 1.0f;
    DateTime lastCheckTime = DateTime.Now;
    bool isEditing = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && raycastResult.gameObject == null)
        {
			m_PointerEventData = new PointerEventData(m_EventSystem);
			m_PointerEventData.position = Input.mousePosition;
			List<RaycastResult> results = new List<RaycastResult>();
			m_Raycaster.Raycast(m_PointerEventData, results);

			RaycastResult result = results.FirstOrDefault(rr => rr.gameObject.tag == "Raycast");
			if (result.gameObject != null)
				raycastResult = result;
		}
        else if (Input.GetKey(KeyCode.Mouse0) && raycastResult.gameObject != null)
        {
            DateTime oldTime = time;
            float value = 0;
            if (raycastResult.gameObject.name == "Hours")
            {
                value = ArrowToValue(HoursArrow, 12);
                time = new DateTime(time.Year, time.Month, time.Day, (int)value, time.Minute, time.Second);
            }
            else if (raycastResult.gameObject.name == "Minutes")
            {
                value = ArrowToValue(MinutesArrow, 60);
				time = new DateTime(time.Year, time.Month, time.Day, time.Hour, (int)value, time.Second);
                Debug.Log($"old={oldTime.Minute}; new={time.Minute}");
                if (oldTime.Minute >= 58 && time.Minute <= 3)
                    time.AddHours(1);
			}
		}
        else
            raycastResult = new RaycastResult();


		if (!TimeText.isFocused)
        {
            updateCounter += Time.deltaTime * ClockSpeed * 30.0f;
            if (updateCounter >= 1.0f)
            {
                updateCounter = 0.0f;
                if (raycastResult.gameObject == null)
                    time = time.AddSeconds(1 / 30f);

                TimeText.text = time.ToString("T");

                TimeSpan ts = new TimeSpan(time.Ticks);
                HoursArrow.localEulerAngles = ValueToAngle(ts.TotalHours, 12);
                MinutesArrow.localEulerAngles = ValueToAngle(ts.TotalMinutes, 60);
                SecondsArrow.localEulerAngles = ValueToAngle(ts.TotalSeconds, 60);
            }

            if ((DateTime.Now - lastCheckTime).TotalHours >= 1f)
            {
                lastCheckTime = DateTime.Now;
                StartCoroutine(YandexTimeAPI.LoadSync());
            }
        }
	}

    Vector3 ValueToAngle(double value, double maxValue)
    {
        return new Vector3(0, 0, -(float)(value / maxValue * 360.0 % 360));
	}

    float AngleToValue(double angle, double maxValue)
    {
        return (float)(maxValue - (angle / 360.0 * maxValue));
    }

    float ArrowToValue(RectTransform arrow, double maxValue)
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 arrowPos = arrow.transform.position;
        float angle = -Vector2.SignedAngle(arrowPos - mousePos, Vector2.up) + 180f;
        float value = AngleToValue(angle, maxValue);
        return value;
    }

    public void SetTime(DateTime time)
    {
        this.time = time;
        Debug.Log($"got {time.ToString()}");
    }

	public void EndEditTime(string newTime)
    {
        DateTime dt;
        if (DateTime.TryParseExact(newTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
            this.time = dt;
        else
            Debug.LogWarning("Failed parsing: " + newTime);
	}
}

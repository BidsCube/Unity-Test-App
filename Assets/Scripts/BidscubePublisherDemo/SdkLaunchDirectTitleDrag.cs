using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Moves the white Direct SDK card when dragging its title (raycast on TMP).</summary>
sealed class SdkLaunchDirectTitleDrag : MonoBehaviour, IDragHandler
{
    RectTransform _panelRt;
    Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        Transform t = transform;
        while (t != null)
        {
            if (t.name == "DirectSdkPanel")
            {
                _panelRt = t as RectTransform;
                break;
            }
            t = t.parent;
        }

        if (_panelRt == null)
            _panelRt = transform.parent as RectTransform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_panelRt == null)
            return;
        var c = _canvas != null ? _canvas : GetComponentInParent<Canvas>();
        float s = c != null ? c.scaleFactor : 1f;
        _panelRt.anchoredPosition += eventData.delta / s;
    }
}

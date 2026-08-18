using UnityEngine;
using UnityEngine.UI;

public class TitleUIView : ITitleUIView
{
    private GameObject[] _linkedUIObjects;

    public TitleUIView(GameObject[] linkedUIObjects)
    {
        _linkedUIObjects = linkedUIObjects;
    }

    public void HighlightButton(int index)
    {
        for (int i = 0; i < _linkedUIObjects.Length; i++)
        {
            var button = _linkedUIObjects[i].GetComponent<Button>();
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = (i == index) ? Color.yellow : Color.white;
                button.colors = colors;
            }
            SetChildImageActive(i, i == index);
        }
    }


    public GameObject GetSelectedButton(int index)
    {
        if (index >= 0 && index < _linkedUIObjects.Length)
        {
            return _linkedUIObjects[index];
        }
        return null;
    }

    public void SetChildImageActive(int index, bool isActive)
    {
        if (index >= 0 && index < _linkedUIObjects.Length)
        {
            var childImages = _linkedUIObjects[index].GetComponentsInChildren<Image>(true);
            foreach (var childImage in childImages)
            {
                if (childImage.gameObject != _linkedUIObjects[index]) 
                {
                    childImage.gameObject.SetActive(isActive);
                }
            }
        }
    }
}

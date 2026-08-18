using UnityEngine;

public interface ITitleUIView
{
    void HighlightButton(int index);
    GameObject GetSelectedButton(int index);
    void SetChildImageActive(int index, bool isActive);
}

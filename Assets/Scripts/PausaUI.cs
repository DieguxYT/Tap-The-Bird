using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class PausaUI : UIWindow
{
     #region Test Methods

    [Button("Test Show")]
    private void TestShow()
    {
        Show();
    }

    [Button("Test Hide")]
    private void TestHide()
    {
        Hide();
    }


    #endregion
}

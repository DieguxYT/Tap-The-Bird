using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class DerrotaUI : UIWindow
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
